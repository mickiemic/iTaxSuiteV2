using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public class S300TimsPurchaseSvc : S300BasePurchaseSvc, IS300PurchaseSvc
    {
        private readonly IEtimsService _etimsService;

        public S300TimsPurchaseSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, 
            ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IEtimsService etimsService)
            : base(dbContext, multiplexer, extSystConfig, masterDataSvc, httpClientFactory)
        {
            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();

            _etimsService = etimsService;
        }
        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.VSCU;
        }

        public async Task<Result<PurchSalesWrapper,string>> FetchETRInvoices()
        {
            string _method_ = "FetchETRInvoices";
            PurchSalesWrapper result = null;
            string _strError = string.Empty;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.PO_INVOICE_SYNC];
                var purchaseSet = await _dbContext.PurchTransact.Select(e => e.Reference).ToHashSetAsync();

                var trackerDate = syncChannel.GetMinDate();
                string lastReqDate = trackerDate.ToString(ETIMSConst.FMT_DATETIME);

                var selectRes = await _etimsService.SelectPurchaseSales(lastReqDate);
                if (selectRes.IsError)
                {
                    _strError = selectRes.GetError();
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                TrnsPurchaseSalesResp trnsPurchaseSales = selectRes.GetValue();
                if (!trnsPurchaseSales.HasData())
                {
                    _strError = $"The ETR Purchases list response has no purchases. {trnsPurchaseSales.RawResponse}";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                result = trnsPurchaseSales.PurchSalesData;

                result.PurchSalesList.RemoveAll(i => purchaseSet.Contains(i.Reference));

                foreach(var purchaseSale in result.PurchSalesList)
                {
                    var trnsPurchaseSaveReq = new TrnsPurchaseSaveReq(_clientBranch, purchaseSale);
                    var purchaseTrx = new PurchTransact(_clientBranch, purchaseSale);

                    var purchTrxData = new PurchTrxData(purchaseTrx, trnsPurchaseSaveReq, purchaseSale);
                    purchaseTrx.PurchTrxData = purchTrxData;

                    using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                    {
                        int _etrSeqValue = _clientBranch.PurchInvoiceSeq;
                        try
                        {
                            if (_dbContext.PurchTransact.AddIfNotExists(purchaseTrx, p => p.Reference == purchaseTrx.Reference) == null)
                            {
                                UI.Warn($"Purchase Reference {purchaseTrx.Reference} Already Exists");
                                continue;
                            }
                            _dbContext.Attach(_clientBranch);
                            if (_dbContext.SaveChanges() < 1)
                            {
                                throw new Exception($"Purchase Reference {purchaseTrx.Reference} saving to database failed");
                            }

                            if (purchaseTrx.DocStamp > trackerDate)
                                trackerDate = purchaseTrx.DocStamp;

                            syncChannel.UpdateTracker(purchaseTrx.DocNumber);
                            syncChannel.UpdateTracker(trackerDate);
                            _clientBranch.PurchInvoiceSeq = (_etrSeqValue + 1);

                            if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                            {
                                throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                            }
                            if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                            {
                                UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                            }

                            await _dbTrans.CommitAsync();
                            _dbContext.ChangeTracker.Clear();

                            await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                        }
                        catch (Exception iex)
                        {
                            await _dbTrans.RollbackAsync();
                            _dbContext.ChangeTracker.Clear();
                            _clientBranch.PurchInvoiceSeq = _etrSeqValue;
                            UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                            continue;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return result;
        }

        public async Task<Result<PurchTransact, string>> AcceptPurchaseTrx(PurchTransact purchTransact)
        {
            string _method_ = "AcceptPurchaseTrx";
            string _strError = string.Empty;
            try
            {
                UI.Info($">> {_method_} : {JsonConvert.SerializeObject(purchTransact)}");

                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL };
                var _dbTrx =  await _dbContext.PurchTransact.Include(x => x.PurchTrxData)
                    .Where(x => x.PurchaseID == purchTransact.PurchaseID && !completeStatii.Contains(x.RecordStatus))
                    .OrderBy(e => e.CreatedOn).AsNoTracking().FirstOrDefaultAsync();
                if (_dbTrx == null)
                {
                    _strError = $"No PurchTransact {purchTransact.CacheKey} available for processing";
                    UI.Error(_strError);
                    return _strError;
                }
                purchTransact = _dbTrx;

                var trnsPurchaseSaveReq = purchTransact.PurchTrxData.GetEtimsRequest();
                var purchSaveResp = await _etimsService.SaveEtimsPurchase(trnsPurchaseSaveReq);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        var recordStatus = RecordStatus.POST_FAIL;

                        if (purchSaveResp.IsError)
                        {
                            _strError = $"PurchTransact {purchTransact.CacheKey} failed saving EtimsPurchase request: {purchSaveResp.GetError()}";
                            UI.Error(_strError);

                            // Update & Save Transact Changes
                            await _dbContext.PurchTrxData.Where(e => e.PurchaseID == purchTransact.PurchaseID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            await _dbContext.PurchTransact.Where(e => e.PurchaseID == purchTransact.PurchaseID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );

                            await _dbTrans.CommitAsync();
                            return _strError;
                        }

                        var purchaseSaveResp = purchSaveResp.GetValue();
                        if (purchSaveResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

                        await _dbContext.PurchTrxData.Where(e => e.PurchaseID == purchTransact.PurchaseID).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, purchaseSaveResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbContext.PurchTransact.Where(e => e.PurchaseID == purchTransact.PurchaseID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, purchaseSaveResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        await _dbTrans.CommitAsync();
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return purchTransact;
        }

        public async Task<Result<TrnsPurchaseSalesResp, string>> ImportPurchases(DateTime? filter)
        {
            string _method_ = "ImportPurchases";
            try
            {
                DateTime filterDate = filter ?? DateTime.Today;
                string lastReqDt = filterDate.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectPurchaseSales(lastReqDt);
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }
        }

    }
}
