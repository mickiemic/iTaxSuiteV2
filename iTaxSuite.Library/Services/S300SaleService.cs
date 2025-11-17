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
    public interface IS300SaleService
    {
        Task<Result<OEInvoices, string>> FetchOEInvoices();
        Task<Result<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer, string>> GetARCustomer(SageDocFilter sageFilter);
        Task<Result<PagedResult<SalesTransact>, string>> GetSales(SalesFilter filter);
        Task<Result<EtimsTransact, string>> ProcessSaveSale(EtimsTransact transactSale);
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

        public async Task<Result<OEInvoices, string>> FetchOEInvoices()
        {
            string _method_ = "FetchOEInvoices";
            OEInvoices result = null;
            string _strError = string.Empty;
            Dictionary<string, S300TaxGroup> taxGroupMap = null;
            HashSet<string> taxAuthKeys = null;
            try
            {
                var syncChannel = _syncChannelMap[GeneralConst.OE_INVOICE_SYNC];
                var invoiceMap = await _dbContext.SalesTransact.ToDictionaryAsync(e => e.DocNumber, e => e.DocStamp);
                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = string.Format("{0} ge {1}Z", syncChannel.DateCol, new DateTime(2024,12,01).Date.ToString("s"));

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
                    result = await client.ProcessGetReqBasicAsync<OEInvoices>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password,
                        null, qParams);
                    if (result == null)
                    {
                        _strError = "Null OEInvoices response from Sage";
                        UI.Error($"{_method_} : {_strError}");
                        return _strError;
                    }
                    loop = (result.nextLink != null);
                    syncChannel.IncrOffSet(result.Invoices.Count);

                    result.Invoices.RemoveAll(i => invoiceMap.ContainsKey(i.InvoiceNumber));

                    foreach (var invoice in result.Invoices)
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

                        var oeSaleTrx = new SalesTransact(_clientBranch, _customer, invoice);
                        
                        var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, _taxGroup, taxAuthKeys, _customer);
                        var mapResult = await _masterDataSvc.MapSalesInvcAttribs(trnsSalesSaveReq);
                        if (mapResult.IsError)
                        {
                            _strError = mapResult.GetError();
                            UI.Error($"{_method_} error : {_strError}");
                            return _strError;
                        }
                        trnsSalesSaveReq = mapResult.GetValue();

                        var salesTrxData = new SalesTrxData(oeSaleTrx, trnsSalesSaveReq, invoice);
                        oeSaleTrx.SalesTrxData = salesTrxData;

                        var stockMovement = new StockMovement(_clientBranch, invoice);
                        var stockIOSaveReq = new StockIOSaveReq(_clientBranch, invoice, trnsSalesSaveReq);
                        var stockTrxData = new StockMovData(stockMovement, invoice, stockIOSaveReq);
                        stockMovement.StockMovData = stockTrxData;

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
                                    UI.Warn($"StockMovement {stockMovement.DocNumber} Already Exists");
                                    continue;
                                }
                                if (_dbContext.SaveChanges() < 1)
                                {
                                    throw new Exception($"StockMovement {stockMovement.DocNumber} saving to database failed");
                                }

                                syncChannel.UpdateTracker(oeSaleTrx.DocNumber);
                                _clientBranch.SaleInvoiceSeq = (_etrSeqValue + 1);

                                await _dbTrans.CommitAsync();
                                _dbContext.ChangeTracker.Clear();
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

                    if (!await _masterDataSvc.SaveSyncChannel(syncChannel))
                    {
                        UI.Error($"{_method_} - SaveSyncSchedule : Failed Updating ItemsSync");
                    }
                    if (!await _masterDataSvc.UpdateBranchAsync(_clientBranch))
                    {
                        UI.Error($"{_method_} - UpdateBranchAsync : Failed Updating ClientBranch Details");
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
                    _strError = $"Invalid or missing OEInvoice {oeSaleTrx.DocNumber} in SalesTrxData data";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var qParams = new Dictionary<string, string>();
                qParams["$filter"] = $"InvoiceNumber eq '{oeSaleTrx.DocNumber}'";

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
                    _strError = $"Not Found OEInvoices response from Sage for InvoiceNumber {oeSaleTrx.DocNumber}";
                    UI.Error($"{_method_} error : {_strError}");
                    return _strError;
                }

                var invoice = result.Invoices.FirstOrDefault(i => i.InvoiceNumber == oeSaleTrx.DocNumber);
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

                var trnsSalesSaveReq = new TrnsSalesSaveReq(_clientBranch, invoice, _taxGroup, taxAuthKeys, _customer);
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
                    salesTrxData.RequestPayload = JsonConvert.SerializeObject(trnsSalesSaveReq);
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
            EtimsTransact transactSale = null, transactStock = null;
            try
            {
                if (string.IsNullOrWhiteSpace(filter.DocNumber) || string.IsNullOrWhiteSpace(filter.BranchCode))
                    return $"Invalid Filter Provided : [{filter.BranchCode}:{filter.DocNumber}]";

                var saleTransact = await _dbContext.SalesTransact.Include(e => e.SalesTrxData)
                    .Where(e => e.BranchCode.Equals(filter.BranchCode) && e.DocNumber.Equals(filter.DocNumber))
                    .FirstOrDefaultAsync();
                if (saleTransact is null)
                    return $"No valid stock item found for Document: {filter.DocNumber}";
                //TODO: Check Status before queueing

                transactSale = saleTransact.GetSalesTransact(_clientBranch);
                if (transactSale is null)
                    return $"No valid Sales transaction generated for Document: {filter.DocNumber}";

                transactStock = saleTransact.GetStockTransact(_clientBranch);
                if (transactStock is null)
                    return $"No valid StockIO transaction generated for Document: {filter.DocNumber}";

                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (_dbContext.EtimsTransacts.AddIfNotExists(transactSale, x => x.DocNumber == transactSale.DocNumber
                            && x.ReqType == transactSale.ReqType && x.BranchCode == transactSale.BranchCode && x.DocStamp == transactSale.DocStamp) == null)
                        {
                            return $"EtimsTransaction for Document: {filter.DocNumber} Already Exists";
                        }
                        if (_dbContext.EtimsTransacts.AddIfNotExists(transactStock, x => x.DocNumber == transactStock.DocNumber
                            && x.ReqType == transactStock.ReqType && x.BranchCode == transactStock.BranchCode && x.DocStamp == transactStock.DocStamp) == null)
                        {
                            return $"EtimsTransaction for Document: {filter.DocNumber} Already Exists";
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
                            .SetProperty(x => x.ResponsePayload, saleTrxResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                        );
                        await _dbContext.SalesTransact.Where(e => e.SalesTrxID == saleTransact.SalesTrxID)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saleTrxResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbTrans.CommitAsync();

                        if (!saleTrxResp.IsSuccess && !saleTrxResp.IsDuplicate)
                        { 
                            _strError = etimsRespSale.GetError();
                            UI.Error($"Saving SaleTransact: {saleTransact.CacheKey} failed: {etimsRespSale.GetError()}");
                            return _strError;
                        }

                        // *** Process StockIO
                        var transactIO = await _dbContext.EtimsTransacts.Where(e => e.ParentKey == transactSale.ReqKey)
                            .OrderBy(e => e.CreatedOn).AsNoTracking().FirstOrDefaultAsync();
                        var _ioParts = transactSale.DocNumber.Split(":");
                        if (completeStatii.Contains(transactIO.RecordStatus))
                        {
                            UI.Warn($"IO EtimsTransact for DocNumber {transactSale.DocNumber} is already processed.");
                            return transactSale;
                        }

                        // Get IO Transaction
                        var stockMovement = await _dbContext.StockMovement.Include(e => e.StockMovData)
                            .Where(e => e.BranchCode == _ioParts[0] && e.DocNumber == _ioParts[1]).OrderBy(e => e.CreatedOn)
                            .AsNoTracking().FirstOrDefaultAsync();
                        var etimsReqTwo = stockMovement.StockMovData.GetEtimsRequest();

                        var eTimsRespIO = await _etimsService.SaveEtimsStockIO(etimsReqTwo);
                        tStamp = DateTime.Now;
                        if (eTimsRespIO.IsError)
                        {
                            recordStatus = RecordStatus.POST_FAIL;
                            _strError = etimsRespSale.GetError();
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
                            .SetProperty(x => x.ResponsePayload, _strError)
                            .SetProperty(x => x.ResponseTime, tStamp)
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
