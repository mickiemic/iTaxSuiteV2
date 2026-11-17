using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace iTaxSuite.Library.Services
{
    public interface IS300TimsUtilService
    {
        TaxDeviceType GetDeviceType();
        Task<Result<int, string>> FetchNotifications(DateTime lastStamp);
        Task<Result<List<string>, string>> GetNotifByUser(string userId);
        Task<Result<int, string>> GetNotifCount(string userId);
        Task<Result<Notification, string>> ReadNotifDetail(string userId, string notificationId);
        Task<Result<int, string>> MarkAllAsRead(string userId);
        Task<Result<int, string>> FetchAllTISCodes(DateTime lastStamp);
        Task<Result<int, string>> FetchAllItemClasses(DateTime lastStamp);
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
            string _method_ = "FetchNotifications";
            try
            {
                string strStamp = lastStamp.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectNotices(strStamp);
                if (result.IsError)
                {
                    return result.GetError();
                }
                var noticeResp = result.GetValue();
                if (noticeResp.NoticeData.NoticeList.Count == 0)
                {
                    return 0;
                }

                var userIds = new List<string>();
                if (noticeResp.NoticeData.NoticeList.Count > 0)
                {
                    userIds = _dbContext.Users.Select(x => x.Id).ToList();
                }
                var notifList = new List<Notification>();
                foreach(var notice in noticeResp.NoticeData.NoticeList)
                {
                    var notification = new Notification(notice);
                    // Consider checking TargetType, TargetRoleId and TargetUserId for allocating Notifications
                    foreach (var userId in userIds)
                    {
                        var receiver = new NotificationReceiver(notification, userId, _clientBranch.BranchCode);
                        notification.NotificationReceivers.Add(receiver);

                    }
                    notifList.Add(notification);
                }

                var notificationIds = notifList.Select(x => x.NotificationID).ToHashSet();
                var existingNotificationIds = await _dbContext.Notifications.Where(n => notificationIds.Contains(n.NotificationID))
                    .Select(n => n.NotificationID).ToHashSetAsync();
                notifList = notifList.Where(n => !existingNotificationIds.Contains(n.NotificationID)).ToList();
                if (notifList.Count == 0)
                {
                    return 0;
                }

                await _dbContext.Notifications.AddRangeAsync(notifList);
                int saved = await _dbContext.SaveChangesAsync();
                return saved;
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
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return "Invalid/empty userId provided";
                }
                int unreadCount = await _dbContext.NotificationReceivers.Include(n => n.Notification)
                    .Where(nr => nr.UserID == userId && (!nr.IsRead || !nr.IsDismissed)
                        && (!nr.Notification.ExpiresAt.HasValue || nr.Notification.ExpiresAt.Value > DateTime.Now))
                    .CountAsync();
                return unreadCount;
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
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return "Invalid/empty userId provided";
                }
                var userNotifications = await _dbContext.NotificationReceivers.Include(n => n.Notification)
                    .Where(nr => nr.UserID == userId && (!nr.IsRead || !nr.IsDismissed)
                        && (!nr.Notification.ExpiresAt.HasValue || nr.Notification.ExpiresAt.Value > DateTime.Now))
                    .Select(nr => nr.Notification.Title)
                    .ToListAsync();
                return userNotifications;
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
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return "Invalid/empty userId provided";
                }
                if (string.IsNullOrWhiteSpace(notificationId))
                {
                    return "Invalid/empty notificationId provided";
                }
                var notification = await _dbContext.Notifications.Include(n => n.NotificationReceivers)
                    .FirstOrDefaultAsync(n => n.NotificationID == notificationId 
                        && n.NotificationReceivers.Any(nr => nr.UserID == userId));
                if (notification != null)
                {
                    var receiver = notification.NotificationReceivers.FirstOrDefault(nr => nr.UserID == userId && nr.NotificationID == notificationId);
                    if (receiver != null)
                    {
                        receiver.IsRead = true;
                        receiver.ReadDate = DateTime.Now;
                        receiver.UpdatedOn = DateTime.Now;
                        receiver.UpdatedBy = GeneralConst.APPLICATION_NAME;
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return notification ?? new Notification();
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int, string>> MarkAllAsRead(string userId)
        {
            string _method_ = "MarkAllAsRead";
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return "Invalid/empty userId provided";
                }
                var notifications = await _dbContext.NotificationReceivers
                    .Where(nr => nr.UserID == userId && (!nr.IsRead || !nr.IsDismissed))
                    .ToListAsync();
                foreach (var receiver in notifications)
                {
                    receiver.IsRead = true;
                    receiver.ReadDate = DateTime.Now;
                    receiver.UpdatedOn = DateTime.Now;
                    receiver.UpdatedBy = GeneralConst.APPLICATION_NAME;
                }
                int updatedCount = await _dbContext.SaveChangesAsync();
                return updatedCount;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int, string>> FetchAllTISCodes(DateTime lastStamp)
        {
            string _method_ = "FetchAllTISCodes";
            try
            {
                string strStamp = lastStamp.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectCodes(strStamp);
                if (result.IsError)
                {
                    return result.GetError();
                }
                var codesResp = result.GetValue();
                if (codesResp.Data.CodesList.Count == 0)
                {
                    return 0;
                }

                var codesList = new List<TISystemCode>();
                foreach(var header in codesResp.Data.CodesList)
                {
                    var systemCode = new TISystemCode(header);
                    codesList.Add(systemCode);
                }

                var codeIds  = codesList.Select(x => x.CodeClass).ToHashSet();
                var existingCodes = await _dbContext.TISystemCodes.Select(c => c.CodeClass).ToHashSetAsync();
                codesList = codesList.Where(c => !existingCodes.Contains(c.CodeClass)).ToList();
                if (codesList.Count == 0)
                {
                    return 0;
                }

                await _dbContext.TISystemCodes.AddRangeAsync(codesList);
                int saved = await _dbContext.SaveChangesAsync();

                return saved;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<int, string>> FetchAllItemClasses(DateTime lastStamp)
        {
            string _method_ = "FetchAllItemClasses";
            try
            {
                string strStamp = lastStamp.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectItemClass(strStamp);
                if (result.IsError)
                {
                    return result.GetError();
                }
                var classResp = result.GetValue();
                if (classResp.Data.ItemClassList.Count == 0)
                {
                    return 0;
                }

                var classList = new List<ItemClassCode>();
                foreach (var item in classResp.Data.ItemClassList)
                {
                    var classItem = new ItemClassCode(item);
                    classList.Add(classItem);
                }

                var classIds = classList.Select(x => x.ClassCode).ToHashSet();
                var existingCodes = await _dbContext.ItemClassCodes.Select(c => c.ClassCode).ToHashSetAsync();
                classList = classList.Where(c => !existingCodes.Contains(c.ClassCode)).ToList();
                if (classList.Count == 0)
                {
                    return 0;
                }

                await _dbContext.ItemClassCodes.AddRangeAsync(classList);
                int saved = await _dbContext.SaveChangesAsync();

                return saved;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return ex.GetBaseException().Message;
            }
        }


    }

}
