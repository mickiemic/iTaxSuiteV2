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
                    if (channel.ChannelId == GeneralConst.IC_PRODUCT_SYNC)
                    {
                        RecurringJob.RemoveIfExists(ICProductsFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<ICProductsFetcher>(ICProductsFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.AR_PRODUCT_SYNC)
                    {
                        RecurringJob.RemoveIfExists(ARProductsFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<ARProductsFetcher>(ARProductsFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.GL_PRODUCT_SYNC)
                    {
                        RecurringJob.RemoveIfExists(GLProductsFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<GLProductsFetcher>(GLProductsFetcher.JobId,
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
                        RecurringJob.RemoveIfExists(OEInvoiceFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<OEInvoiceFetcher>(OEInvoiceFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.OE_CRDRNOTE_SYNC)
                    {
                        RecurringJob.RemoveIfExists(OECRNoteFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<OECRNoteFetcher>(OECRNoteFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.AR_INVOICE_SYNC)
                    {
                        RecurringJob.RemoveIfExists(ARInvoiceFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<ARInvoiceFetcher>(ARInvoiceFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.AR_CRDRNOTE_SYNC)
                    {
                        RecurringJob.RemoveIfExists(ARCRNoteFetcher.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<ARCRNoteFetcher>(ARCRNoteFetcher.JobId,
                            x => x.ExecuteAsync(cancellationToken), channel.TriggerExpr);
                    }
                    else if (channel.ChannelId == GeneralConst.ALL_TAXTRXS_POST)
                    {
                        RecurringJob.RemoveIfExists(AllTaxTrxsPost.JobId);
                        if (channel.IsActive)
                            RecurringJob.AddOrUpdate<AllTaxTrxsPost>(AllTaxTrxsPost.JobId,
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
