namespace Application.Services.Accounting.UserTokenSrv.Dto
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool IsAdmin { get; set; }
        /// <summary>شناسه‌ی تصادفی دستگاه پنل (فقط نشست‌های پنل؛ اختیاری برای سازگاری با کلاینت‌های قدیمی).</summary>
        public string DeviceId { get; set; }
    }
}
