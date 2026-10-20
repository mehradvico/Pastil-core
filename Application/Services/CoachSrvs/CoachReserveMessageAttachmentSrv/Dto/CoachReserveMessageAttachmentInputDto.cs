using Entities.Entities.CoachField;
using Application.Common.Dto.Input;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface;

namespace Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto
{
    public class CoachReserveMessageAttachmentInputDto : BaseInputDto, ICoachReserveMessageAttachmentSearchFields
    {
        public long? CoachReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
