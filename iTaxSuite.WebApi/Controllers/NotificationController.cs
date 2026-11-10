using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Sage.CA.SBS.ERP.Sage300.Common.Models.Landlord;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Notifications")]
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : MBaseController
    {
        private readonly IS300TimsUtilService _s300TimsUtilService;

        public NotificationController(IS300TimsUtilService s300TimsUtilService)
        {
            _s300TimsUtilService = s300TimsUtilService;
        }

        [HttpPost("fetchall")]
        public async Task<IActionResult> FetchNotifications(DateTime? lastStamp)
        {
            string _method_ = "GetNotifications";
            try
            {
                var result = await _s300TimsUtilService.FetchNotifications(lastStamp ?? DateTime.Now);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost("getunreadcount")]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            string _method_ = "GetNotifications";
            try
            {
                var result = await _s300TimsUtilService.GetNotifCount(userId);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost("getunread")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            string _method_ = "GetByUser";
            try
            {
                var result = await _s300TimsUtilService.GetNotifByUser(userId);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPut("readcontent")]
        public async Task<IActionResult> ReadDetail(string userId, string notificationId)
        {
            string _method_ = "ReadDetail";
            try
            {
                var result = await _s300TimsUtilService.ReadNotifDetail(userId, notificationId);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }


        //[HttpGet]
        //public async Task<ActionResult<PagedResult<NotificationDto>>> Get(
        //[FromQuery] bool? read,
        //[FromQuery] string? type,
        //[FromQuery] DateTime? from,
        //[FromQuery] int page = 1,
        //[FromQuery] int pageSize = 20)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var query = _db.Notifications
        //        .Where(n => n.TargetType == TargetType.All
        //                 || (n.TargetType == TargetType.Role && _userRoles.Contains(n.TargetRoleId))
        //                 || (n.TargetType == TargetType.SpecificUser && n.TargetUserId == userId));

        //    if (read.HasValue)
        //        query = query.Where(n =>
        //            read.Value
        //                ? _db.NotificationRecipients.Any(r => r.NotificationId == n.Id && r.UserId == userId && r.IsRead)
        //                : !_db.NotificationRecipients.Any(r => r.NotificationId == n.Id && r.UserId == userId && r.IsRead));

        //    // ... pagination, map to DTO
        //}

        //[HttpGet("unread-count")]
        //public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var count = await _db.Notifications
        //        .Where(n => /* visibility predicate same as above */)
        //        .Where(n => !_db.NotificationRecipients
        //            .Any(r => r.NotificationId == n.Id && r.UserId == userId && r.IsRead))
        //        .CountAsync();
        //    return Ok(new UnreadCountDto { Count = count });
        //}

        //[HttpPatch("{id:int}")]
        //public async Task<IActionResult> Patch(int id, [FromBody] PatchNotificationRequest req)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Lazy upsert into NotificationRecipients
        //    var existing = await _db.NotificationRecipients
        //        .FirstOrDefaultAsync(r => r.NotificationId == id && r.UserId == userId);

        //    if (existing == null)
        //    {
        //        _db.NotificationRecipients.Add(new NotificationRecipient
        //        {
        //            NotificationId = id,
        //            UserId = userId,
        //            IsRead = req.IsRead,
        //            ReadAtUtc = req.IsRead ? DateTime.UtcNow : null
        //        });
        //    }
        //    else
        //    {
        //        existing.IsRead = req.IsRead;
        //        existing.ReadAtUtc = req.IsRead ? DateTime.UtcNow : null;
        //    }

        //    await _db.SaveChangesAsync();
        //    return NoContent(); // 204
        //}

        //[HttpPatch("read-all")]
        //public async Task<IActionResult> MarkAllRead()
        //{
        //    // Bulk update — can be 202 if large
        //}
    }
}
