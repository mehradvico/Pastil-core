using Application.Common.Enumerable.Message;

namespace Application.Services.Accounting.OtpVerifySrv.Dto
{
    public class OtpVerifyVDto
    {
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        /// <summary>طول کد OTP ارسال‌شده (پاسخ ارسال کد)؛ کلاینت تعداد کادرهای ورود را از همین می‌گیرد.</summary>
        public int CodeLength { get; set; }
        public MessageTypeEnum Type { get; set; } = MessageTypeEnum.Otp;


    }
}
