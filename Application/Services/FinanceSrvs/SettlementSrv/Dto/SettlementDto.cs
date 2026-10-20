using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    public class SettlementDto : Id_FieldDto
    {
        /// <summary>ارائه‌دهنده‌ی دریافت‌کننده؛ در ثبت تسویه‌ی جدید الزامی است.</summary>
        public long? ProviderId { get; set; }
        /// <summary>استخر تسویه: Companion=1 (نمایندگی‌ها)، Store=2 (فروشگاه‌ها)، Groomer=3 (آرایشگاه‌ها)، Clinic=4 (کلینیک‌ها).</summary>
        public Entities.Entities.SettlementKindEnum? Kind { get; set; }
        /// <summary>منسوخ: ثبت تسویه با StoreId/CompanionId پذیرفته نمی‌شود؛ فقط برای خواندن ردیف‌های قدیمی.</summary>
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
        public long UserBankCardId { get; set; }
        public string TrackingCode { get; set; }
    }
}
