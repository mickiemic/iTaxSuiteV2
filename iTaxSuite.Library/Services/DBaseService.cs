using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace iTaxSuite.Library.Services
{
    public interface IDBaseService
    {
        Task<bool> InsertRequestLogAsync(ApiRequestLog requestLog);
        Task<bool> UpdateRequestLogAsync(ApiRequestLog requestLog);
    }

    public class DBaseService : IDBaseService
    {
        private readonly ETimsDBContext _dbContext;

        public DBaseService(ETimsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> InsertRequestLogAsync(ApiRequestLog requestLog)
        {
            string _method_ = "InsertRequestLogAsync";
            try
            {
                requestLog.ReqPayload = requestLog.ReqPayload.WithMaxLength(4000, true);
                await _dbContext.ApiRequestLog.AddAsync(requestLog);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} : {ex.GetBaseException().Message}");
                return false;
            }
        }

        public async Task<bool> UpdateRequestLogAsync(ApiRequestLog requestLog)
        {
            string _method_ = "UpdateRequestLogAsync";

            int affectedRows = 0;
            try
            {
                affectedRows = await _dbContext.ApiRequestLog.Where(x => x.RequestID == requestLog.RequestID)
                    .ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.StatusCode, requestLog.StatusCode)
                    .SetProperty(x => x.RespHeaders, requestLog.RespHeaders.WithMaxLength(512, true))
                    .SetProperty(x => x.RespPayload, requestLog.RespPayload.WithMaxLength(4000, true))
                    .SetProperty(x => x.ResponseAt, requestLog.ResponseAt)
                    .SetProperty(x => x.Duration, double.Round(requestLog.Duration, 4))
                    );
                UI.Debug($"{_method_} RequestID:{requestLog.RequestID} Affected Rows: {affectedRows}");
                return (affectedRows == 1);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} : {ex.GetBaseException().Message}");
                return false;
            }
        }
    }
}
