using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto
{
    public class CompanionReserveMessageAttachmentInputDto : BaseInputDto, ICompanionReserveMessageAttachmentSearchFields
    {
        public long? CompanionReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
