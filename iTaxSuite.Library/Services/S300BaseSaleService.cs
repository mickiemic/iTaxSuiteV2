using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text;

namespace iTaxSuite.Library.Services
{
    public abstract class S300BaseSaleService
    {
        protected readonly IDatabase _baseDb;
        protected readonly IHttpClientFactory _httpClientFactory;

        protected readonly ETimsDBContext _dbContext;
        protected readonly ExtSystConfig _extSystConfig;
        protected readonly IMasterDataSvc _masterDataSvc;

        protected Dictionary<string, SyncChannel> _syncChannelMap;
        protected ClientBranch _clientBranch = null;

        protected S300BaseSaleService(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, ExtSystConfig extSystConfig,
            IMasterDataSvc masterDataSvc, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;
            _httpClientFactory = httpClientFactory;
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

        public async Task<Result<SalesTransact, string>> QuerySaleTransact(SaleTrxKey saleTrxKey, bool fixTransaction = true)
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
                    .AsSplitQuery().FirstOrDefaultAsync(e => e.DocNumber == saleTrxKey.DocNumber);
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

    }
}
