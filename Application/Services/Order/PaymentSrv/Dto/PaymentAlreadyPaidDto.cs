namespace Application.Services.Order.PaymentSrv.Dto
{
    // همراه کد ResultCodeEnum.PaymentAlreadyPaid (۲۰): فرانت به‌جای خطا، کاربر را به جزئیات همین مورد می‌برد.
    // CallBackTypeLabel یکی از مقادیر PaymentCallbackTypeEnum است (CompanionReserve، ConsultationPurchase، ...).
    public class PaymentAlreadyPaidDto
    {
        public string CallBackTypeLabel { get; set; }
        public string CallBackId { get; set; }
    }
}
