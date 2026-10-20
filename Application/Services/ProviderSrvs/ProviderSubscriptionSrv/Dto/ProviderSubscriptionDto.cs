using Application.Common.Dto.Field;
using System;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto
{
    /// <summary>ورودی «اختصاص اشتراک» توسط ادمین.</summary>
    public class ProviderSubscriptionDto : Id_FieldDto
    {
        public long ProviderId { get; set; }
        public long PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; }
    }
}
