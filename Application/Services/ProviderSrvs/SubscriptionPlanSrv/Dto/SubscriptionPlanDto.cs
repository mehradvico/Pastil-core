using Application.Common.Dto.Field;

namespace Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto
{
    public class SubscriptionPlanDto : Name_FieldDto
    {
        public int Level { get; set; }
        public bool Active { get; set; }
    }
}
