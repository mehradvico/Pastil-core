using Entities.Entities.CommonField;
using Entities.Entities.ProviderField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class Settlement : Id_Field
    {
        /// <summary>ارائه‌دهنده‌ی دریافت‌کننده‌ی تسویه؛ تسویه‌های جدید فقط با ProviderId ثبت می‌شوند و StoreId/CompanionId در آن‌ها null است.</summary>
        public long? ProviderId { get; set; }
        /// <summary>کدام استخر تسویه شده: نمایندگی‌ها یا فروشگاه‌ها.</summary>
        public SettlementKindEnum Kind { get; set; }
        /// <summary>فقط در ردیف‌های قدیمی (قبل از تسویه‌ی ارائه‌دهنده‌محور) پر است.</summary>
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserBankCardId { get; set; }
        public string TrackingCode { get; set; }
        public double PaidPrice { get; set; }
        public long ItemCount { get; set; }

        public UserBankCard UserBankCard { get; set; }
        public Companion Companion { get; set; }
        public Store Store { get; set; }
        public Provider Provider { get; set; }
    }
}
