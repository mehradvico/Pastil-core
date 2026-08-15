using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.Dto;
using System.Runtime.Serialization;

namespace Application.Services.CompanionSrvs.CompanionUserSrv.Dto
{
    public class CompanionUserDto : Id_FieldDto
    {
        public bool? UserAccept { get; set; }

        public long CompanionId { get; set; }
        public string Phone { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public long? ExpertiseId { get; set; }
        public ExpertiseVDto Expertise { get; set; }
        public CompanionVDto Companion { get; set; }
        public UserMinVDto User { get; set; }


    }
}
