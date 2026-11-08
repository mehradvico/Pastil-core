using System.Collections.Generic;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto
{
    public class FinancePansionInputDto
    {
        /// <summary>خالی = همه‌ی پانسیون‌های کاربر (فقط ناحیه‌ی ارائه‌دهنده)</summary>
        public long? PansionId { get; set; }
        public bool? Permitted { get; set; }
    }

    public class FinancePansionInfoVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool? IsDaycare { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public long? ProviderId { get; set; }
        public long? CompanionId { get; set; }
        public decimal DailyCommissionPercent { get; set; }
        public decimal HourlyCommissionPercent { get; set; }
        public bool HasCommission { get; set; }
    }

    /// <summary>هم‌شکل FinanceCompanionVDto (CompanionId اینجا ۰ است) به‌علاوه‌ی فهرست پانسیون‌ها.</summary>
    public class FinancePansionVDto : FinanceCompanionVDto
    {
        public List<FinancePansionInfoVDto> Pansions { get; set; } = new List<FinancePansionInfoVDto>();
    }
}
