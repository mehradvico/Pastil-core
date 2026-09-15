using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// عیب‌یابی پوش نوتیفیکیشن
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PushDiagnosticsController : ControllerBase
    {
        private readonly IDataBaseContext _context;
        private readonly IFcmSender _fcmSender;
        private readonly IOptions<VapidKeysOption> _vapid;

        public PushDiagnosticsController(
            IDataBaseContext context,
            IFcmSender fcmSender,
            IOptions<VapidKeysOption> vapid)
        {
            _context = context;
            _fcmSender = fcmSender;
            _vapid = vapid;
        }

        /// <summary>
        /// وضعیت آمادگی سرور برای ارسال پوش + آمار دستگاه‌های ثبت‌شده.
        /// برای عیب‌یابی سریع «چرا پوش نمی‌رسد» بدون نیاز به دسترسی به دیتابیس.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] long? userId)
        {
            var subs = _context.PushSubscriptions.AsNoTracking().Where(x => x.IsActive);

            var webPushCount = await subs.CountAsync(x => x.Provider != (long)PushProviderEnum.Fcm);
            var fcmCount = await subs.CountAsync(x => x.Provider == (long)PushProviderEnum.Fcm);
            var unlinkedCount = await subs.CountAsync(x => x.UserId == null);

            object? forUser = null;
            if (userId.HasValue && userId.Value > 0)
            {
                var userSubs = subs.Where(x => x.UserId == userId.Value);
                forUser = new
                {
                    userId = userId.Value,
                    total = await userSubs.CountAsync(),
                    webPush = await userSubs.CountAsync(x => x.Provider != (long)PushProviderEnum.Fcm),
                    fcm = await userSubs.CountAsync(x => x.Provider == (long)PushProviderEnum.Fcm),
                    platforms = await userSubs
                        .GroupBy(x => x.Platform)
                        .Select(g => new { platform = g.Key ?? "web", count = g.Count() })
                        .ToListAsync()
                };
            }

            return Ok(new
            {
                // اگر false باشد، هیچ پوشی به اپ‌های فلاتر نمی‌رسد: متغیر محیطی
                // PASTIL_FCM_SERVICE_ACCOUNT_JSON روی سرور ست نشده یا محتوایش نامعتبر است.
                fcmConfigured = _fcmSender.IsConfigured,
                webPushConfigured = !string.IsNullOrWhiteSpace(_vapid.Value?.PublicKey) &&
                                    !string.IsNullOrWhiteSpace(_vapid.Value?.PrivateKey),
                activeSubscriptions = new
                {
                    webPush = webPushCount,
                    fcm = fcmCount,
                    // ردیف‌هایی که به هیچ کاربری وصل نیستند؛ پوشِ «کاربر خاص» به این‌ها نمی‌رسد.
                    notLinkedToAnyUser = unlinkedCount
                },
                forUser
            });
        }
    }
}
