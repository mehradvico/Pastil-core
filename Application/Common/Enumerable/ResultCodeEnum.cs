namespace Application.Common.Enumerable
{
    public enum ResultCodeEnum : int
    {
        None = 0,

        SignUp = 1,
        OneFactor = 2,
        TwoFactor = 3,
        Locked = 4,

        VerifyCodeCountError = 5,

        // پرداخت این هدف (رزرو/سفارش/سفر/...) قبلاً موفق شده؛ فرانت به‌جای خطا، کاربر را به جزئیات همان مورد می‌برد
        PaymentAlreadyPaid = 20,

        // تأیید پرداخت با بانک جواب قطعی نداد (تایم‌اوت/خطای شبکه)؛ پرداخت «ناموفق» ثبت نشده و job تطبیق نتیجه را قطعی می‌کند
        PaymentVerificationPending = 21,

    }
}
