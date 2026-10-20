using Entities.Entities.ClinicField;
using Application.Common.Dto.Input;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto
{
    public class ClinicReserveMessageAttachmentInputDto : BaseInputDto, IClinicReserveMessageAttachmentSearchFields
    {
        public long? ClinicReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
