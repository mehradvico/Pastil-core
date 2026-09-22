using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.Dto;

namespace Application.Services.CompanionSrvs.CompanionUserSrv.Dto
{
    public class CompanionUserVDto : Id_FieldDto
    {
        public bool? UserAccept { get; set; }

        public long CompanionId { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public long? ExpertiseId { get; set; }
        public ExpertiseVDto Expertise { get; set; }
        public System.Collections.Generic.List<long> ExpertiseIds { get; set; } = new();
        public System.Collections.Generic.List<ExpertiseVDto> Expertises { get; set; } = new();
        public CompanionVDto Companion { get; set; }
        public UserMinVDto User { get; set; }
    }
}
