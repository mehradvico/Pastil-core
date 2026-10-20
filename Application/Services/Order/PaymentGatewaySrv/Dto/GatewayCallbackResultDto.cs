namespace Application.Services.Order.PaymentGatewaySrv.Dto
{
    public class GatewayCallbackResultDto
    {
        public bool IsSuccess { get; set; }
        // جواب قطعی بانک نرسید (تایم‌اوت، خطای شبکه، پاسخ نامعتبر): نه موفق است نه ناموفق و هیچ‌چیز روی پرداخت ثبت نمی‌شود
        public bool IsPending { get; set; }
        public string ErrorMessage { get; set; }
        public string RefNumber { get; set; }
        public string TraceNumber { get; set; }
        public string Description { get; set; }
        public string Token { get; set; }
    }
}
