using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingBoostLevel
    {
        public int Level { get; set; }
        /// <summary>افزایش هزینه‌ی سفر در این سطح (تومان)</summary>
        public double Amount { get; set; }
    }

    // ارائه‌دهنده‌ای که اولویت‌دهی (Boost) پشتیبانی می‌کند؛ فعلاً فقط میاره (GET /boost-levels/).
    public interface IBoostCapableShippingProvider
    {
        /// <summary>سطوح فعلی boost؛ در خطا/غیرفعال بودن لیست خالی (هرگز Exception نمی‌دهد). نتیجه چند دقیقه کش می‌شود.</summary>
        Task<IReadOnlyList<ShippingBoostLevel>> GetBoostLevelsAsync(CancellationToken cancellationToken = default);
    }
}
