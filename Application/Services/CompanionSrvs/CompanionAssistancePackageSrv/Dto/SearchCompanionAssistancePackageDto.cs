using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto
{
    public class SearchCompanionAssistancePackageDto : Name_FieldDto
    {
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long CompanionAssistanceId { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public long AssistanceId { get; set; }
        public string AssistanceName { get; set; }
        public string Description { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
