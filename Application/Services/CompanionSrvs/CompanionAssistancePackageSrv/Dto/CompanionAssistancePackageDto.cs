using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto
{
    public class CompanionAssistancePackageDto : Name_FieldDto
    {
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public long CompanionAssistanceId { get; set; }
        public string Discription { get; set; }
        public string PetSize { get; set; }

        public CompanionAssistanceVDto CompanionAssistance { get; set; }
    }
}
