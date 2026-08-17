using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Scriban;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public class S300DTaxSaleService : S300BaseSaleService, IS300SaleService
    {
        private readonly IDigiTaxService _dTaxService;
        private readonly IS300ProductSvc _productSvc;
        private readonly bool FixMultiLine = false;

        private readonly SemaphoreSlim _smFetchOEInvoices = new(1, 1);
        private readonly SemaphoreSlim _smFetchOECRNotes = new(1, 1);
        private readonly SemaphoreSlim _smFetchARInvoices = new(1, 1);
        private readonly SemaphoreSlim _smFetchARCRNotes = new(1, 1);

        public S300DTaxSaleService(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory,
            ExtSystConfig extSystConfig, IMasterDataSvc masterDataSvc, IDigiTaxService dTaxService, IEnumerable<IS300ProductSvc> productSvcs)
            : base(dbContext, multiplexer, extSystConfig, masterDataSvc, httpClientFactory)
        {
            _syncChannelMap = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();

            _dTaxService = dTaxService;
            _productSvc = productSvcs.Single(x => x.GetDeviceType() == TaxDeviceType.DIGITAX);
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
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                if (!await _smFetchARInvoices.WaitAsync(0))
                {
                    _strError = "FetchOEInvoices is already running. Please wait for it to complete.";
                    UI.Warn($"{_method_} : {_strError}");
                    return _strError;
                }
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

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                }

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
                        _strError = $"Not Found ARInvoices response from Sage: $skip={syncChannel.OffSet}";
                        UI.Debug($"{_method_} error : {_strError}");
                        return results;
                    }

                    foreach (var invBatch in invoiceBatches.InvoiceBatches)
                    {
                        var invoices = invBatch.Invoices.Where(x => x.DocumentType ==
                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice).ToList();
                        if (invoices == null || invoices.Count == 0)
                        {
                            _strError = $"ARInvoices BatchNumber {invBatch.BatchNumber} has no Invoices: $skip={syncChannel.OffSet}";
                            UI.Warn($"{invBatch.BatchNumber}", $"{_method_} error : {_strError}");

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
                                throw new Exception($"ARInvoices SaveSyncTrxChannel BatchNumber {invBatch.BatchNumber} saving to database failed");
                            }
                            _dbContext.ChangeTracker.Clear();
                            await _masterDataSvc.UpdateSyncTrxTracker(syncChannel);

                            continue;
                        }

                        foreach (var invoice in invoices)
                        {
                            bool _skipDocument = false;

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

                            if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                            {
                                scribanHelper["document"] = invoice;
                                string currEval = syncChannel.ParseSyntax.Filter;
                                bool evalRes = await ScriptHelper.strToBool(currEval, context);
                                if (!evalRes)
                                {
                                    _skipDocument = true;
                                    UI.Info($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"Skipping Invoice:[{invoice.DocumentNumber}] :: {currEval} >> {evalRes}");
                                }
                            }

                            string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingCurrencyCode}:Sales";
                            if (!taxGroupMap.ContainsKey(strTaxKey))
                            {
                                _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                                UI.Error($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"{_method_} error : {_strError}");
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
                                UI.Error($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"{_method_} error : {_strError}");
                                return _strError;
                            }
                            _customer = sCustomer.GetValue();

                            var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                            if (_skipDocument)
                            {
                                arSaleTrx.RecordStatus = RecordStatus.INVALID;
                                arSaleTrx.Remark = $"Invoice skipped, should not be processed";
                            }
                            var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, _productSvc);
                            if (mapResult.IsError)
                            {
                                _strError = mapResult.GetError();
                                UI.Error($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"{_method_} ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] MapSalesInvcAttribs error : {_strError}");
                                if (arSaleTrx.IsValid())
                                {
                                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                    arSaleTrx.Remark = _strError;
                                }
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
                            UI.Info($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"<< ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                            if (arSaleTrx.IsValid())
                            {
                                if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                                    arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                                else
                                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                            }

                            var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveSaleReq, invoice);
                            if (_skipDocument)
                            {
                                salesTrxData.ClearRequestData();
                            }
                            arSaleTrx.SalesTrxData = salesTrxData;
                            UI.Info($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"<< ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}] SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                            using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                            {
                                int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                                try
                                {
                                    if (_dbContext.SalesTransact.AddIfNotExists(arSaleTrx, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"ARInvoice:[{invoice.BatchNumber}:{invoice.DocumentNumber}]  Already Exists");
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
                                        UI.Error($"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
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
                                    UI.Error(iex, $"{invoice.BatchNumber}:{invoice.DocumentNumber}", $"{_method_} save valid record error : {iex.GetBaseException().Message}");
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
            finally
            {
                _smFetchARInvoices.Release();
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
                if (!await _smFetchARCRNotes.WaitAsync(0))
                {
                    _strError = "FetchOEInvoices is already running. Please wait for it to complete.";
                    UI.Warn($"{_method_} : {_strError}");
                    return _strError;
                }
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
                        _strError = $"Not Found ARInvoices response from Sage: $skip={syncChannel.OffSet}";
                        UI.Debug($"{_method_} error : {_strError}");
                        return results;
                    }

                    foreach (var invBatch in invoiceBatches.InvoiceBatches)
                    {
                        var arCRNotes = invBatch.Invoices.Where(x => x.DocumentType ==
                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote).ToList();
                        if (arCRNotes == null || arCRNotes.Count == 0)
                        {
                            _strError = $"ARInvoices BatchNumber {invBatch.BatchNumber} has no CreditNotes: $skip={syncChannel.OffSet}";
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

                        foreach (var crNote in arCRNotes)
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
                                UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} error : {_strError}");
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
                                UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} error : {_strError}");
                                return _strError;
                            }
                            _customer = sCustomer.GetValue();

                            var arSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                            var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, _productSvc);
                            if (mapResult.IsError)
                            {
                                _strError = mapResult.GetError();
                                UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] MapSalesInvcAttribs error : {_strError}");
                                if (arSaleTrx.IsValid())
                                {
                                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                    arSaleTrx.Remark = _strError;
                                }
                            }
                            else
                            {
                                arSaleTrx = mapResult.GetValue();
                            }

                            var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.ApplytoDocument);
                            if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                            {
                                _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.ApplytoDocument} for ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]";
                                UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} error : {_strError}");
                                if (arSaleTrx.RecordStatus != RecordStatus.INVALID)
                                {
                                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                    arSaleTrx.Remark = _strError;
                                }
                            }
                            else if (origInvoice.DocExchRate != arSaleTrx.DocExchRate)
                            {
                                _strError = $"Exchange Rate Error, Invoice Rate {origInvoice.DocExchRate:N2} differs from Credit Note Rate {arSaleTrx.DocExchRate:N2}";
                                UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} error : {_strError}");
                                if (arSaleTrx.IsValid())
                                {
                                    arSaleTrx.RecordStatus = RecordStatus.INVALID;
                                    arSaleTrx.Remark = _strError;
                                }
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
                            UI.Info($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"<< ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");

                            if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                                arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else if (arSaleTrx.RecordStatus != RecordStatus.INVALID)
                                arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                            var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveCNoteReq, crNote);
                            arSaleTrx.SalesTrxData = salesTrxData;
                            UI.Info($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"<< ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}] SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                            using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                            {
                                int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                                try
                                {
                                    if (_dbContext.SalesTransact.AddIfNotExists(arSaleTrx, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"ARCRNote:[{crNote.BatchNumber}:{crNote.DocumentNumber}]  Already Exists");
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
                                        UI.Error($"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
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
                                    UI.Error(iex, $"{crNote.BatchNumber}:{crNote.DocumentNumber}", $"{_method_} save valid record error : {iex.GetBaseException().Message}");
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
            finally
            {
                _smFetchARCRNotes.Release();
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
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                if (!await _smFetchOEInvoices.WaitAsync(0))
                {
                    _strError = "FetchOEInvoices is already running. Please wait for it to complete.";
                    UI.Warn($"{_method_} : {_strError}");
                    return _strError;
                }
                UI.Debug($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE" && e.DocType == DocumentType.INVOICE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(300);
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

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                }

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var invList = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (invList == null || invList.Invoices.Count == 0)
                    {
                        _strError = $"Null/Missing OEInvoices response from Sage: $skip={syncChannel.OffSet}";
                        UI.Debug($"{_method_} : {_strError}");
                        return results;
                    }
                    loop = (invList.nextLink != null);
                    syncChannel.IncrOffSet(invList.Invoices.Count);

                    invList.Invoices.RemoveAll(i => invoiceMap.ContainsKey(i.InvoiceNumber));
                    if (invList.Invoices.Count == 0)
                    {
                        return results;
                    }

                    foreach (var invoice in invList.Invoices)
                    {
                        bool _skipDocument = false;
                        if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                        {
                            scribanHelper["document"] = invoice;
                            string currEval = syncChannel.ParseSyntax.Filter;
                            bool evalRes = await ScriptHelper.strToBool(currEval, context);
                            if (!evalRes)
                            {
                                _skipDocument = true;
                                UI.Info(invoice.InvoiceNumber, $"Skipping Invoice:[{invoice.InvoiceNumber}] :: {currEval} against [{invoice.OrderNumber}] >> {evalRes}");
                            }
                        }

                        // Sort Tax Group
                        string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                        if (!taxGroupMap.ContainsKey(strTaxKey))
                        {
                            _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                            UI.Error(invoice.InvoiceNumber, $"{_method_} error : {_strError}");
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
                            UI.Error(invoice.InvoiceNumber, $"{_method_} error : {_strError}");
                            return _strError;
                        }
                        _customer = sCustomer.GetValue();

                        var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                        if (_skipDocument)
                        {
                            oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                            oeSaleTrx.Remark = $"Invoice skipped, should not be processed";
                        }
                        var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx, _productSvc);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error(invoice.InvoiceNumber, $"{_method_} OEInvoice:{invoice.InvoiceNumber}, MapSalesInvcAttribs error : {_strError}");
                            if (oeSaleTrx.IsValid())
                            {
                                oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                oeSaleTrx.Remark = _strError;
                            }
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
                        UI.Info(oeSaleTrx.DocNumber, $"<< {oeSaleTrx.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                        if (oeSaleTrx.IsValid())
                        {
                            if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                                oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else
                                oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        }

                        var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveSaleReq, invoice);
                        if (_skipDocument)
                        {
                            salesTrxData.ClearRequestData();
                        }
                        oeSaleTrx.SalesTrxData = salesTrxData;
                        UI.Info(oeSaleTrx.DocNumber, $"<< {oeSaleTrx.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                            try
                            {
                                if (_dbContext.SalesTransact.AddIfNotExists(oeSaleTrx, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn(oeSaleTrx.DocNumber, $"OEInvoice {oeSaleTrx.DocNumber} Already Exists");
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
                                    UI.Error(oeSaleTrx.DocNumber, $"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
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
                                UI.Error(iex, oeSaleTrx.DocNumber, $"{_method_} OEInvoice:{oeSaleTrx.DocNumber} save valid record error : {iex.GetBaseException().Message}");
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
            finally
            {
                _smFetchOEInvoices.Release();
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
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                if (!await _smFetchOECRNotes.WaitAsync(0))
                {
                    _strError = "FetchOECRDRNotes is already running. Please wait for it to complete.";
                    UI.Warn($"{_method_} : {_strError}");
                    return _strError;
                }
                UI.Debug($">> {_method_}");
                var syncChannel = _syncChannelMap[GeneralConst.OE_CRDRNOTE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE" && e.DocType == DocumentType.CREDITNOTE)
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, syncChannel.GetMinDate().Date.ToString("s"));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(300);
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

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                }

                bool loop = true;
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    var crNoteList = await client.ProcessGetReqBasicAsync<OECreditDebitNotes>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (crNoteList == null || crNoteList.CreditDebitNotes.Count == 0)
                    {
                        _strError = $"Null OECreditDebitNotes response from Sage: $skip={syncChannel.OffSet}";
                        UI.Debug($"{_method_} : {_strError}");
                        return results;
                    }
                    loop = (crNoteList.nextLink != null);
                    syncChannel.IncrOffSet(crNoteList.CreditDebitNotes.Count);

                    crNoteList.CreditDebitNotes.RemoveAll(i => invoiceMap.ContainsKey(i.CreditDebitNoteNumber));
                    foreach (var crNote in crNoteList.CreditDebitNotes)
                    {
                        bool _skipDocument = false;
                        if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                        {
                            scribanHelper["document"] = crNote;
                            string currEval = syncChannel.ParseSyntax.Filter;
                            bool evalRes = await ScriptHelper.strToBool(currEval, context);
                            if (!evalRes)
                            {
                                _skipDocument = true;
                                UI.Info(crNote.CreditDebitNoteNumber, $"Skipping Invoice:[{crNote.CreditDebitNoteNumber}] :: {currEval} against [{crNote.OrderNumber}] >> {evalRes}");
                            }
                        }

                        // Sort Tax Group
                        string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingTRCurrency}:Sales";
                        if (!taxGroupMap.ContainsKey(strTaxKey))
                        {
                            _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                            UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} error : {_strError}");
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
                            UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} error : {_strError}");
                            return _strError;
                        }
                        _customer = sCustomer.GetValue();

                        var oeSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                        var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx, _productSvc);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} OECreditDebitNote:{oeSaleTrx.DocNumber}, MapSalesInvcAttribs error : {_strError}");
                            if (oeSaleTrx.IsValid())
                            {
                                oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                                oeSaleTrx.Remark = _strError;
                            }
                        }
                        else
                        {
                            oeSaleTrx = mapResult.GetValue();
                        }

                        var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.InvoiceNumber);
                        if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                        {
                            _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.InvoiceNumber} for OECreditDebitNote:{oeSaleTrx.DocNumber}";
                            UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} error : {_strError}");
                            if (oeSaleTrx.IsValid())
                            {
                                oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                                oeSaleTrx.Remark = _strError;
                            }
                        }
                        if (origInvoice.DocExchRate != oeSaleTrx.DocExchRate)
                        {
                            _strError = $"Exchange Rate Error, Invoice Rate {origInvoice.DocExchRate:N2} differs from Credit Note Rate {oeSaleTrx.DocExchRate:N2}";
                            UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} error : {_strError}");
                            if (oeSaleTrx.IsValid())
                            {
                                oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                                oeSaleTrx.Remark = _strError;
                            }
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
                        UI.Info(crNote.CreditDebitNoteNumber, $"<< {oeSaleTrx.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");
                        if (oeSaleTrx.IsValid())
                        {
                            if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                                oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else
                                oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        }

                        var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveCNoteReq, crNote);
                        if (_skipDocument)
                        {
                            salesTrxData.ClearRequestData();
                        }
                        oeSaleTrx.SalesTrxData = salesTrxData;
                        UI.Info(crNote.CreditDebitNoteNumber, $"<< {oeSaleTrx.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                        using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                        {
                            int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                            try
                            {
                                if (_dbContext.SalesTransact.AddIfNotExists(oeSaleTrx, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn(crNote.CreditDebitNoteNumber, $"OECRNote {oeSaleTrx.DocNumber} Already Exists");
                                    continue;
                                }
                                _dbContext.Attach(_clientBranch);
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"OECRNote {oeSaleTrx.DocNumber} saving to database failed");
                                }

                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                if (!await _masterDataSvc.UpdateBranchTrxAsync(_clientBranch, _dbContext))
                                {
                                    throw new Exception($"{_method_} - UpdateBranchTrxAsync : Failed Updating ClientBranch Details");
                                }
                                if (!await _masterDataSvc.SaveSyncTrxChannel(syncChannel, _dbContext))
                                {
                                    UI.Error(crNote.CreditDebitNoteNumber, $"{_method_} - SaveSyncSchedule : Failed Updating SyncTrxChannel");
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
                                UI.Error(iex, crNote.CreditDebitNoteNumber, $"{_method_} OECRNote: {oeSaleTrx.DocNumber} save valid record error : {iex.GetBaseException().Message}");
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
            finally
            {
                _smFetchOECRNotes.Release();
            }

            return results;
        }

        public async Task<Result<SalesTransact, string>> ReFetchInvoice(SaleBatchTrxKey saleBatchTrxKey)
        {
            string _method_ = "ReFetchOEInvoice";
            SalesTransact saleTrx = null;
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                if (saleBatchTrxKey == null || string.IsNullOrWhiteSpace(saleBatchTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for OEInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var client = _httpClientFactory.CreateClient();

                saleTrx = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.DocNumber == saleBatchTrxKey.DocNumber);
                if (saleTrx is null)
                {
                    _strError = $"Invalid or missing Invoice {saleTrx.DocNumber} in SalesTransact data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                // var salesTrxData = await _dbContext.SalesTrxData.FirstOrDefaultAsync(x => x.SalesTrxID == oeSaleTrx.SalesTrxID);
                if (saleTrx.SalesTrxData is null)
                {
                    _strError = $"Invalid or missing OEInvoice {saleBatchTrxKey.DocNumber} in SalesTrxData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var salesTrxData = saleTrx.SalesTrxData;

                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(saleTrx, _productSvc);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    if (saleTrx.IsValid())
                    {
                        saleTrx.RecordStatus = RecordStatus.DEPENDS;
                        saleTrx.Remark = _strError;
                        return _strError;
                    }
                }
                saleTrx = mapResult.GetValue();

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

                var qParams = new Dictionary<string, string>();

                if (saleTrx.SourceApp == "OE")
                {
                    #region Sage300 section
                    qParams["$filter"] = $"InvoiceNumber eq '{saleBatchTrxKey.DocNumber}'";

                    string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/OE/OEInvoices");
                    var result = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null || result.Invoices.Count == 0)
                    {
                        _strError = $"Not Found OEInvoices response from Sage for InvoiceNumber {saleBatchTrxKey.DocNumber}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    var invoice = result.Invoices.FirstOrDefault(i => i.InvoiceNumber == saleBatchTrxKey.DocNumber);
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

                    var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, saleTrx, _taxGroup, taxAuthKeys, _customer);
                    if (saleTrx.RecordStatus == RecordStatus.NONE)
                        saleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        saleTrx.RecordStatus = RecordStatus.DEPENDS;

                    var _oldSaveSalesRes = JsonConvert.DeserializeObject<DTaxSaveSaleReq>(salesTrxData.RequestPayload);
                    if (!_oldSaveSalesRes.HasEqualValue(dTaxSaveSaleReq))
                    {
                        salesTrxData.RequestPayload = JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
                        salesTrxData.UpdatedOn = DateTime.Now;
                        salesTrxData.UpdatedBy = GeneralConst.APPLICATION_NAME;
                        int affected = await _dbContext.SaveChangesAsync();
                        UI.Info($"{_method_} update {affected} records updated.");
                    }

                }
                else if (saleTrx.SourceApp == "AR")
                {
                    #region Sage300 section
                    if (string.IsNullOrWhiteSpace(saleBatchTrxKey.BatchNumber))
                    {
                        _strError = $"Invalid filter for ARInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }

                    string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARInvoiceBatches");
                    qParams["$filter"] = $"BatchStatus eq 'Posted' and SourceApplication eq 'AR' and BatchNumber eq {saleBatchTrxKey.BatchNumber}";

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

                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice = null;
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

                    var dTaxSaveSaleReq = new DTaxSaveSaleReq(_clientBranch, invoice, saleTrx, _taxGroup, taxAuthKeys, _customer);
                    if (saleTrx.RecordStatus == RecordStatus.NONE)
                        saleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        saleTrx.RecordStatus = RecordStatus.DEPENDS;

                    var _oldSaveSalesRes = JsonConvert.DeserializeObject<DTaxSaveSaleReq>(salesTrxData.RequestPayload);
                    if (!_oldSaveSalesRes.HasEqualValue(dTaxSaveSaleReq))
                    {
                        salesTrxData.RequestPayload = JsonConvert.SerializeObject(dTaxSaveSaleReq, new DecimalFormatConverter());
                        salesTrxData.UpdatedOn = DateTime.Now;
                        salesTrxData.UpdatedBy = GeneralConst.APPLICATION_NAME;
                        int affected = await _dbContext.SaveChangesAsync();
                        UI.Info($"{_method_} update {affected} records updated.");
                    }

                }
                else
                {
                    _strError = $"Invalid SourceApp {saleTrx.SourceApp} for DocNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return saleTrx;
        }
        public async Task<Result<SalesTransact, string>> ReSyncTaxInvoice(SaleTrxKey saleTrxKey)
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
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
                if (saleTransact is null)
                {
                    _strError = $"Invalid or missing SalesTransact {saleTransact.DocNumber} in SalesTransact data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK };
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
                    UI.Info($"{_method_} info : {_strError}");
                    return _strError;
                }

                var syncResp = await SyncSaleTrx(saleTransact);
                if (syncResp.IsError)
                {
                    _strError = syncResp.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                saleTransact = syncResp.GetValue();
                return saleTransact;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsSalesView, string>> GetConvertARInvoice(SaleBatchTrxKey saleBatchTrxKey, string srcPayload = null)
        {
            string _method_ = "GetConvertARInvoice";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice = null;
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                bool _skipDocument = false;
                var syncChannel = _syncChannelMap[GeneralConst.AR_INVOICE_SYNC];

                #region Sage300 section
                if (string.IsNullOrWhiteSpace(srcPayload) && (saleBatchTrxKey == null || string.IsNullOrWhiteSpace(saleBatchTrxKey.BatchNumber)))
                {
                    _strError = $"Invalid filter for ARInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
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
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                if (!string.IsNullOrWhiteSpace(srcPayload))
                {
                    invoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice>(srcPayload);
                    if (invoice.DocumentType != Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice)
                    {
                        _strError = $"Supplied ARDocument DocumentNumber:{invoice.DocumentNumber} is not an Invoice. It is a {invoice.DocumentType.ToString()}";
                        UI.Warn(invoice.DocumentNumber, _strError);
                        return _strError;
                    }
                    saleBatchTrxKey.BatchNumber = invoice.BatchNumber.ToString();
                    saleBatchTrxKey.DocNumber = invoice.DocumentNumber;
                }
                else
                {
                    var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                                null, qParams);
                    if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                    {
                        _strError = $"Not Found ARInvoices response from Sage for BatchNumber {saleBatchTrxKey.DocNumber}";
                        UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                        return _strError;
                    }

                    var invBatch = invoiceBatches.InvoiceBatches.FirstOrDefault();
                    if (invBatch == null)
                    {
                        _strError = $"Missing ARInvoices response from Results for BatchNumber {saleBatchTrxKey.DocNumber}";
                        UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                        return _strError;
                    }
                    if (string.IsNullOrWhiteSpace(saleBatchTrxKey.DocNumber))
                        invoice = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice);
                    else
                        invoice = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice
                        && x.DocumentNumber == saleBatchTrxKey.DocNumber);
                }
                if (invoice == null)
                {
                    _strError = $"ARInvoices BatchNumber {saleBatchTrxKey.DocNumber} has no Invoice with DocumentNumber:{saleBatchTrxKey.DocNumber}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    return _strError;
                }

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                    scribanHelper["document"] = invoice;
                    string currEval = syncChannel.ParseSyntax.Filter;
                    bool evalRes = await ScriptHelper.strToBool(currEval, context);
                    if (!evalRes)
                    {
                        _skipDocument = true;
                        UI.Info($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"Skipping Invoice:[{invoice.DocumentNumber}] :: {currEval} >> {evalRes}");
                    }
                }

                string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingCurrencyCode}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
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
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                if (_skipDocument)
                {
                    arSaleTrx.RecordStatus = RecordStatus.INVALID;
                    arSaleTrx.Remark = $"Invoice skipped, should not be processed";
                }
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, _productSvc);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} MapSalesInvcAttribs error : {_strError}");
                    if (arSaleTrx.IsValid())
                    {
                        if (invoice.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Summary)
                        {
                            _skipDocument = true;
                            arSaleTrx.RecordStatus = RecordStatus.INVALID;
                        }
                        else
                            arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        arSaleTrx.Remark = _strError;
                    }
                    else if (invoice.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Summary)
                    {
                        _skipDocument = true;
                    }
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
                UI.Info($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"<< {saleBatchTrxKey.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                if (arSaleTrx.IsValid())
                {
                    if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                        arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                }
                var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveSaleReq, invoice);
                if (_skipDocument)
                {
                    salesTrxData.ClearRequestData();
                }
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
                UI.Error(ex, saleBatchTrxKey.DocNumber, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsSalesView, string>> GetConvertARCRNote(SaleBatchTrxKey saleBatchTrxKey, string srcPayload = null)
        {
            string _method_ = "GetConvertARCRNote";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice crNote = null;
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                bool _skipDocument = false;
                var syncChannel = _syncChannelMap[GeneralConst.AR_CRDRNOTE_SYNC];

                #region Sage300 section
                if (string.IsNullOrWhiteSpace(srcPayload) && (saleBatchTrxKey == null || string.IsNullOrWhiteSpace(saleBatchTrxKey.BatchNumber)))
                {
                    _strError = $"Invalid filter for ARInvoice => {JsonConvert.SerializeObject(saleBatchTrxKey)}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
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
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (invoiceBatches == null || invoiceBatches.InvoiceBatches.Count == 0)
                {
                    _strError = $"Not Found ARInvoices response from Sage for BatchNumber {saleBatchTrxKey.DocNumber}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    return _strError;
                }

                if (!string.IsNullOrWhiteSpace(srcPayload))
                {
                    crNote = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice>(srcPayload);
                    if (crNote.DocumentType != Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote)
                    {
                        _strError = $"Supplied ARDocument DocumentNumber:{crNote.DocumentNumber} is not an CreaditNote. It is a {crNote.DocumentType.ToString()}";
                        UI.Warn(crNote.DocumentNumber, _strError);
                        return _strError;
                    }
                    saleBatchTrxKey.BatchNumber = crNote.BatchNumber.ToString();
                    saleBatchTrxKey.DocNumber = crNote.DocumentNumber;
                }
                else
                {
                    var invBatch = invoiceBatches.InvoiceBatches.FirstOrDefault();
                    if (invBatch == null)
                    {
                        _strError = $"Missing ARInvoices response from Results for BatchNumber {saleBatchTrxKey.DocNumber}";
                        UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                        return _strError;
                    }
                    if (string.IsNullOrWhiteSpace(saleBatchTrxKey.DocNumber))
                        crNote = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote);
                    else
                        crNote = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                        Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote
                        && x.DocumentNumber == saleBatchTrxKey.DocNumber);
                }
                if (crNote == null)
                {
                    _strError = $"ARInvoices BatchNumber {saleBatchTrxKey.DocNumber} has no CreditNotes with DocumentNumber:{saleBatchTrxKey.DocNumber}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    return _strError;
                }

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                    scribanHelper["document"] = crNote;
                    string currEval = syncChannel.ParseSyntax.Filter;
                    bool evalRes = await ScriptHelper.strToBool(currEval, context);
                    if (!evalRes)
                    {
                        _skipDocument = true;
                        UI.Info($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"Skipping Invoice:[{crNote.DocumentNumber}] :: {currEval} >> {evalRes}");
                    }
                }

                string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingCurrencyCode}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
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
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                if (_skipDocument)
                {
                    arSaleTrx.RecordStatus = RecordStatus.INVALID;
                    arSaleTrx.Remark = $"Invoice skipped, should not be processed";
                }
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx, _productSvc);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} MapSalesInvcAttribs error : {_strError}");
                    if (arSaleTrx.IsValid())
                    {
                        if (crNote.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Summary)
                        {
                            _skipDocument = true;
                            arSaleTrx.RecordStatus = RecordStatus.INVALID;
                        }
                        else
                            arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        arSaleTrx.Remark = _strError;
                    }
                    else if (crNote.InvoiceType == Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.InvoiceTypeEnum.Summary)
                    {
                        _skipDocument = true;
                    }
                }
                else
                {
                    arSaleTrx = mapResult.GetValue();
                }

                var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.ApplytoDocument);
                if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                {
                    _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.ApplytoDocument} for OECRNote => {JsonConvert.SerializeObject(saleBatchTrxKey.DocNumber)}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    if (arSaleTrx.IsValid())
                    {
                        arSaleTrx.RecordStatus = RecordStatus.INVALID;
                        arSaleTrx.Remark = _strError;
                    }
                }
                else if (origInvoice.DocExchRate != arSaleTrx.DocExchRate)
                {
                    _strError = $"Exchange Rate Error, Invoice Rate {origInvoice.DocExchRate:N2} differs from Credit Note Rate {arSaleTrx.DocExchRate:N2}";
                    UI.Error($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {_strError}");
                    if (arSaleTrx.IsValid())
                    {
                        arSaleTrx.RecordStatus = RecordStatus.INVALID;
                        arSaleTrx.Remark = _strError;
                    }
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

                        // arSaleTrx.SalesItems.RemoveAll(x => x.ProductCode.Equals(kv.Key));
                        arSaleTrx.SalesItems.Add(finalItem);
                    }
                }
                #endregion

                var dTaxSaveCNoteReq = new DTaxSaveCNoteReq(_clientBranch, crNote, arSaleTrx, origInvoice, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"<< {saleBatchTrxKey.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");
                if (arSaleTrx.IsValid())
                {
                    if (arSaleTrx.RecordStatus == RecordStatus.NONE)
                        arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        arSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                }
                var salesTrxData = new SalesTrxData(arSaleTrx, dTaxSaveCNoteReq, crNote);
                if (_skipDocument)
                {
                    salesTrxData.ClearRequestData();
                }
                arSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"<< {saleBatchTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = arSaleTrx,
                    DTaxSaveCNoteReq = dTaxSaveCNoteReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{saleBatchTrxKey.BatchNumber}:{saleBatchTrxKey.DocNumber}", $"{_method_} error : {ex.GetBaseException()}");
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
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                bool _skipDocument = false;
                Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice invoice = null;
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];

                #region Sage300 section
                if (string.IsNullOrWhiteSpace(srcPayload) && (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber)))
                {
                    _strError = $"Invalid filter for OEInvoice => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
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
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                }

                if (!string.IsNullOrWhiteSpace(srcPayload))
                {
                    invoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice>(srcPayload);
                    saleTrxKey.DocNumber = invoice.InvoiceNumber;
                }
                else
                {
                    var result = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null || result.Invoices.Count == 0)
                    {
                        _strError = $"Not Found OEInvoices response from Sage for InvoiceNumber {saleTrxKey.DocNumber}";
                        UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                        return _strError;
                    }
                    invoice = result.Invoices.FirstOrDefault(i => i.InvoiceNumber == saleTrxKey.DocNumber);
                }
                if (invoice is null)
                {
                    _strError = $"Invoice Number {saleTrxKey.DocNumber} not found in Sage ERP";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }
                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper["document"] = invoice;
                    string currEval = syncChannel.ParseSyntax.Filter;
                    bool evalRes = await ScriptHelper.strToBool(currEval, context);
                    if (!evalRes)
                    {
                        _skipDocument = true;
                        UI.Info(saleTrxKey.DocNumber, $"Skipping Invoice:[{invoice.InvoiceNumber}] :: {currEval} against [{invoice.OrderNumber}] >> {evalRes}");
                    }
                }

                string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingTRCurrency}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
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
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                if (_skipDocument)
                {
                    oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                    oeSaleTrx.Remark = $"Invoice skipped, should not be processed";
                }
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx, _productSvc);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error(oeSaleTrx.DocNumber, $"{_method_} MapSalesInvcAttribs error : {_strError}");
                    if (oeSaleTrx.IsValid())
                    {
                        oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        oeSaleTrx.Remark = _strError;
                    }
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
                UI.Info(oeSaleTrx.DocNumber, $"<< {saleTrxKey.DocNumber} DTaxSaveSaleReq : {JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat)}");
                if (oeSaleTrx.IsValid())
                {
                    if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                        oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                }
                var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveSaleReq, invoice);
                if (_skipDocument)
                {
                    salesTrxData.ClearRequestData();
                }
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info(saleTrxKey.DocNumber, $"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = oeSaleTrx,
                    DTaxSaveSale = dTaxSaveSaleReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, saleTrxKey.DocNumber, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsSalesView, string>> GetConvertOECRNote(SaleTrxKey saleTrxKey, string srcPayload = null)
        {
            string _method_ = "GetConvertOECRNote";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
            Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote = null;
            ScribanHelper scribanHelper = null;
            TemplateContext context = null;
            try
            {
                bool _skipDocument = false;
                var syncChannel = _syncChannelMap[GeneralConst.OE_CRDRNOTE_SYNC];

                #region Sage300 section
                if (string.IsNullOrWhiteSpace(srcPayload) && (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber)))
                {
                    _strError = $"Invalid filter for OECRNote => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
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
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }
                taxGroupMap = gResult.GetValue();
                var authResult = await _masterDataSvc.GetActiveAuthorities();
                if (authResult.IsError)
                {
                    _strError = "Invalid TaxAuth Cache Setup";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }
                taxAuthKeys = authResult.GetValue();

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper = new ScribanHelper();
                    context = new TemplateContext() { MemberRenamer = member => member.Name };
                    context.PushGlobal(scribanHelper);
                }


                if (!string.IsNullOrWhiteSpace(srcPayload))
                {
                    crNote = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote>(srcPayload);
                    saleTrxKey.DocNumber = crNote.CreditDebitNoteNumber;
                }
                else
                {
                    var result = await client.ProcessGetReqBasicAsync<OECreditDebitNotes>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);
                    if (result == null || result.CreditDebitNotes.Count == 0)
                    {
                        _strError = $"Not Found OECreditDebitNotes response from Sage for CRNumber {saleTrxKey.DocNumber}";
                        UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                        return _strError;
                    }
                    crNote = result.CreditDebitNotes.FirstOrDefault(i => i.CreditDebitNoteNumber == saleTrxKey.DocNumber);
                }
                if (crNote is null)
                {
                    _strError = $"Credit Debit Note Number {saleTrxKey.DocNumber} not found in Sage ERP";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper["document"] = crNote;
                    string currEval = syncChannel.ParseSyntax.Filter;
                    bool evalRes = await ScriptHelper.strToBool(currEval, context);
                    if (!evalRes)
                    {
                        _skipDocument = true;
                        UI.Info(saleTrxKey.DocNumber, $"Skipping Invoice:[{crNote.CreditDebitNoteNumber}] :: {currEval} against [{crNote.OrderNumber}] >> {evalRes}");
                    }
                }

                string strTaxKey = $"{crNote.TaxGroup}:{crNote.TaxReportingTRCurrency}:Sales";
                if (!taxGroupMap.ContainsKey(strTaxKey))
                {
                    _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
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
                    UI.Error(saleTrxKey.DocNumber, $"{_method_} error : {_strError}");
                    return _strError;
                }
                _customer = sCustomer.GetValue();
                #endregion

                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, crNote, _taxGroup, taxAuthKeys);
                if (_skipDocument)
                {
                    oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                    oeSaleTrx.Remark = $"Invoice skipped, should not be processed";
                }
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx, _productSvc);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error(oeSaleTrx.DocNumber, $"{_method_} OECreditDebitNote:{oeSaleTrx.DocNumber}, MapSalesInvcAttribs error : {_strError}");
                    if (oeSaleTrx.IsValid())
                    {
                        oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                        oeSaleTrx.Remark = _strError;
                    }
                }
                else
                {
                    oeSaleTrx = mapResult.GetValue();
                }

                var origInvoice = await _dbContext.SalesTransact.FirstOrDefaultAsync(x => x.DocNumber == crNote.InvoiceNumber);
                if (origInvoice == null || string.IsNullOrWhiteSpace(origInvoice.ExternalID))
                {
                    _strError = $"Invalid/Unprocessed Parent Invoice : {crNote.InvoiceNumber} for OECRNote => {saleTrxKey.DocNumber}";
                    UI.Error(oeSaleTrx.DocNumber, $"{_method_} error : {_strError}");
                    if (oeSaleTrx.IsValid())
                    {
                        oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                        oeSaleTrx.Remark = _strError;
                    }
                }
                if (origInvoice.DocExchRate != oeSaleTrx.DocExchRate)
                {
                    _strError = $"Exchange Rate Error, Invoice Rate {origInvoice.DocExchRate:N2} differs from Credit Note Rate {oeSaleTrx.DocExchRate:N2}";
                    UI.Error(oeSaleTrx.DocNumber, $"{_method_} error : {_strError}");
                    if (oeSaleTrx.IsValid())
                    {
                        oeSaleTrx.RecordStatus = RecordStatus.INVALID;
                        oeSaleTrx.Remark = _strError;
                    }
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
                UI.Info(saleTrxKey.DocNumber, $"<< {saleTrxKey.DocNumber} DTaxSaveCNoteReq : {JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat)}");
                if (oeSaleTrx.IsValid())
                {
                    if (oeSaleTrx.RecordStatus == RecordStatus.NONE)
                        oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                    else
                        oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;
                }

                var salesTrxData = new SalesTrxData(oeSaleTrx, dTaxSaveCNoteReq, crNote);
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info(saleTrxKey.DocNumber, $"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = oeSaleTrx,
                    DTaxSaveCNoteReq = dTaxSaveCNoteReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, saleTrxKey.DocNumber, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

        }

        public async Task<Result<int, string>> SelectFilterInvoices(string jsonPayload)
        {
            string _method_ = "SelectFilterInvoices";
            string _strError = string.Empty;
            int result = -1;
            //string evalString = "EVALB:!string.starts_with list[_index_].OrderNumber \"PK\"";
            OEInvoices invList = null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"http://localhost/Sage300WebApi/v1.0/-/111079/OE/OEInvoices");

                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];

                /*var qParams = new Dictionary<string, string>();
                invList = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, null, qParams);*/
                invList = JsonConvert.DeserializeObject<OEInvoices>(jsonPayload);
                if (invList == null || invList.Invoices.Count == 0)
                {
                    _strError = $"Not Found OEInvoices response from Sage";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                result = invList.Invoices.Count;
                UI.Info($"{_method_} {invList.Invoices.Count} OEInvoices found");

                ScribanHelper scribanHelper = new ScribanHelper();
                var context = new TemplateContext() { MemberRenamer = member => member.Name };
                context.PushGlobal(scribanHelper);

                if (!string.IsNullOrWhiteSpace(syncChannel.ParseSyntax?.Filter))
                {
                    scribanHelper["list"] = invList.Invoices;
                    int skipped = 0;

                    for (int i = 0; i < invList.Invoices.Count; i++)
                    {
                        string currEval = syncChannel.ParseSyntax.Filter.Replace("_index_", i.ToString());
                        bool evalRes = await ScriptHelper.strToBool(currEval, context);
                        if (!evalRes)
                        {
                            //UI.Info($"{currEval} against [{invList.Invoices[i].OrderNumber}] >> {evalRes}");
                            invList.Invoices.RemoveAt(i);
                            skipped++;
                        }
                    }
                    if (skipped > 0)
                    {
                        UI.Warn($"{_method_} skipped {skipped} invoices against Filter: {syncChannel.ParseSyntax?.Filter}");
                    }
                }
                UI.Info($"{_method_} {invList.Invoices.Count} OEInvoices final to process");
                result = invList.Invoices.Count;

                return result;
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
                        ).AsSplitQuery().ToListAsync();
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
                            .SetProperty(x => x.QRText, saleTrxResp.ETimsURL)
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
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.DocNumber.Equals(saleCallback.CBData.TraderInvoiceNo));
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
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                changes += await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.CallbackTime, tStamp)
                            .SetProperty(x => x.CallbackPayload, JsonConvert.SerializeObject(saleCallback))
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                // Update RefCUNumber for CreditNote if applicable
                changes += await _dbContext.SalesTransact.Where(e => e.DocType == DocumentType.CREDITNOTE
                    && e.RefInvNumber == saleTransact.DocNumber && string.IsNullOrWhiteSpace(e.RefCUNumber))
                    .ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.RefCUNumber, cuNumber)
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
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.INVALID, RecordStatus.DEPENDS };

                if (string.IsNullOrWhiteSpace(filter.DocNumber) || string.IsNullOrWhiteSpace(filter.BranchCode))
                {
                    _strError = $"Invalid Filter Provided : [{filter.BranchCode}:{filter.DocNumber}]";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                var saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .Where(e => e.BranchCode.Equals(filter.BranchCode) && e.DocNumber.Equals(filter.DocNumber))
                    .AsSplitQuery().FirstOrDefaultAsync();
                if (saleTransact is null)
                {
                    _strError = $"No valid Sales Transaction found for Document: {filter.DocNumber}";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                transactSale = saleTransact.GetSalesTransact(_clientBranch);
                if (transactSale is null)
                {
                    _strError = $"No valid Sales Transaction generated for Document: {filter.DocNumber}";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                // check status before processing
                if (saleTransact.RecordStatus == RecordStatus.POST_OK //|| saleTransact.RecordStatus == RecordStatus.POST_FAIL
                    || !saleTransact.IsValid())
                {
                    _strError = $"Sales Transaction generation for Sale : [{filter.BranchCode}:{filter.DocNumber}] failed: invalid status";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                var invalidItems = await _dbContext.SalesItem.Where(x => x.SalesTrxID == saleTransact.SalesTrxID
                    && !completeStatii.Contains(x.RecordStatus)).ToListAsync();
                if (false && invalidItems?.Count > 0) // stop checking item status
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
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );
                                await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
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
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            _dbChanges += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ExternalID, saleTrxResp.ID)
                                .SetProperty(x => x.Remark, _remark)
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.CUNumber, cuNumber)
                                .SetProperty(x => x.QRText, saleTrxResp.ETimsURL)
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
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                            );
                            // Update RefCUNumber for CreditNote if applicable
                            _dbChanges += await _dbContext.SalesTransact.Where(e => e.DocType == DocumentType.CREDITNOTE 
                                && e.RefInvNumber == saleTransact.DocNumber && string.IsNullOrWhiteSpace(e.RefCUNumber))
                                .ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.RefCUNumber, cuNumber)
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
                        _strError = $"Sales Transaction invalid for Sale : [{filter.BranchCode}:{filter.DocNumber}] error: {_strError}";
                        UI.Error($"{_method_} error: {_strError}");
                        return _strError;
                    }
                    var parentSale = await _dbContext.SalesTransact.Where(e => e.BranchCode.Equals(filter.BranchCode) 
                        && e.DocNumber.Equals(saleTransact.RefInvNumber)).FirstOrDefaultAsync();
                    if (parentSale is null)
                    {
                        _strError = $"Sales Transaction not found for Sale : [{filter.BranchCode}:{filter.DocNumber}] error: {_strError}";
                        UI.Error($"{_method_} error: {_strError}");
                        return _strError;
                    }
                    if (!parentSale.IsTaxComplete())
                    {
                        _strError = $"Sales Transaction not completed for Sale : [{filter.BranchCode}:{filter.DocNumber}] error: {_strError}";
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
                                catch (Exception tex)
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
                                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                                );
                                await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
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
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
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
                                .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
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

        public async Task<Result<int, string>> PostReadyTaxTrxs()
        {
            string _method_ = "PostReadyTaxTrxs";
            try
            {
                int counter = 0;
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK,
                    RecordStatus.POST_FAIL, RecordStatus.INVALID };

                var queueList = _dbContext.SalesTransact
                    .Where(x => !completeStatii.Contains(x.RecordStatus) && x.Tries <= 3)
                    .AsEnumerable()
                    .Where(x => x.IsValid())
                    .OrderBy(x => x.CreatedOn).Take(10)
                    .Select(x => new QueueSaveSale() { BranchCode = x.BranchCode, DocNumber = x.DocNumber })
                    .ToList();
                counter = queueList.Count;
                UI.Debug($"{_method_} processing {queueList.Count} transactions");
                foreach (var item in queueList)
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

        public async Task<Result<SalesTransact, string>> SyncSaleTrx(SalesTransact saleTransact)
        {
            string _method_ = "SyncSaleTrx";
            string _strError = string.Empty;
            try
            {
                if (saleTransact is null)
                {
                    return $"Invalid SalesTransact provided for synchronization";
                }
                UI.Info($">> {_method_} DocNumber: {saleTransact.CacheKey}");

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
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );
                        _dbChanges += await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _remark)
                            .SetProperty(x => x.RecordStatus, RecordStatus.POST_OK)
                            .SetProperty(x => x.CUNumber, cuNumber)
                            .SetProperty(x => x.QRText, saleTrxResp.ETimsURL)
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
                            .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                        );

                        await _dbTrans.CommitAsync();
                        if (_dbChanges > 0)
                        {
                            saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                            .AsSplitQuery().FirstOrDefaultAsync(e => e.DocNumber == saleTransact.DocNumber);
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
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<int, string>> SyncReadyTaxTrxs()
        {
            string _method_ = "SyncReadyTaxTrxs";
            try
            {
                int counter = 0;
                UI.Debug($"{_method_} starting synchronization");
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.INVALID };
                
                var queueList = await _dbContext.SalesTransact.Include(x => x.SalesTrxData).Where(x => 
                    !completeStatii.Contains(x.RecordStatus) && x.SalesTrxData != null
                    && string.IsNullOrWhiteSpace(x.QRText) && !string.IsNullOrWhiteSpace(x.OfflineURL)
                    && !string.IsNullOrWhiteSpace(x.ExternalID) && EF.Functions.DateDiffSecond(x.LastTry,DateTime.Now) > 60)
                    .OrderBy(x => x.CreatedOn).Take(10).AsSplitQuery().ToListAsync();
                counter = queueList.Count;
                UI.Debug($"{_method_} resyncing {queueList.Count} transactions");
                foreach (var item in queueList)
                {
                    var result = await SyncSaleTrx(item);
                    if (result.IsError)
                    {
                        UI.Error($"{_method_} DocNumber:{item.DocNumber}, Error: {result.GetError()}");
                    }
                    else
                    {
                        UI.Info($"{_method_} DocNumber:{item.DocNumber} Synchronized Successfully");
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

    }
}
