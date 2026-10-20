using Entities.Entities.ClinicField;
using Application.Common.Dto.Field;
using System;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto
{
    public class ClinicReserveMessageReplyVDto : Id_FieldDto
    {
        public long? SenderUserId { get; set; }
        public long ClinicReserveMessageTypeId { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
