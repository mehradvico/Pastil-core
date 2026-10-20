using Application.Common.Dto.Field;
using System.Collections.Generic;

namespace Application.Services.ProviderSrvs.ProviderVenueSrv.Dto
{
    /// <summary>ورودی «اتصال یک نمایندگی به ارائه‌دهنده» توسط ادمین؛ Id = شناسه‌ی فروشگاه یا نماینده (کلینیک/آرایشگاه/پانسیون‌دار...).</summary>
    public class ProviderVenueAssignDto : Id_FieldDto
    {
        public long ProviderId { get; set; }
    }

    public class ProviderVenueVDto : Id_FieldDto
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
    }

    public class ProviderVenuesVDto
    {
        public List<ProviderVenueVDto> Companions { get; set; } = new List<ProviderVenueVDto>();
        public List<ProviderVenueVDto> Groomers { get; set; } = new List<ProviderVenueVDto>();
        public List<ProviderVenueVDto> Clinics { get; set; } = new List<ProviderVenueVDto>();
        public List<ProviderVenueVDto> Stores { get; set; } = new List<ProviderVenueVDto>();
    }
}
