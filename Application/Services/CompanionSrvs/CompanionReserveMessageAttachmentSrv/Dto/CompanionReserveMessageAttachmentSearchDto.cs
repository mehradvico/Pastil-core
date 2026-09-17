using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto
{
    public class CompanionReserveMessageAttachmentSearchDto : BaseSearchDto<CompanionReserveMessageAttachment, CompanionReserveMessageAttachmentVDto>, ICompanionReserveMessageAttachmentSearchFields
    {
        public CompanionReserveMessageAttachmentSearchDto(CompanionReserveMessageAttachmentInputDto dto, IQueryable<CompanionReserveMessageAttachment> list, IMapper mapper) : base(dto, list, mapper)
        {
            CompanionReserveMessageId = dto.CompanionReserveMessageId;
            ContentType = dto.ContentType;
        }

        public long? CompanionReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
