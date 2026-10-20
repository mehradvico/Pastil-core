using Entities.Entities.ClinicField;
using Application.Common.Dto.Input;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageInputDto : BaseInputDto, IClinicReserveMessageSearchFields
    {
        public long? ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long? ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
