using Api.Hubs;
using Application.Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// تماس درون‌برنامه‌ای در انتظار پاسخ کاربر جاری - برای وقتی که پوش لحظه‌ای زنگ به کاربر نرسیده
    /// (اپ بسته بوده یا پوش تحویل داده نشده) اما با باز کردن اپ باید همچنان بتواند وارد تماس شود.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CallPendingController : ControllerBase
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly CallSessionTracker _tracker;

        public CallPendingController(ICurrentUserHelper currentUserHelper, CallSessionTracker tracker)
        {
            _currentUserHelper = currentUserHelper;
            _tracker = tracker;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var currentUser = _currentUserHelper.CurrentUser;
            if (currentUser == null)
            {
                return Ok(new { reserveId = (long?)null, sessionId = (long?)null, callerName = (string)null, isVideo = false });
            }

            var pending = _tracker.FindPendingCallForBooker(currentUser.UserId);
            // تماس جلسه‌ی آنلاین (بدون رزرو) در ردیاب با کلید منفی (-sessionId) نگه داشته می‌شود
            var isSession = pending.HasValue && pending.Value.ReserveId < 0;
            return Ok(new
            {
                reserveId = pending.HasValue && !isSession ? pending.Value.ReserveId : (long?)null,
                sessionId = isSession ? -pending.Value.ReserveId : (long?)null,
                callerName = pending?.CallerName,
                isVideo = pending?.IsVideo ?? false
            });
        }
    }
}
