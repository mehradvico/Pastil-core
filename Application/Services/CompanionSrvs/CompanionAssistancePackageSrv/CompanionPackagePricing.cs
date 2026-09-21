using Entities.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv
{
    /// <summary>
    /// قیمت یک پکیج برای یک «نحوه ارائه». پکیج جدید برای هر حالتی که ارائه می‌دهد ردیف قیمت جدا دارد؛
    /// پکیج قدیمی (بدون ردیف) همه‌ی حالت‌های خدمت را با Price/PrePaymentPrice خودش ارائه می‌دهد.
    /// </summary>
    public static class CompanionPackagePricing
    {
        public readonly struct Quote
        {
            public Quote(bool offered, double price, double prePaymentPrice, bool fromModeRow)
            {
                Offered = offered;
                Price = price;
                PrePaymentPrice = prePaymentPrice;
                FromModeRow = fromModeRow;
            }

            /// <summary>آیا پکیج در این حالت ارائه می‌شود؟</summary>
            public bool Offered { get; }
            public double Price { get; }
            public double PrePaymentPrice { get; }
            /// <summary>true = قیمت از ردیف حالت آمده (در آنلاین یعنی قیمت روش آنلاین جداگانه اضافه نمی‌شود).</summary>
            public bool FromModeRow { get; }
        }

        public static Quote Resolve(IEnumerable<CompanionAssistancePackageType> packageTypes, double packagePrice, double packagePrePaymentPrice, long assistanceTypeId)
        {
            var rows = (packageTypes ?? Enumerable.Empty<CompanionAssistancePackageType>()).Where(t => !t.Deleted).ToList();
            if (rows.Count == 0)
                return new Quote(true, packagePrice, packagePrePaymentPrice, false);

            var row = rows.FirstOrDefault(t => t.CompanionAssistanceTypeId == assistanceTypeId);
            return row == null
                ? new Quote(false, 0, 0, true)
                : new Quote(true, row.Price, row.PrePaymentPrice, true);
        }
    }
}
