using Microsoft.Extensions.Logging;
using System;

namespace Application.Common.Helpers
{
    /// <summary>
    /// تبدیل Exception به پیام امن برای کلاینت. قبلاً در بیش از صد جا مستقیم ex.Message
    /// (متن خطای EF/SQL/کد داخلی) داخل BaseResultDto به کاربر برمی‌گشت؛ حالا جزئیات فقط
    /// در لاگ سرور می‌ماند و کاربر پیام عمومی می‌بیند. Initialize یک‌بار در Program.cs هر سرویس صدا زده می‌شود.
    /// </summary>
    public static class ExceptionResultHelper
    {
        private static ILoggerFactory _loggerFactory;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static string ToClientMessage(this Exception exception)
        {
            try
            {
                _loggerFactory?.CreateLogger("ServiceException")
                    .LogError(exception, "Service exception converted to a generic client message");
            }
            catch
            {
                // لاگ‌کردن هرگز نباید خودش خطا بسازد.
            }
            return Resource.Notification.SomethingWentWrong;
        }
    }
}
