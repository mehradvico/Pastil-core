namespace Application.Services.Order.DeliverySrv
{
    /// <summary>
    /// روش‌های ارسالی که قیمت و پس‌کرایه‌بودنشان را خودِ فروشگاه تعیین می‌کند (پست، تیپاکس، ماهکس، الوپیک)
    /// و قاعده‌ی جغرافیایی آن‌ها: الوپیک فقط تهران؛ بقیه برای خارج از تهران (و داخل تهران فقط اگر فروشگاه الوپیک فعال ندارد).
    /// </summary>
    public static class StoreShippingRules
    {
        public const string PostLabel = "DeliveryType_Post";
        public const string TipaxLabel = "DeliveryType_Tipax";
        public const string MahexLabel = "DeliveryType_Mahex";
        public const string AloPeykLabel = "DeliveryType_AloPeyk";

        public static bool IsStorePriced(string label) =>
            label == PostLabel || label == TipaxLabel || label == MahexLabel || label == AloPeykLabel;

        // «تهران» با یکسان‌سازی ی/ک عربی و فاصله‌ها
        public static bool IsTehran(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var n = name.Replace('ي', 'ی').Replace('ك', 'ک').Replace("\u200c", "").Replace(" ", "").Trim();
            return n == "تهران";
        }

        public static bool AloPeykApplies(bool destinationIsTehran) => destinationIsTehran;

        public static bool NationalCarrierApplies(bool destinationIsTehran, bool storeHasActiveAloPeyk) =>
            !destinationIsTehran || !storeHasActiveAloPeyk;
    }
}
