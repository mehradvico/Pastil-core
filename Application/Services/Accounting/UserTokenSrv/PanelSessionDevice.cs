using Application.Common.Helpers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.Accounting.UserTokenSrv
{
    /// <summary>
    /// نشست‌های پنل ادمین به «دستگاه» گره می‌خورند. کلاینت پنل یک شناسه‌ی تصادفیِ دستگاه (deviceId) در
    /// localStorage نگه می‌دارد و هنگام ورود و هر refresh می‌فرستد؛ سرور فقط هشِ آن را داخل ستون موجود
    /// UserToken.DeviceName ذخیره می‌کند (بدون تغییر schema): <c>panel:&lt;sha256&gt;</c>. نشستِ بدون deviceId
    /// (نسخه‌ی قدیمی پنل) فقط برچسب <c>panel:</c> می‌گیرد و گره نمی‌خورد. توکن‌های وب‌اپ/اپ موبایل
    /// (DeviceName قدیمی «zand») اصلاً تحت تأثیر نیستند.
    /// </summary>
    public static class PanelSessionDevice
    {
        public const string Prefix = "panel:";
        private const int MaxDeviceIdLength = 200;

        public static bool IsPanelSession(string deviceName) =>
            deviceName != null && deviceName.StartsWith(Prefix, StringComparison.Ordinal);

        public static string BuildName(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId) || deviceId.Length > MaxDeviceIdLength)
                return Prefix;
            return Prefix + deviceId.Trim().Tosha256Hash();
        }

        /// <summary>true = مجاز (نشست پنل نیست، یا گره‌ای ندارد، یا همان دستگاه است).</summary>
        public static bool Matches(string storedDeviceName, string presentedDeviceId)
        {
            if (!IsPanelSession(storedDeviceName) || storedDeviceName.Length == Prefix.Length)
                return true;
            if (string.IsNullOrWhiteSpace(presentedDeviceId) || presentedDeviceId.Length > MaxDeviceIdLength)
                return false;

            var expected = Encoding.UTF8.GetBytes(storedDeviceName);
            var actual = Encoding.UTF8.GetBytes(BuildName(presentedDeviceId));
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
