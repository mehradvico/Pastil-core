using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Dto
{
    public class AssistanceDto : FullName_FieldDto
    {
        public bool IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
    }
}
