using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv.Iface
{
    /// <summary>
    /// اشتراک Provider تنها منبع حقیقت است؛ این سرویس آن را روی فیلدهای قدیمی Gold/SilverAccountDate همه‌ی Companionهای Provider می‌نویسد
    /// تا جستجو، مرتب‌سازی، badge و اپ‌ها تا زمان مهاجرت کامل بدون تغییر کار کنند.
    /// </summary>
    public interface IProviderSubscriptionSyncService
    {
        /// <summary>اشتراک جاری Provider را روی Companionهایش می‌نویسد و ذخیره می‌کند.</summary>
        Task SyncProviderAsync(long providerId);
    }
}
