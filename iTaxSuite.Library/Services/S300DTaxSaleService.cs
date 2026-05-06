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
    public class S300DTaxSaleService : S300BaseSaleService, IS300SaleService
    {
        private readonly IDigiTaxService _dTaxService;
        private readonly bool FixMultiLine = true;

        public S300DTaxSaleService(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory,
            ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IDigiTaxService dTaxService)
            : base(dbContext, multiplexer, extSystConfig, masterDataSvc, httpClientFactory)
        {
            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();

            _dTaxService = dTaxService;
        }

        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.DIGITAX;
        }

        public async Task<Result<List<EtimsSalesView>, string>> FetchARInvoices()
        {
            string _method_ = "FetchARInvoices";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            List<EtimsSalesView> results = new();
            try
            {
                UI.Debug($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.AR_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "AR" && e.DocType == DocumentType.INVOICE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARInvoiceBatches");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("BatchStatus eq 'Posted' and SourceApplication eq 'AR' and {0} ge {1}Z",
                    syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                bool loop = true;
                int _invoiceCount = 0;
                decimal _lastInvBatch = -1;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    //TODO: decide on how many to take at a go
                    var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username,
                        _extSystConfig.Password, null, qParams);
                    if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                    {
                        _strError = $"Not Found ARInvoices response from Sage";
                        UI.Debug($"{_method_} error : {_strError}");
                        return results;
                    }

                    foreach (var invBatch in invoiceBatches.InvoiceBatches)
                    {
                        var invoices = invBatch.Invoices.Where(x => x.DocumentType ==
                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice).ToList();
                        if (invoices == null || invoices.Count == 0)
                        {
                            _strError = $"ARInvoices BatchNumber {invBatch.BatchNumber} has no Invoices";
                            UI.Warn($"{_method_} error : {_strError}");

                            // update sync counter and continue
                            syncChannel.UpdateTracker(invBatch.BatchNumber.ToString());
                            syncChannel.IncrOffSet(1);
                            if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                            {
                                UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                            }
                            int changes = await _dbContext.SaveChangesAsync();
                            if (changes < 1)
                            {
                                throw new Exception($"ARCRNote:[{invBatch.BatchNumber}]  saving to database failed");
                            }
                            _dbContext.ChangeTracker.Clear();
                            await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);

                            continue;
                        }

                        foreach (var invoice in invoices)
                        {
                            if (_lastInvBatch != invoice.BatchNumber)
                            {
                                syncChannel.UpdateTracker(invoice.BatchNumber.ToString());
                                syncChannel.IncrOffSet(1);

                                if (_invoiceCount >= 100)
                                {
                                    loop = false;
                                    break;
                                }
                            }
                            _lastInvBatch = invoice.BatchNumber;
                            
                            string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingCurrencyCode}:Sales";
                            if (!taxGroupMap.ContainsKey(strTaxKey))
                            {
                                _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                                UI.Error($"{_method_} error : {_strError}");
                                return _strError;
                            }
                            var _taxGroup = taxGroupMap[strTaxKey];

                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                            var sageFilter = new SageDocFilter()
                            {
                                docKey = "CustomerNumber",
                                docNumber = invoice.CustomerNumber
                            };
                            var sCustomer = await GetARCustomer(sageFilter);
                            if (sCustomer.IsError)
                            {
                                _strError = sCustomer.GetError();
                                UI.Error($"{_method_} error : {_strError}");
                                return _strError;
                            }
                            _customer = sCustomer.GetValue();

                            var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                            var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, true);
                            if (mapResult.IsError)
                            {
                                _strError = mapResult.GetError();
                                UI.Error($"{_method_} ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] MapSalesInvcAttribs error : {_strError}");
                                arSaleTrx.RecordStatus = RecordStatus.INVALID;
                                arSaleTrx.Remark = _strError;
                            }
                            else
                            {
                                arSaleTrx = mapResult.GetValue();
                            }

                            #region Product-Duplicity workaround
                            if (FixMultiLine && arSaleTrx.SalesItems.Count > 1)
                            {
                                var itemMap = new Dictionary<string, int>();
                                foreach (var item in arSaleTrx.SalesItems)
                                {
                                    if (itemMap.ContainsKey(item.ProductCode))
                                    {
                                        itemMap[item.ProductCode]++;
                                    }
                                    else
                                    {
                                        itemMap.Add(item.ProductCode, 1);
                                    }
                                }
                                foreach (var kv in itemMap.Where(x => x.Value > 1))
                                {
                                    var _amtSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                                    var _unitSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                                    var _qtySum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                                    var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                                    var finalItem = arSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                                    finalItem.UnitPrice = _avgUnitPrice;
                                    finalItem.Quantity = finalItem.Package = _qtySum;
                                    finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                                    arSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                                    arSaleTrx.SalesItems.Add(finalItem);
                                }
                            }
                            #endregion

                            var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, arSaleTrx, _taxGroup, taxAuthKeys, _customer);
                            UI.Info($"<< ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");

                            if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                                arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else
                                arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                            var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveSaleReq, invoice);
                            arSaleTrx.SalesTrxData = salesTrxData;
                            UI.Info($"<< ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                            using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                            {
                                int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                                try
                                {
                                    if (_dbContext.SalesTransact.AddIfNotExists(arSaleTrx, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}]  Already Exists");
                                        continue;
                                    }
                                    _dbContext.Attach(_clientBranch);
                                    if (_dbContext.SaveChanges() < 1)
                                    {
                                        throw new Exception($"ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}]  saving to database failed");
                                    }
                                    
                                    _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                    if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                    {
                                        throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                    }
                                    if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                    {
                                        UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                                    }

                                    int changes = await _dbContext.SaveChangesAsync();
                                    if (changes < 1)
                                    {
                                        throw new Exception($"ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}]  saving to database failed");
                                    }

                                    await _dbTrans.CommitAsync();
                                    _dbContext.ChangeTracker.Clear();

                                    await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                                }
                                catch (Exception iex)
                                {
                                    await _dbTrans.RollbackAsync();
                                    _dbContext.ChangeTracker.Clear();
                                    _clientBranch.SaleInvoiceSeq = _etrSeqValue;
                                    UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                    continue;
                                }
                            }

                            var salesView = new EtimsSalesView
                            {
                                SalesTransact = arSaleTrx,
                                DTaxSaveSale = dTaxSaveSaleReq
                            };
                            results.Add(salesView);

                            _invoiceCount++;
                        }
                    }

                    if (!loop)
                        break;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return results;
        }
        public async Task<Result<List<EtimsSalesView>, string>> FetchARCRNotes()
        {
            string _method_ = "FetchARCRNotes";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            List<EtimsSalesView> results = new();
            try
            {
                UI.Debug($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.AR_CRDRNOTE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "AR" && e.DocType == DocumentType.CREDITNOTE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARInvoiceBatches");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("BatchStatus eq 'Posted' and SourceApplication eq 'AR' and {0} ge {1}Z",
                    syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                bool loop = true;
                int _invoiceCount = 0;
                decimal _lastInvBatch = -1;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    //TODO: decide on how many to take at a go
                    var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username,
                        _extSystConfig.Password, null, qParams);
                    if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                    {
                        _strError = $"Not Found ARInvoices response from Sage";
                        UI.Debug($"{_method_} error : {_strError}");
                        return results;
                    }

                    foreach (var invBatch in invoiceBatches.InvoiceBatches)
                    {
                        var arCRNotes = invBatch.Invoices.Where(x => x.DocumentType ==
                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote).ToList();
                        if (arCRNotes == null || arCRNotes.Count == 0)
                        {
                            _strError = $"ARInvoices BatchNumber {invBatch.BatchNumber} has no CreditNotes";
                            UI.Warn($"{_method_} warning : {_strError}");

                            // update sync counter and continue
                            syncChannel.UpdateTracker(invBatch.BatchNumber.ToString());
                            syncChannel.IncrOffSet(1);
                            if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                            {
                                UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                            }
                            int changes = await _dbContext.SaveChangesAsync();
                            if (changes < 1)
                            {
                                throw new Exception($"ARCRNote:[{invBatch.BatchNumber}]  saving to database failed");
                            }
                            _dbContext.ChangeTracker.Clear();
                            await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);

                            continue;
                        }

                        foreach(var crNote in arCRNotes)
                        {
                            if (_lastInvBatch != crNote.BatchNumber)
                            {
                                syncChannel.UpdateTracker(crNote.BatchNumber.ToString());
                                syncChannel.IncrOffSet(1);

                                if (_invoiceCount >= 100)
                                {
                                    loop = false;
                                    break;
                                }
                            }
                            _lastInvBatch = crNote.BatchNumber;

                            string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingCurrencyCode}:Sales";
                            if (!taxGroupMap.ContainsKey(strTaxKey))
                            {
                                _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                                UI.Error($"{_method_} error : {_strError}");
                                return _strError;
                            }
                            var _taxGroup = taxGroupMap[strTaxKey];

                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                            var sageFilter = new SageDocFilter()
                            {
                                docKey = "CustomerNumber",
                                docNumber = crNote.CustomerNumber
                            };
                            var sCustomer = await GetARCustomer(sageFilter);
                            if (sCustomer.IsError)
                            {
                                _strError = sCustomer.GetError();
                                UI.Error($"{_method_} error : {_strError}");
                                return _strError;
                            }
                            _customer = sCustomer.GetValue();

                            var arSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                            var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, true);
                            if (mapResult.IsError)
                            {
                                _strError = mapResult.GetError();
                                UI.Error($"{_method_} ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] MapSalesInvcAttribs error : {_strError}");
                                arSaleTrx.RecordStatus = RecordStatus.INVALID;
                                arSaleTrx.Remark = _strError;
                            }
                            else
                            {
                                arSaleTrx = mapResult.GetValue();
                            }

                            var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.ApplytoDocument);
                            if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                            {
                                _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.ApplytoDocument} for ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]";
                                UI.Error($"{_method_} error : {_strError}");
                                arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                arSaleTrx.Remark = _strError;
                            }

                            #region Product-Duplicity workaround
                            if (FixMultiLine && arSaleTrx.SalesItems.Count > 1)
                            {
                                var itemMap = new Dictionary<string, int>();
                                foreach (var item in arSaleTrx.SalesItems)
                                {
                                    if (itemMap.ContainsKey(item.ProductCode))
                                    {
                                        itemMap[item.ProductCode]++;
                                    }
                                    else
                                    {
                                        itemMap.Add(item.ProductCode, 1);
                                    }
                                }
                                foreach (var kv in itemMap.Where(x => x.Value > 1))
                                {
                                    var _amtSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                                    var _unitSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                                    var _qtySum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                                    var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                                    var finalItem = arSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                                    finalItem.UnitPrice = _avgUnitPrice;
                                    finalItem.Quantity = finalItem.Package = _qtySum;
                                    finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                                    arSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                                    arSaleTrx.SalesItems.Add(finalItem);
                                }
                            }
                            #endregion

                            var dTaxSaveCNoteReq = new DTaxSaveCNoteReq(_clientBranch, crNote, arSaleTrx, origInvoice, _taxGroup, taxAuthKeys, _customer);
                            UI.Info($"<< ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");

                            if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                                arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else
                                arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                            var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveCNoteReq, crNote);
                            arSaleTrx.SalesTrxData = salesTrxData;
                            UI.Info($"<< ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                            using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                            {
                                int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                                try
                                {
                                    if (_dbContext.SalesTransact.AddIfNotExists(arSaleTrx, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]  Already Exists");
                                        continue;
                                    }
                                    _dbContext.Attach(_clientBranch);
                                    if (_dbContext.SaveChanges() < 1)
                                    {
                                        throw new Exception($"ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]  saving to database failed");
                                    }

                                    _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                    if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                    {
                                        throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                    }
                                    if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                    {
                                        UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                                    }

                                    int changes = await _dbContext.SaveChangesAsync();
                                    if (changes < 1)
                                    {
                                        throw new Exception($"ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]  saving to database failed");
                                    }

                                    await _dbTrans.CommitAsync();
                                    _dbContext.ChangeTracker.Clear();

                                    await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                                }
                                catch (Exception iex)
                                {
                                    await _dbTrans.RollbackAsync();
                                    _dbContext.ChangeTracker.Clear();
                                    _clientBranch.SaleInvoiceSeq = _etrSeqValue;
                                    UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                    continue;
                                }
                            }

                            var salesView = new EtimsSalesView
                            {
                                SalesTransact = arSaleTrx,
                                DTaxSaveCNoteReq = dTaxSaveCNoteReq
                            };
                            results.Add(salesView);

                            _invoiceCount++;
                        }
                    }

                    if (!loop)
                        break;
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return results;
        }

        public async Task<Result<List<EtimsSalesView>, string>> FetchOEInvoices()
        {
            string _method_ = "FetchOEInvoices";
            List<EtimsSalesView> results = new();
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            var decimalFormat = new DecimalFormatConverter();
            HashSet<string> taxAuthKeys = null;
            try
            {
                UI.Debug($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE" && e.DocType == DocumentType.INVOICE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OEInvoices");

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();

                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var invList = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (invList == null || invList.Invoices.Count == 0)
                    {
                        _strError = "Null OEInvoices response from Sage";
                        UI.Debug($"{_method_} : {_strError}");
                        return results;
                    }
                    loop = (invList.nextLink != null);
                    syncChannel.IncrOffSet(invList.Invoices.Count);

                    invList.Invoices.RemoveAll(i => invoiceMap.ContainsKey(i.InvoiceNumber));

                    foreach(var invoice in invList.Invoices)
                    {
                        // Sort Tax Group
                        string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                        if (!taxGroupMap.ContainsKey(strTaxKey))
                        {
                            _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                            UI.Error($"{_method_} error : {_strError}");
                            return _strError;
                        }
                        var _taxGroup = taxGroupMap[strTaxKey];

                        // Get Customer Details
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                        var sageFilter = new SageDocFilter()
                        {
                            docKey = "CustomerNumber",
                            docNumber = invoice.CustomerNumber
                        };
                        var sCustomer = await GetARCustomer(sageFilter);
                        if (sCustomer.IsError)
                        {
                            _strError = sCustomer.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            return _strError;
                        }
                        _customer = sCustomer.GetValue();

                        var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                        var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} OEInvoice:{invoice.InvoiceNumber}, MapSalesInvcAttribs error : {_strError}");
                            oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                            oeSaleTrx.Remark = _strError;
                        }
                        else
                        {
                            oeSaleTrx = mapResult.GetValue();
                        }

                        #region Product-Duplicity workaround
                        if (FixMultiLine && oeSaleTrx.SalesItems.Count > 1)
                        {
                            var itemMap = new Dictionary<string, int>();
                            foreach (var item in oeSaleTrx.SalesItems)
                            {
                                if (itemMap.ContainsKey(item.ProductCode))
                                {
                                    itemMap[item.ProductCode]++;
                                }
                                else
                                {
                                    itemMap.Add(item.ProductCode, 1);
                                }
                            }
                            foreach (var kv in itemMap.Where(x => x.Value > 1))
                            {
                                var _amtSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                                var _unitSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                                var _qtySum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                                var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                                var finalItem = oeSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                                finalItem.UnitPrice = _avgUnitPrice;
                                finalItem.Quantity = finalItem.Package = _qtySum;
                                finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                                oeSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                                oeSaleTrx.SalesItems.Add(finalItem);
                            }
                        }
                        #endregion

                        var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                        UI.Info($"<< {oeSaleTrx.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                        if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                            oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                        else
                            oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                        var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveSaleReq, invoice);
                        oeSaleTrx.SalesTrxData = salesTrxData;
                        UI.Info($"<< {oeSaleTrx.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                            try
                            {
                                if (_dbContext.SalesTransact.AddIfNotExists(oeSaleTrx, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn($"OEInvoice {oeSaleTrx.DocNumber} Already Exists");
                                    continue;
                                }
                                _dbContext.Attach(_clientBranch);
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"OEInvoice {oeSaleTrx.DocNumber} saving to database failed");
                                }

                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                                }

                                int changes = await _dbContext.SaveChangesAsync();
                                if (changes < 1)
                                {
                                    throw new Exception($"OEInvoice {oeSaleTrx.DocNumber} saving to database failed");
                                }

                                await _dbTrans.CommitAsync();
                                _dbContext.ChangeTracker.Clear();

                                await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                _dbContext.ChangeTracker.Clear();
                                _clientBranch.SaleInvoiceSeq = _etrSeqValue;
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                continue;
                            }
                        }

                        var salesView = new EtimsSalesView
                        {
                            SalesTransact = oeSaleTrx,
                            DTaxSaveSale = dTaxSaveSaleReq
                        };
                        results.Add(salesView);
                    }

                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return results;
        }

        public async Task<Result<List<EtimsSalesView>, string>> FetchOECRDRNotes()
        {
            string _method_ = "FetchOECRDRNotes";
            List<EtimsSalesView> results = new();
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            var decimalFormat = new DecimalFormatConverter();
            HashSet<string> taxAuthKeys = null;
            try
            {
                UI.Info($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.OE_CRDRNOTE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE" && e.DocType == DocumentType.CREDITNOTE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OECreditDebitNotes");

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();

                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var crNoteList = await client.ProcessGetReqBasicAsync<OECreditDebitNotes>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (crNoteList == null || crNoteList.CreditDebitNotes.Count == 0)
                    {
                        _strError = "Null OECreditDebitNotes response from Sage";
                        UI.Debug($"{_method_} : {_strError}");
                        return results;
                    }
                    loop = (crNoteList.nextLink != null);
                    syncChannel.IncrOffSet(crNoteList.CreditDebitNotes.Count);

                    crNoteList.CreditDebitNotes.RemoveAll(i => invoiceMap.ContainsKey(i.CreditDebitNoteNumber));
                    foreach (var crNote in crNoteList.CreditDebitNotes)
                    {
                        // Sort Tax Group
                        string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingTRCurrency}:Sales";
                        if (!taxGroupMap.ContainsKey(strTaxKey))
                        {
                            _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                            UI.Error($"{_method_} error : {_strError}");
                            return _strError;
                        }
                        var _taxGroup = taxGroupMap[strTaxKey];

                        // Get Customer Details
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                        var sageFilter = new SageDocFilter()
                        {
                            docKey = "CustomerNumber",
                            docNumber = crNote.CustomerNumber
                        };
                        var sCustomer = await GetARCustomer(sageFilter);
                        if (sCustomer.IsError)
                        {
                            _strError = sCustomer.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            return _strError;
                        }
                        _customer = sCustomer.GetValue();

                        var oeSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                        var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} OECreditDebitNote:{oeSaleTrx.DocNumber}, MapSalesInvcAttribs error : {_strError}");
                            oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                            oeSaleTrx.Remark = _strError;
                        }
                        else
                        {
                            oeSaleTrx = mapResult.GetValue();
                        }

                        var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.InvoiceNumber);
                        if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                        {
                            _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.InvoiceNumber} for OECreditDebitNote:{oeSaleTrx.DocNumber}";
                            UI.Error($"{_method_} error : {_strError}");
                            oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                            oeSaleTrx.Remark = _strError;
                        }

                        #region Product-Duplicity workaround
                        if (FixMultiLine && oeSaleTrx.SalesItems.Count > 1)
                        {
                            var itemMap = new Dictionary<string, int>();
                            foreach (var item in oeSaleTrx.SalesItems)
                            {
                                if (itemMap.ContainsKey(item.ProductCode))
                                {
                                    itemMap[item.ProductCode]++;
                                }
                                else
                                {
                                    itemMap.Add(item.ProductCode, 1);
                                }
                            }
                            foreach (var kv in itemMap.Where(x => x.Value > 1))
                            {
                                var _amtSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                                var _unitSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                                var _qtySum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                                var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                                var finalItem = oeSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                                finalItem.UnitPrice = _avgUnitPrice;
                                finalItem.Quantity = finalItem.Package = _qtySum;
                                finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                                oeSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                                oeSaleTrx.SalesItems.Add(finalItem);
                            }
                        }
                        #endregion

                        var dTaxSaveCNoteReq = new DTaxSaveCNoteReq(_clientBranch, crNote, oeSaleTrx, origInvoice, _taxGroup, taxAuthKeys, _customer);
                        UI.Info($"<< {oeSaleTrx.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");
                        if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                            oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                        else
                            oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                        var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveCNoteReq, crNote);
                        oeSaleTrx.SalesTrxData = salesTrxData;
                        UI.Info($"<< {oeSaleTrx.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                            try
                            {
                                if (_dbContext.SalesTransact.AddIfNotExists(oeSaleTrx, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn($"OEInvoice {oeSaleTrx.DocNumber} Already Exists");
                                    continue;
                                }
                                _dbContext.Attach(_clientBranch);
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"OEInvoice {oeSaleTrx.DocNumber} saving to database failed");
                                }

                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
                                }

                                int changes = await _dbContext.SaveChangesAsync();
                                if (changes < 1)
                                {
                                    throw new Exception($"OEInvoice {oeSaleTrx.DocNumber} saving to database failed");
                                }

                                await _dbTrans.CommitAsync();
                                _dbContext.ChangeTracker.Clear();

                                await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);
                            }
                            catch (Exception iex)
                            {
                                await _dbTrans.RollbackAsync();
                                _dbContext.ChangeTracker.Clear();
                                _clientBranch.SaleInvoiceSeq = _etrSeqValue;
                                UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                                continue;
                            }
                        }

                        var salesView = new EtimsSalesView
                        {
                            SalesTransact = oeSaleTrx,
                            DTaxSaveCNoteReq = dTaxSaveCNoteReq
                        };
                        results.Add(salesView);
                    }
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return results;
        }

        public async Task<Result<SalesTransact, string>> ReFetchOEInvoice(SaleTrxKey saleTrxKey)
        {
            string _method_ = "ReFetchOEInvoice";
            SalesTransact oeSaleTrx = null;
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                if (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for OEInvoice => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OEInvoices");

                oeSaleTrx = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
                if (oeSaleTrx is null)
                {
                    _strError = $"Invalid or missing OEInvoice {oeSaleTrx.DocNumber} in SalesTransact data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                // var salesTrxData = await _dbContext.SalesTrxData.FirstOrDefaultAsync(x => x.SalesTrxID == oeSaleTrx.SalesTrxID);
                if (oeSaleTrx.SalesTrxData is null)
                {
                    _strError = $"Invalid or missing OEInvoice {saleTrxKey.DocNumber} in SalesTrxData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var salesTrxData = oeSaleTrx.SalesTrxData;

                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    return _strError;
                }
                oeSaleTrx = mapResult.GetValue();

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"InvoiceNumber eq '{saleTrxKey.DocNumber}'";

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                var result = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                if (result == null || result.Invoices.Count == 0)
                {
                    _strError = $"Not Found OEInvoices response from Sage for InvoiceNumber {saleTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var invoice = result.Invoices.FirstOrDefault(i => i.InvoiceNumber == saleTrxKey.DocNumber);
                string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var _taxGroup = taxGroupMap[strTaxKey];

                Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                var sageFilter = new SageDocFilter()
                {
                    docKey = "CustomerNumber",
                    docNumber = invoice.CustomerNumber
                };
                var sCustomer = await GetARCustomer(sageFilter);
                if (sCustomer.IsError)
                {
                    _strError = sCustomer.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();

                var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                    oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var _oldSaveSalesRes = JsonConvert.DeserializeObject<TrnsSalesSaveReq>(salesTrxData.RequestPayload);
                if (!_oldSaveSalesRes.HasEqualValue(dTaxSaveSaleReq))
                {
                    salesTrxData.RequestPayload = JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
                    salesTrxData.UpdatedOn = DateTime.Now;
                    salesTrxData.UpdatedBy = "Sys-Admin";
                    int affected = await _dbContext.SaveChangesAsync();
                    UI.Info($"{_method_} update {affected} records updated.");
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return oeSaleTrx;
        }
        public async Task<Result<SalesTransact,string>> ReSyncTaxInvoice(SaleTrxKey saleTrxKey)
        {
            string _method_ = "ReSyncTaxInvoice";
            SalesTransact saleTransact = null;
            string _strError = string.Empty;
            try
            {
                if (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for OEInvoice => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
                if (saleTransact is null)
                {
                    _strError = $"Invalid or missing SalesTransact {saleTransact.DocNumber} in SalesTransact data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL };
                if (!completeStatii.Contains(saleTransact.RecordStatus))
                {
                    _strError = $"Invalid or status for SalesTransact DocNumber {saleTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                if (string.IsNullOrWhiteSpace(saleTransact.ExternalID))
                {
                    _strError = $"Invalid or missing SalesTransact DigiTax ID for DocNumber {saleTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                if (saleTransact.SalesTrxData is null)
                {
                    _strError = $"Invalid or missing SalesTransact {saleTrxKey.DocNumber} in SalesTrxData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                if (saleTransact.IsTaxComplete())
                {
                    _strError = $"SalesTransact {saleTrxKey.DocNumber} is already complete. No further action.";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var etimsRespSale = await _dTaxService.GetDTaxOneSale(saleTransact.ExternalID);
                if (etimsRespSale.IsError)
                {
                    _strError = etimsRespSale.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var saleTrxResp = etimsRespSale.GetValue();
                        var cuNumber = saleTrxResp.GetCUNumber(_clientBranch);
                        if (string.IsNullOrWhiteSpace(cuNumber))
                        {
                            _strError = $"No Valid CUNumber Generated for receipt: {saleTransact.SalesTrxID}";
                            UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                        }
                        byte[] qrData = null;
                        string qrText = saleTrxResp.GetQRText();
                        if (string.IsNullOrWhiteSpace(qrText))
                        {
                            _strError = $"No Valid QRText Generated for receipt: {saleTransact.SalesTrxID}";
                            UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                        }
                        else
                        {
                            qrData = FileBinUtils.GenerateQRCode(qrText);
                            if (qrData is null or [])
                            {
                                _strError = $"No Valid QRImage Generated for receipt: {saleTransact.SalesTrxID}";
                                UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                            }
                        }

                        var tStamp = DateTime.Now;
                        var _remark = $"{saleTrxResp.Status} on {tStamp.ToString("s")}";

                        int _dbChanges = await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        _dbChanges += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, RecordStatus.POST_OK)
                            .SetProperty(x => x.CUNumber, cuNumber)
                            .SetProperty(x => x.QRText, qrText)
                            .SetProperty(x => x.QRTime, tStamp)
                            .SetProperty(x => x.QRImage, qrData)
                            .SetProperty(x => x.SDCID, saleTrxResp.SerialNumber)
                            .SetProperty(x => x.InternalData, saleTrxResp.InternalData)
                            .SetProperty(x => x.ReceiptNumber, saleTrxResp.ReceiptNumber)
                            .SetProperty(x => x.ReceiptSignature, saleTrxResp.ReceiptSignature)
                            .SetProperty(x => x.ExternalURL, saleTrxResp.SaleDetailURL)
                            .SetProperty(x => x.OfflineURL, saleTrxResp.OfflineURL)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        await _dbTrans.CommitAsync();
                        if (_dbChanges > 0)
                        {
                            saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                            .FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
                        }
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} SaleTrxID: {saleTransact.SalesTrxID} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }

                return saleTransact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsSalesView, string>> GetConvertARInvoice(SaleBatchTrxKey saleBatchTrxKey)
        {
            string _method_ = "GetConvertARInvoice";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice = null;
            try
            {
                #region Sage300 section
                if (saleBatchTrxKey == null || string.IsNullOrWhiteSpace(saleBatchTrxKey.BatchNumber))
                {
                    _strError = $"Invalid filter for ARInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARInvoiceBatches");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"BatchStatus eq 'Posted' and SourceApplication eq 'AR' and BatchNumber eq {saleBatchTrxKey.BatchNumber}";
                //qParams["$filter"] = $"BatchNumber eq {saleBatchTrxKey.BatchNumber}";

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                {
                    _strError = $"Not Found ARInvoices response from Sage for BatchNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var invBatch = invoiceBatches.InvoiceBatches.FirstOrDefault();
                if (invBatch == null)
                {
                    _strError = $"Missing ARInvoices response from Results for BatchNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                if (string.IsNullOrWhiteSpace(saleBatchTrxKey.DocNumber))
                    invoice = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice);
                else
                    invoice = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice
                    && x.DocumentNumber == saleBatchTrxKey.DocNumber);
                if (invoice == null)
                {
                    _strError = $"ARInvoices BatchNumber {saleBatchTrxKey.DocNumber} has no Invoice with DocumentNumber:{saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingCurrencyCode}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var _taxGroup = taxGroupMap[strTaxKey];

                Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                var sageFilter = new SageDocFilter()
                {
                    docKey = "CustomerNumber",
                    docNumber = invoice.CustomerNumber
                };
                var sCustomer = await GetARCustomer(sageFilter);
                if (sCustomer.IsError)
                {
                    _strError = sCustomer.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, true);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    arSaleTrx.RecordStatus = RecordStatus.INVALID;
                    arSaleTrx.Remark = _strError;
                }
                else
                {
                    arSaleTrx = mapResult.GetValue();
                }

                #region Product-Duplicity workaround
                if (FixMultiLine && arSaleTrx.SalesItems.Count > 1)
                {
                    var itemMap = new Dictionary<string, int>();
                    foreach (var item in arSaleTrx.SalesItems)
                    {
                        if (itemMap.ContainsKey(item.ProductCode))
                        {
                            itemMap[item.ProductCode]++;
                        }
                        else
                        {
                            itemMap.Add(item.ProductCode, 1);
                        }
                    }
                    foreach (var kv in itemMap.Where(x => x.Value > 1))
                    {
                        var _amtSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                        var _unitSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                        var _qtySum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                        var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                        var finalItem = arSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                        finalItem.UnitPrice = _avgUnitPrice;
                        finalItem.Quantity = finalItem.Package = _qtySum;
                        finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                        arSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                        arSaleTrx.SalesItems.Add(finalItem);
                    }
                }
                #endregion

                var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, arSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleBatchTrxKey.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");

                if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                    arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveSaleReq, invoice);
                arSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = arSaleTrx,
                    DTaxSaveSale = dTaxSaveSaleReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsSalesView, string>> GetConvertARCRNote(SaleBatchTrxKey saleBatchTrxKey)
        {
            string _method_ = "GetConvertARCRNote";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice crNote = null;
            try
            {
                #region Sage300 section
                if (saleBatchTrxKey == null || string.IsNullOrWhiteSpace(saleBatchTrxKey.BatchNumber))
                {
                    _strError = $"Invalid filter for ARInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARInvoiceBatches");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"BatchStatus eq 'Posted' and SourceApplication eq 'AR' and BatchNumber eq {saleBatchTrxKey.BatchNumber}";
                //qParams["$filter"] = $"BatchNumber eq {saleBatchTrxKey.BatchNumber}";

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                {
                    _strError = $"Not Found ARInvoices response from Sage for BatchNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var invBatch = invoiceBatches.InvoiceBatches.FirstOrDefault();
                if (invBatch == null)
                {
                    _strError = $"Missing ARInvoices response from Results for BatchNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                if (string.IsNullOrWhiteSpace(saleBatchTrxKey.DocNumber))
                    crNote = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote);
                else
                    crNote = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote
                    && x.DocumentNumber == saleBatchTrxKey.DocNumber);
                if (crNote == null)
                {
                    _strError = $"ARInvoices BatchNumber {saleBatchTrxKey.DocNumber} has no CreditNotes with DocumentNumber:{saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingCurrencyCode}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var _taxGroup = taxGroupMap[strTaxKey];

                Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                var sageFilter = new SageDocFilter()
                {
                    docKey = "CustomerNumber",
                    docNumber = crNote.CustomerNumber
                };
                var sCustomer = await GetARCustomer(sageFilter);
                if (sCustomer.IsError)
                {
                    _strError = sCustomer.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, true);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    arSaleTrx.RecordStatus = RecordStatus.INVALID;
                    arSaleTrx.Remark = _strError;
                }
                else
                {
                    arSaleTrx = mapResult.GetValue();
                }

                var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.ApplytoDocument);
                if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                {
                    _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.ApplytoDocument} for OECRNote => {JsonConvert.SerializeObject(saleBatchTrxKey.DocNumber)}";
                    UI.Error($"{_method_} error : {_strError}");
                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                    arSaleTrx.Remark = _strError;
                }

                #region Product-Duplicity workaround
                if (FixMultiLine &&  arSaleTrx.SalesItems.Count > 1)
                {
                    var itemMap = new Dictionary<string, int>();
                    foreach (var item in arSaleTrx.SalesItems)
                    {
                        if (itemMap.ContainsKey(item.ProductCode))
                        {
                            itemMap[item.ProductCode]++;
                        }
                        else
                        {
                            itemMap.Add(item.ProductCode, 1);
                        }
                    }
                    foreach (var kv in itemMap.Where(x => x.Value > 1))
                    {
                        var _amtSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                        var _unitSum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                        var _qtySum = arSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                        var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                        var finalItem = arSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                        finalItem.UnitPrice = _avgUnitPrice;
                        finalItem.Quantity = finalItem.Package = _qtySum;
                        finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                        // arSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                        arSaleTrx.SalesItems.Add(finalItem);
                    }
                }
                #endregion

                var dTaxSaveCNoteReq = new DTaxSaveCNoteReq(_clientBranch, crNote, arSaleTrx, origInvoice, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleBatchTrxKey.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");

                if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                    arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveCNoteReq, crNote);
                arSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = arSaleTrx,
                    DTaxSaveCNoteReq = dTaxSaveCNoteReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsSalesView, string>> GetConvertOEInvoice(SaleTrxKey saleTrxKey, string srcPayload = null)
        {
            string _method_ = "GetConvertOEInvoice";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice = null;

                #region Sage300 section
                if (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for OEInvoice => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OEInvoices");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"InvoiceNumber eq '{saleTrxKey.DocNumber}'";

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                if (!string.IsNullOrWhiteSpace(srcPayload))
                {
                    invoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice>(srcPayload);
                }
                else
                {
                    var result = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null || result.Invoices.Count == 0)
                    {
                        _strError = $"Not Found OEInvoices response from Sage for InvoiceNumber {saleTrxKey.DocNumber}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }
                    invoice = result.Invoices.FirstOrDefault(i => i.InvoiceNumber == saleTrxKey.DocNumber);
                }
                string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var _taxGroup = taxGroupMap[strTaxKey];

                Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                var sageFilter = new SageDocFilter()
                {
                    docKey = "CustomerNumber",
                    docNumber = invoice.CustomerNumber
                };
                var sCustomer = await GetARCustomer(sageFilter);
                if (sCustomer.IsError)
                {
                    _strError = sCustomer.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                    oeSaleTrx.Remark = _strError;
                }
                else
                {
                    oeSaleTrx = mapResult.GetValue();
                }

                #region Product-Duplicity workaround
                if (FixMultiLine && oeSaleTrx.SalesItems.Count > 1)
                {
                    var itemMap = new Dictionary<string, int>();
                    foreach (var item in oeSaleTrx.SalesItems)
                    {
                        if (itemMap.ContainsKey(item.ProductCode))
                        {
                            itemMap[item.ProductCode]++;
                        }
                        else
                        {
                            itemMap.Add(item.ProductCode, 1);
                        }
                    }
                    foreach (var kv in itemMap.Where(x => x.Value > 1))
                    {
                        var _amtSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                        var _unitSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                        var _qtySum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                        var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                        var finalItem = oeSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                        finalItem.UnitPrice = _avgUnitPrice;
                        finalItem.Quantity = finalItem.Package = _qtySum;
                        finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                        oeSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                        oeSaleTrx.SalesItems.Add(finalItem);
                    }
                }
                #endregion

                var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleTrxKey.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                    oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveSaleReq, invoice);
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = oeSaleTrx,
                    DTaxSaveSale = dTaxSaveSaleReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsSalesView, string>> GetConvertOECRNote(SaleTrxKey saleTrxKey)
        {
            string _method_ = "GetConvertOECRNote";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                #region Sage300 section
                if (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for OECRNote => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OECreditDebitNotes");

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"CreditDebitNoteNumber eq '{saleTrxKey.DocNumber}'";

                var gResult = await _masterDataSvc.GetTaxGroups();
                if (gResult.IsError)
                {
                    _strError = "Invalid TaxGroup Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                var result = await client.ProcessGetReqBasicAsync<OECreditDebitNotes>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                if (result == null || result.CreditDebitNotes.Count == 0)
                {
                    _strError = $"Not Found OECreditDebitNotes response from Sage for CRNumber {saleTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var crNote = result.CreditDebitNotes.FirstOrDefault(i => i.CreditDebitNoteNumber == saleTrxKey.DocNumber);
                string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingTRCurrency}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var _taxGroup = taxGroupMap[strTaxKey];

                Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer _customer = null;
                var sageFilter = new SageDocFilter()
                {
                    docKey = "CustomerNumber",
                    docNumber = crNote.CustomerNumber
                };
                var sCustomer = await GetARCustomer(sageFilter);
                if (sCustomer.IsError)
                {
                    _strError = sCustomer.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} OECreditDebitNote:{oeSaleTrx.DocNumber}, MapSalesInvcAttribs error : {_strError}");
                    oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                    oeSaleTrx.Remark = _strError;
                }
                else
                {
                    oeSaleTrx = mapResult.GetValue();
                }

                var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.InvoiceNumber);
                if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                {
                    _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.InvoiceNumber} for OECRNote => {JsonConvert.SerializeObject(saleTrxKey.DocNumber)}";
                    UI.Error($"{_method_} error : {_strError}");
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                    oeSaleTrx.Remark = _strError;
                }

                #region Product-Duplicity workaround
                if (FixMultiLine && oeSaleTrx.SalesItems.Count > 1)
                {
                    var itemMap = new Dictionary<string, int>();
                    foreach (var item in oeSaleTrx.SalesItems)
                    {
                        if (itemMap.ContainsKey(item.ProductCode))
                        {
                            itemMap[item.ProductCode]++;
                        }
                        else
                        {
                            itemMap.Add(item.ProductCode, 1);
                        }
                    }
                    foreach (var kv in itemMap.Where(x => x.Value > 1))
                    {
                        var _amtSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.TotalAmount);
                        var _unitSum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.UnitPrice * x.Quantity);
                        var _qtySum = oeSaleTrx.SalesItems.Where(x => x.ProductCode.Equals(kv.Key)).Sum(x => x.Quantity);
                        var _avgUnitPrice = Math.Round(_amtSum / _qtySum, 2);
                        var finalItem = oeSaleTrx.SalesItems.First(x => x.ProductCode.Equals(kv.Key));
                        finalItem.UnitPrice = _avgUnitPrice;
                        finalItem.Quantity = finalItem.Package = _qtySum;
                        finalItem.TotalAmount = (_qtySum * _avgUnitPrice);

                        oeSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                        oeSaleTrx.SalesItems.Add(finalItem);
                    }
                }
                #endregion

                var dTaxSaveCNoteReq = new DTaxSaveCNoteReq(_clientBranch, crNote, oeSaleTrx, origInvoice, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleTrxKey.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");
                if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                    oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveCNoteReq, crNote);
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = oeSaleTrx,
                    DTaxSaveCNoteReq = dTaxSaveCNoteReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

        }

        public async Task<Result<SalesTransact, string>> GetQRImage(int salesTrxId, bool updateMeta = false)
        {
            string _method_ = "GetQRImage";
            try
            {
                /*if (updateMeta)
                {
                    var xData = await _dbContext.SalesTransact.Include(x => x.SalesTrxData)
                        .Where(x => !string.IsNullOrWhiteSpace(x.SalesTrxData.ResponsePayload)
                            && (x.QRImage == null || x.QRImage.Length == 0)
                        ).ToListAsync();
                    foreach (var item in xData)
                    {
                        var etrSalesResp = JsonConvert.DeserializeObject<TrnsSalesSaveResp>(item.SalesTrxData.ResponsePayload);
                        var cuNumber = etrSalesResp.GetCUNumber(_clientBranch);
                        if (string.IsNullOrWhiteSpace(cuNumber))
                        {
                            UI.Error($"No Valid CUNumber Generated for receipt: {item.SalesTrxID}");
                            continue;
                        }
                        var qrText = etrSalesResp.GetQRText(_clientBranch);
                        if (string.IsNullOrWhiteSpace(qrText))
                        {
                            UI.Error($"No Valid QRText Generated for receipt: {item.SalesTrxID}");
                            continue;
                        }
                        var qrData = FileBinUtils.GenerateQRCode(qrText);
                        if (qrData is null or [])
                        {
                            UI.Error($"No Valid QRImage Generated for receipt: {item.SalesTrxID}");
                            continue;
                        }

                        await _dbContext.SalesTransact.Where(e => e.SalesTrxID == item.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.CUNumber, cuNumber)
                            .SetProperty(x => x.QRText, qrText)
                            .SetProperty(x => x.QRTime, item.SalesTrxData.ResponseTime)
                            .SetProperty(x => x.QRImage, qrData)
                            .SetProperty(x => x.SDCID, etrSalesResp.Data.sdcId)
                            .SetProperty(x => x.InternalData, etrSalesResp.Data.InternalData)
                            .SetProperty(x => x.ReceiptNumber, etrSalesResp.Data.ReceiptNumber)
                            .SetProperty(x => x.ReceiptSignature, etrSalesResp.Data.ReceiptSignature)
                        );
                    }
                }*/

                var salesTransact = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.SalesTrxID == salesTrxId);
                if (salesTransact is null || salesTransact.QRImage is null)
                {
                    return $"No Image found for ID:{salesTrxId}";
                }
                return salesTransact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int, string>> ProcessSaleCallback(SaleCallback saleCallback)
        {
            string _method_ = "ProcessSaleCallback";
            string _strError = null;
            try
            {
                int changes = 0;
                if (saleCallback is null || saleCallback?.CBData is null)
                {

                    _strError = $"Invalid DigiTax SalesCallback request:{JsonConvert.SerializeObject(null)}";
                    UI.Error(_strError);
                    return _strError;
                }
                UI.Info($"{_method_} >> {JsonConvert.SerializeObject(saleCallback)}");
                var saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .FirstOrDefaultAsync(e => e.DocNumber.Equals(saleCallback.CBData.TraderInvoiceNo));
                if (saleTransact is null)
                {
                    _strError = $"Invalid DigiTax SalesTransact request for DocNumber: {saleCallback.CBData.TraderInvoiceNo}";
                    UI.Error(_strError);
                    return _strError;
                }

                var tStamp = DateTime.Now;
                var _remark = $"Callback Complete at {tStamp.ToString("s")}";

                var cuNumber = saleCallback.GetCUNumber(_clientBranch);
                if (string.IsNullOrWhiteSpace(cuNumber))
                {
                    _strError = $"No Valid CUNumber Generated for receipt: {saleTransact.SalesTrxID}";
                    UI.Error($"{_method_} error: {saleTransact.SalesTrxID}");
                }
                byte[] qrData = null;
                string qrText = saleCallback.GetQRText();
                if (string.IsNullOrWhiteSpace(qrText))
                {
                    _strError = $"No Valid QRText Generated for receipt: {saleTransact.SalesTrxID}";
                    UI.Error($"{_method_} error: {saleTransact.SalesTrxID}");
                }
                else
                {
                    qrData = FileBinUtils.GenerateQRCode(qrText);
                    if (qrData is null or [])
                    {
                        _strError = $"No Valid QRImage Generated for receipt: {saleTransact.SalesTrxID}";
                        UI.Error($"{_method_} error: {saleTransact.SalesTrxID}");
                    }
                }

                changes += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ExternalID, saleCallback.CBData.ID)
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, RecordStatus.POST_OK)
                            .SetProperty(x => x.CUNumber, cuNumber)
                            .SetProperty(x => x.QRText, qrText)
                            .SetProperty(x => x.QRTime, tStamp)
                            .SetProperty(x => x.QRImage, qrData)
                            //.SetProperty(x => x.SDCID, saleCallback.CBData.SerialNumber)
                            .SetProperty(x => x.InternalData, saleCallback.CBData.InternalData)
                            .SetProperty(x => x.ReceiptNumber, saleCallback.CBData.ReceiptNumber)
                            .SetProperty(x => x.ReceiptSignature, saleCallback.CBData.ReceiptSignature)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                changes += await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.CallbackTime, tStamp)
                            .SetProperty(x => x.CallbackPayload, JsonConvert.SerializeObject(saleCallback))
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                return changes;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsTransact, string>> QueueSaveSale(QueueSaveSale filter)
        {
            string _method_ = "QueueSaveSale";
            string _strError = null;
            EtimsTransact transactSale = null;
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL };

                if (string.IsNullOrWhiteSpace(filter.DocNumber) || string.IsNullOrWhiteSpace(filter.BranchCode))
                {
                    _strError = $"Invalid Filter Provided : [{filter.BranchCode}:{filter.DocNumber}]";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                var saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .Where(e => e.BranchCode.Equals(filter.BranchCode) && e.DocNumber.Equals(filter.DocNumber))
                    .FirstOrDefaultAsync();
                if (saleTransact is null)
                {
                    _strError = $"No valid stock item found for Document: {filter.DocNumber}";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                //TODO: Check Status before queueing

                transactSale = saleTransact.GetSalesTransact(_clientBranch);
                if (transactSale is null)
                {
                    _strError = $"No valid Sales transaction generated for Document: {filter.DocNumber}";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                // check status before processing
                if (saleTransact.RecordStatus == RecordStatus.POST_OK //|| saleTransact.RecordStatus == RecordStatus.POST_FAIL
                    || saleTransact.RecordStatus == RecordStatus.POST_DUPL || !saleTransact.IsValid())
                {
                    _strError = $"EtimsTransact generation for Sale : [{filter.BranchCode}:{filter.DocNumber}] failed: invalid status";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                var invalidItems = await _dbContext.SalesItem.Where(x => x.SalesTrxID == saleTransact.SalesTrxID 
                    && !completeStatii.Contains(x.RecordStatus)).ToListAsync();
                if (invalidItems?.Count > 0)
                {
                    var tempList = new List<string>();
                    invalidItems.ForEach(x => tempList.Add($"[{x.ProductCode} => {x.Description}]"));
                    _strError = "Invalid items in the invoice: \n " + string.Join(", ", tempList);
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                if (saleTransact.DocType == DocumentType.INVOICE)
                {
                    var dTaxSaveSaleReq = saleTransact.SalesTrxData.GetDTaxInvRequest();
                    var _error = dTaxSaveSaleReq.GetError();
                    if (!string.IsNullOrWhiteSpace(_error))
                    {
                        _strError = $"EtimsTransact invalid for Sale : [{filter.BranchCode}:{filter.DocNumber}] error: {_strError}";
                        UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                        return _strError;
                    }
                    var etimsRespSale = await _dTaxService.CreateDTaxSale(dTaxSaveSaleReq);
                    using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var tStamp = DateTime.Now;
                            var recordStatus = RecordStatus.POST_FAIL;

                            if (etimsRespSale.IsError)
                            {
                                _strError = etimsRespSale.GetError();
                                try
                                {
                                    var _objResp = JsonConvert.DeserializeObject<DTaxSaveSaleResp>(_strError);
                                    if (_objResp is not null && !string.IsNullOrWhiteSpace(_objResp.Message))
                                        _strError = _objResp.Message;
                                }
                                catch (Exception tex)
                                {
                                    UI.Error($"{_method_} SaleTransact ID:{saleTransact.SalesTrxID} failed deserializing {_strError}, error: {tex.GetBaseException().Message}");
                                }
                                UI.Error($"Saving SaleTransact: {saleTransact.CacheKey} failed: {etimsRespSale.GetError()}");
                                transactSale.RespPayload = etimsRespSale.GetError();

                                // Update & Save Transact Changes
                                await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ResponsePayload, etimsRespSale.GetError())
                                    .SetProperty(x => x.ResponseTime, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                                );
                                await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
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

                            var saleTrxResp = etimsRespSale.GetValue();
                            transactSale.RespPayload = saleTrxResp.RawResponse;
                            recordStatus = RecordStatus.POST_OK;

                            var cuNumber = saleTrxResp.GetCUNumber(_clientBranch);
                            if (string.IsNullOrWhiteSpace(cuNumber))
                            {
                                _strError = $"No Valid CUNumber Generated for receipt: {saleTransact.SalesTrxID}";
                                UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                            }
                            byte[] qrData = null;
                            string qrText = saleTrxResp.GetQRText();
                            if (string.IsNullOrWhiteSpace(qrText))
                            {
                                _strError = $"No Valid QRText Generated for receipt: {saleTransact.SalesTrxID}";
                                UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                            }
                            else
                            {
                                qrData = FileBinUtils.GenerateQRCode(qrText);
                                if (qrData is null or [])
                                {
                                    _strError = $"No Valid QRImage Generated for receipt: {saleTransact.SalesTrxID}";
                                    UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                                }
                            }

                            // Update & Save Transact Changes
                            var _remark = $"{saleTrxResp.Status} on {tStamp.ToString("s")}";

                            int _dbChanges = await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            _dbChanges += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ExternalID, saleTrxResp.ID)
                                .SetProperty(x => x.Remark, _remark)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.CUNumber, cuNumber)
                                .SetProperty(x => x.QRText, qrText)
                                .SetProperty(x => x.QRTime, tStamp)
                                .SetProperty(x => x.QRImage, qrData)
                                .SetProperty(x => x.SDCID, saleTrxResp.SerialNumber)
                                .SetProperty(x => x.InternalData, saleTrxResp.InternalData)
                                .SetProperty(x => x.ReceiptNumber, saleTrxResp.ReceiptNumber)
                                .SetProperty(x => x.ReceiptSignature, saleTrxResp.ReceiptSignature)
                                .SetProperty(x => x.ExternalURL, saleTrxResp.SaleDetailURL)
                                .SetProperty(x => x.OfflineURL, saleTrxResp.OfflineURL)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );

                            await _dbTrans.CommitAsync();
                            UI.Debug($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, DBChanges: {_dbChanges}");
                        }
                        catch (Exception iex)
                        {
                            await _dbTrans.RollbackAsync();
                            UI.Error(iex, $"{_method_} SaleTrxID: {saleTransact.SalesTrxID} save valid record error : {iex.GetBaseException().Message}");
                            throw;
                        }
                    }
                }
                else if (saleTransact.DocType == DocumentType.CREDITNOTE)
                {
                    var dTaxSaveSaleReq = saleTransact.SalesTrxData.GetDTaxCNoteRequest();
                    var _error = dTaxSaveSaleReq.GetError();
                    if (!string.IsNullOrWhiteSpace(_error))
                    {
                        _strError = $"EtimsTransact invalid for Sale : [{filter.BranchCode}:{filter.DocNumber}] error: {_strError}";
                        UI.Error($"{_method_} error: {_strError}");
                        return _strError;
                    }
                    var etimsRespSale = await _dTaxService.CreateDTaxCRNote(dTaxSaveSaleReq);
                    using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var tStamp = DateTime.Now;
                            var recordStatus = RecordStatus.POST_FAIL;

                            if (etimsRespSale.IsError)
                            {
                                _strError = etimsRespSale.GetError();
                                try
                                {
                                    var _objResp = JsonConvert.DeserializeObject<DTaxSaveCNoteResp>(_strError);
                                    if (_objResp is not null && !string.IsNullOrWhiteSpace(_objResp.Message))
                                        _strError = _objResp.Message;
                                }
                                catch(Exception tex)
                                {
                                    UI.Error($"{_method_} SaleTransact ID:{saleTransact.SalesTrxID} failed deserializing {_strError}, error: {tex.GetBaseException().Message}");
                                }
                                UI.Error($"Saving SaleTransact: {saleTransact.CacheKey} failed: {etimsRespSale.GetError()}");
                                transactSale.RespPayload = _strError;

                                // Update & Save Transact Changes
                                await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ResponsePayload, etimsRespSale.GetError())
                                    .SetProperty(x => x.ResponseTime, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                                );
                                await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
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

                            var saleTrxResp = etimsRespSale.GetValue();
                            transactSale.RespPayload = saleTrxResp.RawResponse;
                            recordStatus = RecordStatus.POST_OK;

                            var cuNumber = saleTrxResp.GetCUNumber(_clientBranch);
                            if (string.IsNullOrWhiteSpace(cuNumber))
                            {
                                _strError = $"No Valid CUNumber Generated for receipt: {saleTransact.SalesTrxID}";
                                UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                            }
                            byte[] qrData = null;
                            string qrText = saleTrxResp.GetQRText();
                            if (string.IsNullOrWhiteSpace(qrText))
                            {
                                _strError = $"No Valid QRText Generated for receipt: {saleTransact.SalesTrxID}";
                                UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                            }
                            else
                            {
                                qrData = FileBinUtils.GenerateQRCode(qrText);
                                if (qrData is null or [])
                                {
                                    _strError = $"No Valid QRImage Generated for receipt: {saleTransact.SalesTrxID}";
                                    UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                                }
                            }

                            // Update & Save Transact Changes
                            var _remark = $"{saleTrxResp.Status} on {tStamp.ToString("s")}";

                            int _dbChanges = await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            _dbChanges += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ExternalID, saleTrxResp.ID)
                                .SetProperty(x => x.Remark, _remark)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.CUNumber, cuNumber)
                                .SetProperty(x => x.QRText, qrText)
                                .SetProperty(x => x.QRTime, tStamp)
                                .SetProperty(x => x.QRImage, qrData)
                                .SetProperty(x => x.SDCID, saleTrxResp.SerialNumber)
                                .SetProperty(x => x.InternalData, saleTrxResp.InternalData)
                                .SetProperty(x => x.ReceiptNumber, saleTrxResp.ReceiptNumber)
                                .SetProperty(x => x.ReceiptSignature, saleTrxResp.ReceiptSignature)
                                .SetProperty(x => x.ExternalURL, saleTrxResp.SaleDetailURL)
                                .SetProperty(x => x.OfflineURL, saleTrxResp.OfflineURL)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );

                            await _dbTrans.CommitAsync();
                            UI.Debug($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, DBChanges: {_dbChanges}");
                        }
                        catch (Exception iex)
                        {
                            await _dbTrans.RollbackAsync();
                            UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                            throw;
                        }
                    }
                }
                else
                {
                    _strError = $"Invalid DocType: {saleTransact.DocType.ToString()} for SalesTransact DocNumber:{saleTransact.DocNumber}";
                    UI.Error($"{_method_} SaleTrxID: {saleTransact.SalesTrxID}, error: {_strError}");
                    return _strError;
                }

                return transactSale;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int,string>> PostReadyTaxTrxs()
        {
            string _method_ = "PostReadyTaxTrxs";
            try
            {
                int counter = 0;
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL, RecordStatus.DEPENDS };

                var queueList = await _dbContext.SalesTransact.Where(x => !completeStatii.Contains(x.RecordStatus) && x.Tries <= 3)
                    .OrderBy(x => x.CreatedOn).Take(5)
                    .Select(x => new QueueSaveSale() { BranchCode = x.BranchCode, DocNumber = x.DocNumber }).ToListAsync();
                counter = queueList.Count;
                UI.Debug($"{_method_} processing {queueList.Count} transactions");
                foreach(var item in queueList)
                {
                    var result = await QueueSaveSale(item);
                    if (result.IsError)
                    {
                        UI.Error($"{_method_} DocNumber:{item.DocNumber}, Error: {result.GetError()}");
                    }
                    else
                    {
                        UI.Error($"{_method_} DocNumber:{item.DocNumber} Processed Successfully");
                    }
                }
                return counter;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public Task<Result<EtimsTransact, string>> ProcessSaveSale(EtimsTransact transactSale)
        {
            throw new NotImplementedException();
        }
    }
}
