using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageDto : Id_FieldDto
    {
        public long ClinicReserveId { get; set; }
        public long? SenderUserId { get; set; }
        public long ClinicReserveMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public string Content { get; set; }
    }
}
