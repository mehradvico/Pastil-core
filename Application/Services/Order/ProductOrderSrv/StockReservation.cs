using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.ProductOrderSrv
{
    /// <summary>
    /// رزرو موجودی هنگام ثبت سفارش. سفارش پرداخت‌نشده‌ی فعال (در مهلت <see cref="HoldMinutes"/>) بخشی از موجودی را
    /// «نگه می‌دارد»؛ سفارش جدید فقط از «موجودی − نگه‌داشته‌شده‌ی دیگران» می‌تواند بردارد. کاهش واقعی موجودی
    /// همچنان هنگام موفقیت پرداخت انجام می‌شود (ProductService.IncreaseSellCountAsync)، پس نیازی به ستون یا migration جدید نیست.
    /// </summary>
    public static class StockReservation
    {
        /// <summary>هم‌اندازه‌ی مهلت انقضای پرداخت باز (PaymentService: ۳۰ دقیقه).</summary>
        public const int HoldMinutes = 30;

        /// <summary>
        /// شناسه‌ی آیتم‌هایی که تعداد درخواستی‌شان از موجودی در دسترس بیشتر است؛ لیست خالی یعنی همه‌چیز موجود است.
        /// آیتمی که در <paramref name="quantities"/> نباشد ناموجود حساب می‌شود.
        /// </summary>
        public static List<long> FindShortages(
            IReadOnlyDictionary<long, int> requested,
            IReadOnlyDictionary<long, int> quantities,
            IReadOnlyDictionary<long, int> heldByOthers)
        {
            return requested
                .Where(pair =>
                {
                    if (!quantities.TryGetValue(pair.Key, out var quantity))
                        return true;
                    heldByOthers.TryGetValue(pair.Key, out var held);
                    return pair.Value > quantity - held;
                })
                .Select(pair => pair.Key)
                .OrderBy(id => id)
                .ToList();
        }
    }
}
