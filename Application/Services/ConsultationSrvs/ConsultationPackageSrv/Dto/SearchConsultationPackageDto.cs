using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto
{
    public class SearchConsultationPackageDto : Name_FieldDto
    {
        public long? CompanionId { get; set; }
        public long? ClinicId { get; set; }
        public long? CoachId { get; set; }
        // نام مرکز مالک (نمایندگی / کلینیک / مربی)
        public string CompanionName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
