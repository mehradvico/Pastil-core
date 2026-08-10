using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Dto
{
    public class AssistanceVDto : FullName_FieldDto
    {
        public bool IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }

        public AssistanceGroupVDto AssistanceGroup { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
