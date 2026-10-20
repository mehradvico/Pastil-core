using Application.Common.Dto.Field;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto;
using System;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto
{
    public class ProviderSubscriptionVDto : Id_FieldDto
    {
        public long ProviderId { get; set; }
        public long PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }

        public SubscriptionPlanVDto Plan { get; set; }
    }
}
