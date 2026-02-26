using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text;

namespace iTaxSuite.Library.Services
{
    public interface IS300SaleService
    {
        Task<Result<List<EtimsSalesView>, string>> FetchARCRNotes();
        Task<Result<List<EtimsSalesView>, string>> FetchARInvoices();
        Task<Result<List<EtimsSalesView>, string>> FetchOECRDRNotes();
        Task<Result<List<EtimsSalesView>, string>> FetchOEInvoices();
        Task<Result<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer, string>> GetARCustomer(SageDocFilter sageFilter);
        Task<Result<EtimsSalesView, string>> GetConvertARCRNote(SaleBatchTrxKey saleBatchTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertARInvoice(SaleBatchTrxKey saleBatchTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertOECRNote(SaleTrxKey saleTrxKey);
        Task<Result<EtimsSalesView, string>> GetConvertOEInvoice(SaleTrxKey saleTrxKey);
        Task<Result<SalesTransact, string>> GetQRImage(int salesTrxId, bool updateMeta = false);
        Task<Result<PagedResult<SalesTransact>, string>> GetSales(SalesFilter filter);
        Task<Result<EtimsTransact, string>> ProcessSaveSale(EtimsTransact transactSale);
        Task<Result<SalesTransact, string>> QuerySaleTransact(SaleTrxKey saleTrxKey, bool fixTransaction = true);
        Task<Result<EtimsTransact, string>> QueueSaveSale(QueueSaveSale filter);
        Task<Result<SalesTransact, string>> ReFetchOEInvoice(SaleTrxKey saleTrxKey);
    }
    public class S300SaleService : IS300SaleService
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IDatabase _baseDb;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExtSystConfig _extSystConfig;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IEtimsService _etimsService;

        private readonly Dictionary<string, SyncChannel> _syncChannelMap;
        private readonly ClientBranch _clientBranch = null;

        public S300SaleService(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IHttpClientFactory httpClientFactory, 
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

        public async Task<Result<PagedResult<SalesTransact>, string>> GetSales(SalesFilter filter)
        {
            string _method_ = "GetSales";
            PagedResult<SalesTransact> result = new();
            try
            {
                var query = _dbContext.SalesTransact.AsQueryable();
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
                if (!string.IsNullOrWhiteSpace(filter.CustNumber))
                    query = query.Where(x => x.CustNumber.Equals(filter.CustNumber));

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

        public async Task<Result<SalesTransact, string>> GetQRImage(int salesTrxId, bool updateMeta = false)
        {
            string _method_ = "GetQRImage";
            try
            {
                if (updateMeta)
                {
                    var xData = await _dbContext.SalesTransact.Include(x => x.SalesTrxData)
                        .Where(x => !string.IsNullOrWhiteSpace(x.SalesTrxData.ResponsePayload) 
                        //&& (x.QRImage == null || x.QRImage.Length == 0)
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
                }

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

        public async Task<Result<List<EtimsSalesView>, string>> FetchOEInvoices()
        {
            string _method_ = "FetchOEInvoices";
            List<EtimsSalesView> result = new();
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE")
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
                    if (invList == null)
                    {
                        _strError = "Null OEInvoices response from Sage";
                        UI.Error($"{_method_} : {_strError}");
                        return _strError;
                    }
                    loop = (invList.nextLink != null);
                    syncChannel.IncrOffSet(invList.Invoices.Count);

                    invList.Invoices.RemoveAll(i => invoiceMap.ContainsKey(i.InvoiceNumber));

                    foreach (var invoice in invList.Invoices)
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
                            UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                            return _strError;
                        }
                        oeSaleTrx = mapResult.GetValue();

                        var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                        if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                            oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                        else
                            oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                        var salesTrxData = new SalesTrxData(oeSaleTrx, trnsSalesSaveReq, invoice);
                        oeSaleTrx.SalesTrxData = salesTrxData;

                        var stockMovement = new StockMovement(_clientBranch, invoice);
                        var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                        var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                        stockMovement.StockMovData = stockTrxData;

                        var salesView = new EtimsSalesView
                        {
                            SalesTransact = oeSaleTrx,
                            StockMovement = stockMovement,
                            SalesSaveReq = trnsSalesSaveReq,
                            StockIOSaveReq = stockIOSaveReq
                        };
                        result.Add(salesView);

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
                                if (_dbContext.StockMovement.AddIfNotExists(stockMovement, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn($"OEInvoice {stockMovement.DocNumber} Already Exists");
                                    continue;
                                }
                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);
                                syncChannel.UpdateTracker(oeSaleTrx.DocNumber);

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
                                    throw new Exception($"OEInvoice {stockMovement.DocNumber} saving to database failed");
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
                    }

                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return result;
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
                var salesTrxData = oeSaleTrx.SalesTrxData;
                if (salesTrxData is null)
                {
                    _strError = $"Invalid or missing OEInvoice {saleTrxKey.DocNumber} in SalesTrxData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

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

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(trnsSalesSaveReq);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }
                trnsSalesSaveReq = mapResult.GetValue();
                var _oldSaveSalesRes = JsonConvert.DeserializeObject<TrnsSalesSaveReq>(salesTrxData.RequestPayload);
                if (!_oldSaveSalesRes.HasEqualValue(trnsSalesSaveReq))
                {
                    salesTrxData.RequestPayload = JsonConvert.SerializeObject(trnsSalesSaveReq, new DecimalFormatConverter());
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
        public async Task<Result<List<EtimsSalesView>, string>> FetchOECRDRNotes()
        {
            string _method_ = "FetchOECRDRNotes";
            List<EtimsSalesView> result = new();
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "OE")
                    .ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, new DateTime(2024, 12, 01).Date.ToString("s"));

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
                    if (result == null)
                    {
                        _strError = "Null OECreditDebitNotes response from Sage";
                        UI.Error($"{_method_} : {_strError}");
                        return _strError;
                    }
                    loop = (crNoteList.nextLink != null);
                    syncChannel.IncrOffSet(crNoteList.CreditDebitNotes.Count);

                    crNoteList.CreditDebitNotes.RemoveAll(i => invoiceMap.ContainsKey(i.InvoiceNumber));

                    foreach (var invoice in crNoteList.CreditDebitNotes)
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
                            UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                            return _strError;
                        }
                        oeSaleTrx = mapResult.GetValue();

                        var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                        if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                            oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                        else
                            oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                        var salesTrxData = new SalesTrxData(oeSaleTrx, trnsSalesSaveReq, invoice);
                        oeSaleTrx.SalesTrxData = salesTrxData;

                        var stockMovement = new StockMovement(_clientBranch, invoice);
                        var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                        var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                        stockMovement.StockMovData = stockTrxData;
                        var salesView = new EtimsSalesView
                        {
                            SalesTransact = oeSaleTrx,
                            StockMovement = stockMovement,
                            SalesSaveReq = trnsSalesSaveReq,
                            StockIOSaveReq = stockIOSaveReq
                        };
                        result.Add(salesView);

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
                                if (_dbContext.StockMovement.AddIfNotExists(stockMovement, p => p.DocNumber == oeSaleTrx.DocNumber) == null)
                                {
                                    UI.Warn($"OEInvoice {stockMovement.DocNumber} Already Exists");
                                    continue;
                                }
                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);
                                syncChannel.UpdateTracker(oeSaleTrx.DocNumber);

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
                                    throw new Exception($"OEInvoice {stockMovement.DocNumber} saving to database failed");
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
                    }

                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }

            return result;
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
                var syncChannel = _syncChannelMap[GeneralConst.AR_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.Where(e => e.SourceApp == "AR")
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
                while (loop)
                {
                    qParams["$skip"] = syncChannel.OffSet.ToString();
                    //TODO: decide on how many to take at a go
                    var invoiceBatches = await client.ProcessGetReqBasicAsync<ARInvoiceBatches>(_reqUrl, _extSystConfig.Username,
                        _extSystConfig.Password, null, qParams);
                    if (invoiceBatches == null && !invoiceBatches.InvoiceBatches.Any())
                    {
                        _strError = $"Not Found ARInvoices response from Sage";
                        UI.Error($"{_method_} error : {_strError}");
                        return _strError;
                    }
                    
                    foreach(var invBatch in invoiceBatches.InvoiceBatches)
                    {
                        var invoices = invBatch.Invoices.Where(x => x.DocumentType ==
                            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.Invoice).ToList();
                        if (invoices == null || invoices.Count == 0)
                        {
                            _strError = $"ARInvoices BatchNumber {invBatch.BatchNumber} has no Invoices";
                            UI.Error($"{_method_} error : {_strError}");
                            continue;
                        }

                        foreach(var invoice in invoices)
                        {
                            string strTaxKey = $"{invoice.TaxGroup}:{invoice.TaxReportingCurrencyCode}:Sales";
                            if (!taxGroupMap.ContainsKey(strTaxKey))
                            {
                                _strError = $"Tax Setup Missing GroupKey {strTaxKey}";
                                UI.Error($"{_method_} error : {_strError}");
                                continue;
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
                                continue;
                            }
                            _customer = sCustomer.GetValue();

                            var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                            var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx);
                            if (mapResult.IsError)
                            {
                                _strError = mapResult.GetError();
                                UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                                continue;
                            }
                            arSaleTrx = mapResult.GetValue();

                            var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, arSaleTrx, _taxGroup, taxAuthKeys, _customer);
                            UI.Info($"<< {invoice.DocumentNumber} TrnsSalesSaveReq : {JsonConvert.SerializeObject(trnsSalesSaveReq, decimalFormat)}");

                            if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                                arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                            else
                                arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                            var salesTrxData = new SalesTrxData(arSaleTrx, trnsSalesSaveReq, invoice);
                            arSaleTrx.SalesTrxData = salesTrxData;
                            UI.Info($"<< {invoice.DocumentNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                            var stockMovement = new StockMovement(_clientBranch, invoice);
                            var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                            var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                            stockMovement.StockMovData = stockTrxData;
                            UI.Info($"<< {invoice.DocumentNumber} StockMovement : {JsonConvert.SerializeObject(stockMovement, decimalFormat)}");

                            using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                            {
                                int _etrSeqValue = _clientBranch.SaleInvoiceSeq;
                                try
                                {
                                    if (_dbContext.SalesTransact.AddIfNotExists(arSaleTrx, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"ARInvoice {arSaleTrx.DocNumber} Already Exists");
                                        continue;
                                    }
                                    _dbContext.Attach(_clientBranch);
                                    if (_dbContext.SaveChanges() < 1)
                                    {
                                        throw new Exception($"ARInvoice {arSaleTrx.DocNumber} saving to database failed");
                                    }
                                    if (_dbContext.StockMovement.AddIfNotExists(stockMovement, p => p.DocNumber == arSaleTrx.DocNumber) == null)
                                    {
                                        UI.Warn($"ARInvoice {stockMovement.DocNumber} Already Exists");
                                        continue;
                                    }
                                    _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);
                                    syncChannel.UpdateTracker(arSaleTrx.DocNumber);

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
                                        throw new Exception($"ARInvoice {stockMovement.DocNumber} saving to database failed");
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
                                StockMovement = stockMovement,
                                SalesSaveReq = trnsSalesSaveReq,
                                StockIOSaveReq = stockIOSaveReq
                            };
                            results.Add(salesView);
                        }
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
                var syncChannel = _syncChannelMap[GeneralConst.AR_CRDRNOTE_SYNC];
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
            throw new Exception("Unimplemnted");
        }
        public async Task<Result<SalesTransact,string>> QuerySaleTransact(SaleTrxKey saleTrxKey, bool fixTransaction = true)
        {
            string _method_ = "QuerySaleTransact";
            string _strError = string.Empty;
            SalesTransact saleTrx = null;
            try
            {
                var okStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL };

                if (saleTrxKey == null || string.IsNullOrWhiteSpace(saleTrxKey.DocNumber))
                {
                    _strError = $"Invalid filter for SaleTransact => {JsonConvert.SerializeObject(saleTrxKey)}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                saleTrx = await _dbContext.SalesTransact.Include(e => e.SalesTrxData).Include(x => x.SalesItems)
                    .FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
                if (saleTrx is null)
                {
                    _strError = $"Invalid or missing SalesTransact {saleTrx.DocNumber} in SalesTransact data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                if (!okStatii.Contains(saleTrx.RecordStatus))
                {
                    var itemStatii = new HashSet<RecordStatus>();
                    //saleTrx.SalesItems[0].RecordStatus = RecordStatus.INVALID;
                    saleTrx.SalesItems.ForEach(x => itemStatii.Add(x.RecordStatus));
                    if (fixTransaction && itemStatii.Count == 1 && okStatii.Contains(itemStatii.ElementAt(0)))
                    {
                        var oldStatus = saleTrx.RecordStatus;
                        saleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                        await _dbContext.SaveChangesAsync();
                        UI.Info($"{_method_} Updating Status for DocNumber:{saleTrx.DocNumber} {oldStatus} -> {saleTrx.RecordStatus}");
                    }
                    
                    if (itemStatii.Except(okStatii).Count() > 0)
                    {
                        var sb = new StringBuilder();
                        foreach (var item in saleTrx.SalesItems.Where(x => !okStatii.Contains(x.RecordStatus)))
                        {
                            sb.AppendLine($"ProductCode:{item.ProductCode}, Name:[{item.Description}] status is not ok");
                        }
                        return sb.ToString();
                    }
                }

                return saleTrx;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<EtimsSalesView, string>> GetConvertOEInvoice(SaleTrxKey saleTrxKey)
        {
            string _method_ = "GetConvertOEInvoice";
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            var decimalFormat = new DecimalFormatConverter();
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

                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    return _strError;
                }
                oeSaleTrx = mapResult.GetValue();

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleTrxKey.DocNumber} TrnsSalesSaveReq : {JsonConvert.SerializeObject(trnsSalesSaveReq, decimalFormat)}");
                if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                    oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(oeSaleTrx, trnsSalesSaveReq, invoice);
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var stockMovement = new StockMovement(_clientBranch, invoice);
                var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} StockMovement : {JsonConvert.SerializeObject(stockMovement, decimalFormat)}");

                var salesView = new EtimsSalesView { SalesTransact = oeSaleTrx, StockMovement = stockMovement,
                    SalesSaveReq = trnsSalesSaveReq, StockIOSaveReq = stockIOSaveReq};
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

                var invoice = result.CreditDebitNotes.FirstOrDefault(i => i.CreditDebitNoteNumber == saleTrxKey.DocNumber);
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
                var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(oeSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    return _strError;
                }
                oeSaleTrx = mapResult.GetValue();

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, oeSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleTrxKey.DocNumber} TrnsSalesSaveReq : {JsonConvert.SerializeObject(trnsSalesSaveReq, decimalFormat)}");
                if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                    oeSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    oeSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(oeSaleTrx, trnsSalesSaveReq, invoice);
                oeSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(oeSaleTrx, decimalFormat)}");

                var stockMovement = new StockMovement(_clientBranch, invoice);
                var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;
                UI.Info($"<< {saleTrxKey.DocNumber} StockMovement : {JsonConvert.SerializeObject(stockMovement, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = oeSaleTrx,
                    StockMovement = stockMovement,
                    SalesSaveReq = trnsSalesSaveReq,
                    StockIOSaveReq = stockIOSaveReq
                };
                return salesView;
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
                if (invoiceBatches == null && !invoiceBatches.InvoiceBatches.Any())
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

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    return _strError;
                }
                arSaleTrx = mapResult.GetValue();

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, arSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleBatchTrxKey.DocNumber} TrnsSalesSaveReq : {JsonConvert.SerializeObject(trnsSalesSaveReq, decimalFormat)}");

                if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                    arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(arSaleTrx, trnsSalesSaveReq, invoice);
                arSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                var stockMovement = new StockMovement(_clientBranch, invoice);
                var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} StockMovement : {JsonConvert.SerializeObject(stockMovement, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = arSaleTrx,
                    StockMovement = stockMovement,
                    SalesSaveReq = trnsSalesSaveReq,
                    StockIOSaveReq = stockIOSaveReq
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
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice invoice = null;
            try
            {
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
                if (invoiceBatches == null && !invoiceBatches.InvoiceBatches.Any())
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
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote);
                else
                    invoice = invBatch.Invoices.FirstOrDefault(x => x.DocumentType ==
                    Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice.DocumentTypeEnum.CreditNote
                    && x.DocumentNumber == saleBatchTrxKey.DocNumber);
                if (invoice == null)
                {
                    _strError = $"ARInvoices BatchNumber {saleBatchTrxKey.DocNumber} has no CreditNotes with DocumentNumber:{saleBatchTrxKey.DocNumber}";
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

                var arSaleTrx = new SalesTransact(_clientBranch, _customer, invoice, _taxGroup, taxAuthKeys);
                var mapResult = await _masterDataSvc.MapSalesInvcAttribs(arSaleTrx);
                if (mapResult.IsError)
                {
                    _strError = mapResult.GetError();
                    UI.Error($"{_method_} MapSalesInvcAttribs error : {_strError}");
                    return _strError;
                }
                arSaleTrx = mapResult.GetValue();

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, arSaleTrx, _taxGroup, taxAuthKeys, _customer);
                UI.Info($"<< {saleBatchTrxKey.DocNumber} TrnsSalesSaveReq : {JsonConvert.SerializeObject(trnsSalesSaveReq, decimalFormat)}");

                if (trnsSalesSaveReq.RecordStatus == RecordStatus.NONE)
                    arSaleTrx.RecordStatus = RecordStatus.QUEUEDOUT;
                else
                    arSaleTrx.RecordStatus = RecordStatus.DEPENDS;

                var salesTrxData = new SalesTrxData(arSaleTrx, trnsSalesSaveReq, invoice);
                arSaleTrx.SalesTrxData = salesTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} SalesTransact : {JsonConvert.SerializeObject(arSaleTrx, decimalFormat)}");

                var stockMovement = new StockMovement(_clientBranch, invoice);
                var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                stockMovement.StockMovData = stockTrxData;
                UI.Info($"<< {saleBatchTrxKey.DocNumber} StockMovement : {JsonConvert.SerializeObject(stockMovement, decimalFormat)}");

                var salesView = new EtimsSalesView
                {
                    SalesTransact = arSaleTrx,
                    StockMovement = stockMovement,
                    SalesSaveReq = trnsSalesSaveReq,
                    StockIOSaveReq = stockIOSaveReq
                };
                return salesView;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer, string>> GetARCustomer(SageDocFilter sageFilter)
        {
            string _method_ = "GetARCustomer";
            string _strError = null;
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer result = null;
            try
            {
                if (sageFilter == null || !sageFilter.IsValid)
                {
                    _strError = $"Invalid Customer Number: {sageFilter.docNumber}";
                    UI.Error($"{_method_} : {_strError}");
                    return _strError;
                }

                var qParams = new Dictionary<string, string>();
                string strFilter = sageFilter.GetFilterString();
                if (!string.IsNullOrWhiteSpace(strFilter))
                    qParams["$filter"] = strFilter;

                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/AR/ARCustomers");
                var customerList = await client.ProcessGetReqBasicAsync<ARCustomers>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                            null, qParams);
                if (customerList == null && !customerList.Customers.Any())
                {
                    _strError = $"Not Found ARCustomers response from Sage for CustomerNumber {sageFilter.docNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                result = customerList.Customers.FirstOrDefault();
                if (result == null)
                {
                    _strError = $"Missing ARCustomers response from Results for CustomerNumber {sageFilter.docNumber}";
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

        public async Task<Result<EtimsTransact, string>> QueueSaveSale(QueueSaveSale filter)
        {
            string _method_ = "QueueSaveSale";
            string _strError = null;
            EtimsTransact transactSale = null, transactStock = null;
            try
            {
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
                if (saleTransact.RecordStatus == RecordStatus.POST_OK || saleTransact.RecordStatus == RecordStatus.POST_FAIL
                    || saleTransact.RecordStatus == RecordStatus.POST_DUPL || !saleTransact.IsValid())
                {
                    _strError = $"EtimsTransact generation for Sale : [{filter.BranchCode}:{filter.DocNumber}] failed: invalid status";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }

                transactStock = saleTransact.GetStockTransact(_clientBranch);
                if (transactStock is null)
                {
                    _strError = $"No valid StockIO transaction generated for Document: {filter.DocNumber}";
                    UI.Error($"{_method_} error: {_strError}");
                    return _strError;
                }
                
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (_dbContext.EtimsTransacts.AddIfNotExists(transactSale, x => x.DocNumber == transactSale.DocNumber
                            && x.ReqType == transactSale.ReqType && x.BranchCode == transactSale.BranchCode && x.DocStamp == transactSale.DocStamp) == null)
                        {
                            _strError = $"EtimsTransaction for Document: {filter.DocNumber} Already Exists";
                            UI.Error($"{_method_} error: {_strError}");
                            return _strError;
                        }
                        if (_dbContext.EtimsTransacts.AddIfNotExists(transactStock, x => x.DocNumber == transactStock.DocNumber
                            && x.ReqType == transactStock.ReqType && x.BranchCode == transactStock.BranchCode && x.DocStamp == transactStock.DocStamp) == null)
                        {
                            _strError = $"EtimsTransaction for Document: {filter.DocNumber} Already Exists";
                            UI.Error($"{_method_} error: {_strError}");
                            return _strError;
                        }
                        if (_dbContext.SaveChanges() < 1)
                        {
                            throw new Exception($"EtimsTransaction {filter.DocNumber} saving to database failed");
                        }
                        await _dbTrans.CommitAsync();
                    }
                    catch (Exception iex)
                    {
                        await _dbTrans.RollbackAsync();
                        UI.Error(iex, $"{_method_} save valid record error : {iex.GetBaseException().Message}");
                        throw;
                    }
                }
                return transactSale;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsTransact,string>> ProcessSaveSale(EtimsTransact transactSale)
        {
            string _method_ = "ProcessSaveSale";
            string _strError = string.Empty;
            try
            {
                var _saleParts = transactSale.DocNumber.Split(":");
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL };

                // Get Sale Item
                var saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .Where(e => e.BranchCode == _saleParts[0] && e.DocNumber == _saleParts[1])
                    .OrderBy(e => e.CreatedOn).AsNoTracking().FirstOrDefaultAsync();
                var etimsReqOne = saleTransact.SalesTrxData.GetEtimsRequest();

                var etimsRespSale = await _etimsService.SaveEtimsSale(etimsReqOne);
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var tStamp = DateTime.Now;
                        var recordStatus = RecordStatus.POST_FAIL;

                        if (etimsRespSale.IsError)
                        {
                            _strError = etimsRespSale.GetError();
                            UI.Error($"Saving SaleTransact: {saleTransact.CacheKey} failed: {etimsRespSale.GetError()}");
                            transactSale.RespPayload = _strError;

                            // Update & Save Transact Changes
                            await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactSale.DocNumber && x.ReqType == transactSale.ReqType
                                && x.BranchCode == transactSale.BranchCode && x.DocStamp == transactSale.DocStamp).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
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
                        
                        TrnsSalesSaveResp saleTrxResp = etimsRespSale.GetValue();
                        transactSale.RespPayload = saleTrxResp.RawResponse;
                        if (saleTrxResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;
                        else if (saleTrxResp.IsDuplicate)
                            recordStatus = RecordStatus.POST_DUPL;

                        var cuNumber = saleTrxResp.GetCUNumber(_clientBranch);
                        if (string.IsNullOrWhiteSpace(cuNumber))
                        {
                            _strError = $"No Valid CUNumber Generated for receipt: {saleTransact.SalesTrxID}";
                            UI.Error($"{_method_} error: {saleTransact.SalesTrxID}");
                        }
                        byte[] qrData = null;
                        string qrText = saleTrxResp.GetQRText(_clientBranch);
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

                        // Update & Save Transact Changes
                        await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactSale.DocNumber && x.ReqType == transactSale.ReqType
                            && x.BranchCode == transactSale.BranchCode && x.DocStamp == transactSale.DocStamp).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        if (saleTrxResp.IsDuplicate && !string.IsNullOrWhiteSpace(saleTransact.SalesTrxData.RequestPayload))
                        {
                            var oldEtrResp = JsonConvert.DeserializeObject<TrnsSalesSaveResp>(saleTransact.SalesTrxData.RequestPayload);
                            if (!oldEtrResp.IsSuccess) // Only update when the newer response is better than the old one
                            {
                                await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                    .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                                    .SetProperty(x => x.ResponseTime, tStamp)
                                    .SetProperty(x => x.UpdatedOn, tStamp)
                                    .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                                );
                            }
                        }
                        else
                        {
                            await _dbContext.SalesTrxData.Where(e => e.SalesTrxID == saleTransact.SalesTrxID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                        }
                        await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saleTrxResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.CUNumber, cuNumber)
                            .SetProperty(x => x.QRText, qrText)
                            .SetProperty(x => x.QRTime, tStamp)
                            .SetProperty(x => x.QRImage, qrData)
                            .SetProperty(x => x.SDCID, saleTrxResp.Data.sdcId)
                            .SetProperty(x => x.InternalData, saleTrxResp.Data.InternalData)
                            .SetProperty(x => x.ReceiptNumber, saleTrxResp.Data.ReceiptNumber)
                            .SetProperty(x => x.ReceiptSignature, saleTrxResp.Data.ReceiptSignature)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        if (!saleTrxResp.IsSuccess && !saleTrxResp.IsDuplicate)
                        {
                            await _dbTrans.CommitAsync();
                            _strError = etimsRespSale.GetError();
                            UI.Error($"Saving SaleTransact: {saleTransact.CacheKey} failed: {etimsRespSale.GetError()}");
                            return _strError;
                        }

                        // *** Process StockIO
                        var transactIO = await _dbContext.EtimsTransacts.Where(e => e.ParentKey == transactSale.ReqKey)
                            .OrderBy(e => e.CreatedOn).AsNoTracking().FirstOrDefaultAsync();
                        var _ioParts = transactIO.DocNumber.Split(":");
                        if (completeStatii.Contains(transactIO.RecordStatus))
                        {
                            UI.Warn($"IO EtimsTransact for DocNumber {transactSale.DocNumber} is already processed.");
                            return transactSale;
                        }

                        // Get IO Transaction
                        var stockMovement = await _dbContext.StockMovement.Include(e => e.StockMovData)
                            .Where(e => e.BranchCode == _ioParts[0] && e.DocNumber == transactSale.DocNumber).OrderBy(e => e.CreatedOn)
                            .AsNoTracking().FirstOrDefaultAsync();
                        var etimsReqTwo = stockMovement.StockMovData.GetEtimsRequest();

                        var eTimsRespIO = await _etimsService.SaveEtimsStockIO(etimsReqTwo);
                        tStamp = DateTime.Now;
                        if (eTimsRespIO.IsError)
                        {
                            recordStatus = RecordStatus.POST_FAIL;
                            _strError = eTimsRespIO.GetError();
                            UI.Error($"Saving StockMovement: {stockMovement.CacheKey} failed: {eTimsRespIO.GetError()}");

                            // Update & Save Transact Changes
                            await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactIO.DocNumber && x.ReqType == transactIO.ReqType
                                && x.BranchCode == transactIO.BranchCode && x.DocStamp == transactIO.DocStamp).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.RecordStatus, recordStatus)
                                .SetProperty(x => x.Tries, x => x.Tries + 1)
                                .SetProperty(x => x.LastTry, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                            await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
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

                        StockIOSaveResp stockIOSaveResp = eTimsRespIO.GetValue();
                        if (stockIOSaveResp.IsSuccess)
                            recordStatus = RecordStatus.POST_OK;

                        await _dbContext.EtimsTransacts.Where(x => x.DocNumber == transactIO.DocNumber && x.ReqType == transactIO.ReqType
                            && x.BranchCode == transactIO.BranchCode && x.DocStamp == transactIO.DocStamp).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockMovData.Where(e => e.MovementID == stockMovement.MovementID).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, stockIOSaveResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockMovement.Where(e => e.MovementID == stockMovement.MovementID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, stockIOSaveResp.ResultMsg)
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

                return transactSale;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

    }
}
