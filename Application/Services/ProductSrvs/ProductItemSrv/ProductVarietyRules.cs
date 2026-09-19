using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductItemSrv
{
    /// <summary>
    /// قواعد تنوع محصول (خالص و بدون دیتابیس؛ قابل تست). نوع تنوع و مقدارها را ادمین تعیین می‌کند؛ فروشنده فقط برای
    /// مقدارهای موجودِ همان تنوع قیمت/موجودی می‌دهد.
    /// </summary>
    public static class ProductVarietyRules
    {
        /// <summary>
        /// اعتبار سطرهای ثبت آیتم نسبت به تنوع محصول: محصول بدون تنوع ⇒ مقدار باید null باشد؛ محصول دارای تنوع ⇒ مقدار
        /// باید non-null و متعلق به همان تنوع (غیرحذف‌شده) باشد. یک سطر نامعتبر یعنی کل درخواست رد شود.
        /// </summary>
        public static bool RowsAreValid(
            long? productVarietyId,
            long? productVariety2Id,
            IReadOnlySet<long> validValues1,
            IReadOnlySet<long> validValues2,
            IEnumerable<(long? Item1, long? Item2)> rows)
        {
            foreach (var (item1, item2) in rows)
            {
                if (!SlotIsValid(productVarietyId, validValues1, item1) || !SlotIsValid(productVariety2Id, validValues2, item2))
                    return false;
            }
            return true;
        }

        private static bool SlotIsValid(long? productVarietyId, IReadOnlySet<long> validValues, long? itemValue)
        {
            if (productVarietyId == null)
                return itemValue == null;
            return itemValue.HasValue && validValues.Contains(itemValue.Value);
        }

        /// <summary>
        /// آیا تغییر ساختار تنوع محصول باید رد شود؟ اگر فروشنده‌ای برای این محصول آیتمی با مقدار تنوع دارد، هر تغییر
        /// (حذف/جایگزینی/افزودن تنوع دوم) آیتم‌های او را نامعتبر یا پاک می‌کند؛ پس مجاز نیست. افزودن «مقدار» به مخزن یک
        /// تنوع، به این قاعده ربطی ندارد و همیشه آزاد است.
        /// </summary>
        public static bool ChangeIsBlocked(
            long? oldVarietyId,
            long? oldVariety2Id,
            long? newVarietyId,
            long? newVariety2Id,
            bool hasItemsUsingVarietyValues)
        {
            var changed = oldVarietyId != newVarietyId || oldVariety2Id != newVariety2Id;
            return changed && hasItemsUsingVarietyValues;
        }
    }
}
