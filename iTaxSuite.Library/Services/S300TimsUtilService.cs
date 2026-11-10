using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Newtonsoft.Json;

namespace iTaxSuite.Library.Services
{
    public interface IS300TimsUtilService
    {
        TaxDeviceType GetDeviceType();
        Task<Result<int, string>> FetchNotifications(DateTime lastStamp);
        Task<Result<List<string>, string>> GetNotifByUser(string userId);
        Task<Result<int, string>> GetNotifCount(string userId);
        Task<Result<Notification, string>> ReadNotifDetail(string userId, string notificationId);
    }

    public class S300TimsUtilService : IS300TimsUtilService
    {
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly ClientBranch _clientBranch = null;
        private readonly IEtimsService _etimsService;
        private readonly ETimsDBContext _dbContext;

        public S300TimsUtilService(IMasterDataSvc masterDataSvc, IEtimsService etimsService, ETimsDBContext dbContext)
        {
            _masterDataSvc = masterDataSvc;

            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
            _etimsService = etimsService;
            _dbContext = dbContext;
        }

        public TaxDeviceType GetDeviceType()
        {
            return TaxDeviceType.VSCU;
        }
    
        public async Task<Result<int,string>> FetchNotifications(DateTime lastStamp)
        {
            string _method_ = "GetNotifications";
            try
            {
                string strStamp = lastStamp.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectNotices(strStamp);
                if (result.IsError)
                {
                    return result.GetError();
                }
                var noticeResp = result.GetValue();

                var userIds = new List<string>();
                if (noticeResp.NoticeData.NoticeList.Count > 0)
                {
                    userIds = _dbContext.Users.Select(x => x.Id).ToList();
                }
                var notifList = new List<Notification>();
                foreach(var notice in noticeResp.NoticeData.NoticeList)
                {
                    var notification = new Notification(notice);
                    foreach (var userId in userIds)
                    {
                        var receiver = new NotificationReceiver(notification, userId, _clientBranch.BranchCode);
                        var strJson = JsonConvert.SerializeObject(receiver);
                        notification.NotificationReceivers.Add(receiver);

                    }
                    notifList.Add(notification);
                }

                return notifList.Count;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<int,string>> GetNotifCount(string userId)
        {
            string _method_ = "GetNotifCount";
            try
            {
                return 0;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<List<string>, string>> GetNotifByUser(string userId)
        {
            string _method_ = "GetNotifByUser";
            try
            {
                return new List<string>()
                {
                    "Notification 01", "Notification 02", "Notification 03"
                };
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<Notification, string>> ReadNotifDetail(string userId, string notificationId)
        {
            string _method_ = "ReadNotifDetail";
            try
            {
                return new Notification();
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

    }

}
