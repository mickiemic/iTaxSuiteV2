using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public interface ITransactService
    {
        Task<Result<EtimsTransact, string>> ExecTransaction(TransactKey filter);
        Task<Result<PagedResult<EtimsTransact>, string>> GetTransactions(TransactFilter filter);
        Task<Result<EtimsTransact, string>> RetryTransaction(EtimsTransact transact);
    }
    public class TransactService : ITransactService
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IDatabase _baseDb;

        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IS300ProductSvc _productSvc;
        private readonly IS300SaleService _saleService;

        private readonly Dictionary<string, SyncChannel> _syncChannelMap;
        private readonly ClientBranch _clientBranch = null;

        public TransactService(ETimsDBContext dbContext, IConnectionMultiplexer multiplexer, IMasterDataSvc masterDataSvc, IS300ProductSvc productSvc, IS300SaleService saleService)
        {
            _dbContext = dbContext;
            _baseDb = multiplexer.GetDatabase();
            _masterDataSvc = masterDataSvc;
            _productSvc = productSvc;
            _saleService = saleService;
        }

        public async Task<Result<PagedResult<EtimsTransact>, string>> GetTransactions(TransactFilter filter)
        {
            string _method_ = "GetTransactions";
            PagedResult<EtimsTransact> result = new();
            try
            {
                var query = _dbContext.EtimsTransacts.AsQueryable();
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
                if (!string.IsNullOrWhiteSpace(filter.BranchCode))
                    query = query.Where(x => x.BranchCode.Equals(filter.BranchCode));
                if (!string.IsNullOrWhiteSpace(filter.ParentKey))
                    query = query.Where(x => x.ParentKey.Equals(filter.ParentKey));
                if (filter.ReqType != ETIMSReqType.NONE)
                    query = query.Where(x => x.ReqType == filter.ReqType);

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

        public async Task<Result<EtimsTransact, string>> ExecTransaction(TransactKey filter)
        {
            string _method_ = "ExecTransaction";
            EtimsTransact result = null;
            string _strError = string.Empty;
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL, RecordStatus.DEPENDS };

                var query = _dbContext.EtimsTransacts.AsQueryable();
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
                else
                {
                    query = query.Where(f => !completeStatii.Contains(f.RecordStatus));
                }

                if (!string.IsNullOrWhiteSpace(filter.DocNumber))
                    query = query.Where(x => x.DocNumber.Equals(filter.DocNumber));
                if (!string.IsNullOrWhiteSpace(filter.BranchCode))
                    query = query.Where(x => x.BranchCode.Equals(filter.BranchCode));
                if (!string.IsNullOrWhiteSpace(filter.ParentKey))
                    query = query.Where(x => x.ParentKey.Equals(filter.ParentKey));
                if (filter.ReqType != ETIMSReqType.NONE)
                    query = query.Where(x => x.ReqType == filter.ReqType);

                result = await query.OrderBy(x => x.EtimsTrxID).FirstOrDefaultAsync();
                if (result == null)
                {
                    _strError = $"Could not find EtimsTransact matching the criteria given";
                    UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                    return _strError;
                }

                if (result.ReqType == ETIMSReqType.CREATE_ITEMS)
                {
                    var pResult = await _productSvc.ProcessSaveProduct(result);
                    if (pResult.IsError)
                    {
                        _strError = pResult.GetError();
                        UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                        return _strError;
                    }
                    result = pResult.GetValue();
                }
                else if (result.ReqType == ETIMSReqType.SAVE_SALE)
                {
                    var pResult = await _saleService.ProcessSaveSale(result);
                    if (pResult.IsError)
                    {
                        _strError = pResult.GetError();
                        UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                        return _strError;
                    }
                    result = pResult.GetValue();
                }
                else if (result.ReqType == ETIMSReqType.SAVE_STOCKIO)
                {
                    var pResult = await _productSvc.ProcessSaveStockIO(result);
                    if (pResult.IsError)
                    {
                        _strError = pResult.GetError();
                        UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                        return _strError;
                    }
                    result = pResult.GetValue();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<EtimsTransact, string>> RetryTransaction(EtimsTransact transact)
        {
            string _method_ = "RetryTransaction";
            EtimsTransact result = null;
            string _strError = string.Empty;
            try
            {
                var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL, RecordStatus.POST_FAIL, RecordStatus.DEPENDS };
                result = await _dbContext.EtimsTransacts.Where(x => !completeStatii.Contains(x.RecordStatus))
                    .OrderBy(x => x.EtimsTrxID).FirstOrDefaultAsync(x => x.EtimsTrxID == transact.EtimsTrxID);
                if (result == null)
                {
                    return result;
                }

                //TODO: stagger processing using time and try count

                if (result.ReqType == ETIMSReqType.CREATE_ITEMS)
                {
                    var pResult = await _productSvc.ProcessSaveProduct(result);
                    if (pResult.IsError)
                    {
                        _strError = pResult.GetError();
                        UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                        return _strError;
                    }
                    result = pResult.GetValue();
                }
                else if (result.ReqType == ETIMSReqType.SAVE_SALE)
                {
                    var pResult = await _saleService.ProcessSaveSale(result);
                    if (pResult.IsError)
                    {
                        _strError = pResult.GetError();
                        UI.Error($"{_method_} for {result.DocNumber} failed, error: {_strError}");
                        return _strError;
                    }
                    result = pResult.GetValue();
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
            return transact;
        }

    }
}
