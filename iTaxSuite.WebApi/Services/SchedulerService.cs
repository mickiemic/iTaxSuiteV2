using Hangfire;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Services;
using iTaxSuite.WebApi.Schedules;

namespace iTaxSuite.WebApi.Services
{
    public interface ISchedulerService
    {
        void ScheduleJobs(CancellationToken cancellationToken);
    }
    public class SchedulerService : ISchedulerService
    {
        private readonly IMasterDataSvc _masterDataSvc;

        public SchedulerService(IMasterDataSvc masterDataSvc)
        {
            _masterDataSvc = masterDataSvc;
        }

        // https://timdeschryver.dev/blog/prevent-a-hangfire-job-from-running-when-it-is-already-active
        public void ScheduleJobs(CancellationToken cancellationToken)
        {
            string _method_ = "ScheduleJobs";
            UI.Debug($">> starting {_method_}");
            try
            {
                var syncChannels = _masterDataSvc.GetChannelsAsync().GetAwaiter().GetResult();
                if (syncChannels == null || !syncChannels.Any())
                    throw new Exception($"{_method_} no tasks to schedule");

                foreach (var channel in syncChannels.Values)
                {
                    if (channel.ChannelId == GeneralConst.PRODUCT_SYNC)
                    {
                        RecurringJob.RemoveIfExists(ProductsFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<ProductsFetcher>(ProductsFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.PO_INVOICE_SYNC)
                    {
                        RecurringJob.RemoveIfExists(PurchasesFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<PurchasesFetcher>(PurchasesFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.OE_INVOICE_SYNC)
                    {
                        RecurringJob.RemoveIfExists(OESalesFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<OESalesFetcher>(OESalesFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }
        }

    }
}
