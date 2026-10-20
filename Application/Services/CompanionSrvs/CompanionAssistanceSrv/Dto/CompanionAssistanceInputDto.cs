using Application.Common.Dto.Input;
using Application.Common.Enumerable.Code;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;

namespace Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto
{
    public class CompanionAssistanceInputDto : BaseInputDto, ICompanionAssistanceSearchFields
    {
        public long? CompanionId { get; set; }
        public long? AssistanceId { get; set; }
        public bool? IsSinglePackage { get; set; }
        public bool? IsPersonal { get; set; }
        public long? CompanionTypeId { get; set; }
        public long? PetId { get; set; }
        public CompanionAssistanceTypeEnum? Type { get; set; }
        /// <summary>فقط ادمین (سمت سرور) ردیف‌های خدمت گرومینگ قدیمی را می‌بیند.</summary>
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public bool IncludeGrooming { get; set; }
    }
}
