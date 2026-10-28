using Application.Common.Dto.Input;

namespace Application.Services.CompanionSrvs.ExpertiseSrv.Dto
{
    public class ExpertiseInputDto : BaseInputDto
    {
        public long? ProviderRoleId { get; set; }
        // برچسب نقش ارائه‌دهنده (Clinic، Coach، Groomer، Pansion، School)؛ تخصص‌های آن نقش + تخصص‌های عمومی
        public string ProviderRole { get; set; }
    }
}
