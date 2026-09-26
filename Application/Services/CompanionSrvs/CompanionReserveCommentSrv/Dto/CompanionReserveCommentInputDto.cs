using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto
{
    public class CompanionReserveCommentInputDto : BaseInputDto, ICompanionReserveCommentSearchFields
    {
        public long? CompanionReserveId { get; set; }
        public long? CompanionId { get; set; }
        public bool? AllStatus { get; set; }
        public long? UserId { get; set; }
        // نمای نماینده: نظرهای ردشده‌ی ادمین نمایش داده نشوند
        public bool HideRejected { get; set; }
    }
}
