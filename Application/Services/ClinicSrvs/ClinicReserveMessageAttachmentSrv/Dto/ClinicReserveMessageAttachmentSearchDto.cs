using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto
{
    public class ClinicReserveMessageAttachmentSearchDto : BaseSearchDto<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentVDto>, IClinicReserveMessageAttachmentSearchFields
    {
        public ClinicReserveMessageAttachmentSearchDto(ClinicReserveMessageAttachmentInputDto dto, IQueryable<ClinicReserveMessageAttachment> list, IMapper mapper) : base(dto, list, mapper)
        {
            ClinicReserveMessageId = dto.ClinicReserveMessageId;
            ContentType = dto.ContentType;
        }

        public long? ClinicReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
