using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public interface IS300PurchaseSvc
    {
        Task<Result<PurchSalesWrapper, string>> FetchETRInvoices();
        Task<Result<Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor, string>> GetAPVendor(SageDocFilter sageFilter);
        Task<Result<PagedResult<PurchTransact>, string>> GetPurchases(PurchaseFilter filter);
        Task<Result<PurchTransact, string>> AcceptPurchaseTrx(PurchTransact purchTransact);
    }
    public class S300PurchaseSvc : IS300PurchaseSvc
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IDatabase _baseDb;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExtSystConfig _extSystConfig;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IEtimsService _etimsService;

        private readonly Dictionary<string, SyncChannel> _syncChannelMap;
        private readonly ClientBranch _clientBranch = null;

        public S300PurchaseSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, 
            ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IEtimsService etimsService)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _httpClientFactory = httpClientFactory;
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;

            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
            _etimsService = etimsService;
        }

        public async Task<Result<PagedResult<PurchTransact>,string>> GetPurchases(PurchaseFilter filter)
        {
            string _method_ = "GetPurchases";
            PagedResult<PurchTransact> result = new();
            try
            {
                var query = _dbContext.PurchTransact.AsQueryable();
                if (filter != null && filter.RecordGroup != RecordStatusGroup.ALL)
                {
                    if (filter.RecordGroup == RecordStatusGroup.FAILED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_FAIL ||
                            f.RecordStatus == RecordStatus.INVALID);
                    else if (filter.RecordGroup == RecordStatusGroup.SUCCESSFUL)
                        query = query.Where(f => f.RecordStatus == RecordStatus.POST_OK ||
                            f.RecordStatus == RecordStatus.POST_DUPL);
                    else if (filter.RecordGroup == RecordStatusGroup.QUEUED)
                        query = query.Where(f => f.RecordStatus == RecordStatus.QUEUEDOUT ||
                            f.RecordStatus == RecordStatus.MANUALOUT);
                }

                if (!string.IsNullOrWhiteSpace(filter.DocNumber))
                    query = query.Where(x => x.DocNumber.Equals(filter.DocNumber));

                if (filter.HasAnyDate())
                {
                    string _dtFilterError = filter.GetDatesError();
                    if (!string.IsNullOrWhiteSpace(_dtFilterError))
                    {
                        return _dtFilterError;
                    }
                    query = query.Where(x => x.DocStamp >= filter.StartTime.Value
                        && x.DocStamp <= filter.EndTime.Value);
                }

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
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

                //TODO: remove this hack
                //DateTime trackerDate = syncChannel.GetMinDate();
                DateTime trackerDate = new DateTime(2020, 1, 1);
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

                            await _dbTrans.CommitAsync();
                            _dbContext.ChangeTracker.Clear();
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

                if (!await _masterDataSvc.SaveSyncChannel(syncChannel))
                {
                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                }
                if (!await _masterDataSvc.UpdateBranchAsync(_clientBranch))
                {
                    UI.Error($"{_method_} - UpdateBranchAsync : Failed Updating ClientBranch Details");
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }

            return result;
        }

        public async Task<Result<Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor, string>> GetAPVendor(SageDocFilter sageFilter)
        {
            string _method_ = "GetAPVendor";
            string _strError = null;
            Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor result = null;
            try
            {
                if (sageFilter == null || !sageFilter.IsValid)
                {
                    _strError = $"Invalid Vendor Number: {sageFilter.docNumber}";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }

                var qParams = new Dictionary<string, string>();
                string strFilter = sageFilter.GetFilterString();
                if (!string.IsNullOrWhiteSpace(strFilter))
                    qParams["$filter"] = strFilter;

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AP/APVendors");
                var vendorList = await client.ProcessGetReqBasicAsync<APVendors>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (vendorList == null && !vendorList.Vendors.Any())
                {
                    _strError = $"Not Found APVendors response from Sage for VendorNumber {sageFilter.docNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                result = vendorList.Vendors.FirstOrDefault();
                if (result == null)
                {
                    _strError = $"Missing APVendors response from Results for VendorNumber {sageFilter.docNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
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
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.PurchTransact.Where(e => e.PurchaseID == purchTransact.PurchaseID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.Remark, _strError)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.PurchTransact.Where(e => e.PurchaseID == purchTransact.PurchaseID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, purchaseSaveResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
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

    }
}
