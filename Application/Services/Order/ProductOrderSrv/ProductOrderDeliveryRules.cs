using Application.Common.Enumerable;

namespace Application.Services.Order.ProductOrderSrv
{
    // قواعد خالص «تحویل‌گیری توسط کاربر». طراحی: backend/Docs/PRODUCT_ORDER_USER_DELIVERY_FA.md
    public static class ProductOrderDeliveryRules
    {
        public enum Decision
        {
            Allowed,
            AlreadyConfirmed,
            NotAllowed
        }

        private static readonly string SendLabel = ProductOrderStatusEnum.ProductOrderStatus_Send.ToString();
        private static readonly string DeliveredLabel = ProductOrderStatusEnum.ProductOrderStatus_Delivered.ToString();
        private static readonly string CanceledLabel = ProductOrderStateEnum.ProductOrderState_Canceled.ToString();

        // کاربر فقط وقتی می‌تواند پاسخ بدهد که سفارش پرداخت‌شده، لغو‌نشده و در وضعیت «ارسال‌شده» باشد و هنوز «تحویل گرفتم» نزده باشد.
        // «تحویل نگرفتم» را تا وقتی نهایی نشده می‌شود عوض کرد.
        public static Decision Decide(bool isPaid, string statusLabel, string stateLabel, bool? userReceived)
        {
            if (userReceived == true)
                return Decision.AlreadyConfirmed;
            if (!isPaid || statusLabel != SendLabel || stateLabel == CanceledLabel)
                return Decision.NotAllowed;
            return Decision.Allowed;
        }

        // ادمین/فروشگاه حق ندارند سفارش را «تحویل داده شد» کنند (فقط وقتی از قبل در همین وضعیت است، تغییرِ بی‌اثر مجاز است)
        public static bool StaffMayChangeStatus(long currentStatusId, long newStatusId, long deliveredStatusId, bool? userReceived)
        {
            if (newStatusId == deliveredStatusId && currentStatusId != deliveredStatusId)
                return false;
            if (userReceived == true && newStatusId != currentStatusId)
                return false;
            return true;
        }

        public static string DeliveredStatusLabel => DeliveredLabel;

        // ---- تأیید خودکار: اگر کاربر ۷ روز بعد از «ارسال شده» هیچ پاسخی ندهد، سفارش خودکار «تحویل گرفته» می‌شود.
        // اگر «تحویل نگرفتم» زده باشد هرگز خودکار تأیید نمی‌شود (اختلاف است و پیگیری دستی می‌خواهد).
        public const int AutoConfirmAfterDays = 7;
        // هشدار به کاربر این‌قدر روز بعد از ارسال (۲ روز مانده به تأیید خودکار)
        public const int WarnAfterDays = 5;

        public static System.DateTime AutoConfirmDate(System.DateTime sentDate) => sentDate.AddDays(AutoConfirmAfterDays);

        public static bool AutoConfirmDue(System.DateTime? sentDate, bool? userReceived, System.DateTime now) =>
            userReceived == null && sentDate.HasValue && now >= AutoConfirmDate(sentDate.Value);

        // بازه‌ی هشدار: از روز پنجم تا قبل از تأیید خودکار، فقط برای کسی که هنوز پاسخ نداده
        public static bool WarnDue(System.DateTime? sentDate, bool? userReceived, System.DateTime now) =>
            userReceived == null && sentDate.HasValue &&
            now >= sentDate.Value.AddDays(WarnAfterDays) && now < AutoConfirmDate(sentDate.Value);

        // کلیدهای یکتای پوش (token2) برای جلوگیری از ارسال تکراری
        public static string WarnKey(string orderId) => "orderwarn:" + orderId;
        public static string AutoKey(string orderId) => "orderauto:" + orderId;
        // پوش «تحویل نگرفتم» به فروشگاه: یک‌بار برای هر سفارش (تغییر توضیح کاربر پوش تازه نمی‌سازد)
        public static string NotReceivedKey(string orderId) => "ordernotrecv:" + orderId;
    }
}
