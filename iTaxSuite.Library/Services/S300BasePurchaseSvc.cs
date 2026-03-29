using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public abstract class S300BasePurchaseSvc
    {
        protected readonly IDatabase _baseDb;
        protected readonly IHttpClientFactory _httpClientFactory;

        protected readonly ETimsDBContext _dbContext;
        protected readonly ExtSystConfig _extSystConfig;
        protected readonly IMasterDataSvc _masterDataSvc;

        protected Dictionary<string, SyncChannel> _syncChannelMap;
        protected ClientBranch _clientBranch = null;

        protected S300BasePurchaseSvc(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, ExtSystConfig extSystConfig,
            IMasterDataSvc masterDataSvc, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _extSystConfig = extSystConfig;
            _masterDataSvc = masterDataSvc;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<PagedResult<PurchTransact>, string>> GetPurchases(PurchaseFilter filter)
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

    }
}
