using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto
{
    public class AssistanceGroupVDto : Name_FieldDto
    {
        public int Priority { get; set; }
        public bool Active { get; set; }
    }
}
