using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Dto
{
    public class AssistanceExpertiseDto
    {
        public long AssistanceId { get; set; }
        // مجموعه‌ی کامل تخصص‌های مرتبط با خدمت؛ خالی = هیچ تخصصی مرتبط نیست
        public List<long> ExpertiseIds { get; set; } = new();
    }

    public class AssistanceExpertiseItemVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class AssistanceExpertiseVDto
    {
        public long AssistanceId { get; set; }
        public List<long> ExpertiseIds { get; set; } = new();
        public List<AssistanceExpertiseItemVDto> Expertises { get; set; } = new();
    }
}
