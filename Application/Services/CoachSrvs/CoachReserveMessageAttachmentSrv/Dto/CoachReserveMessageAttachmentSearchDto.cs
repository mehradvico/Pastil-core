using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto
{
    public class CoachReserveMessageAttachmentSearchDto : BaseSearchDto<CoachReserveMessageAttachment, CoachReserveMessageAttachmentVDto>, ICoachReserveMessageAttachmentSearchFields
    {
        public CoachReserveMessageAttachmentSearchDto(CoachReserveMessageAttachmentInputDto dto, IQueryable<CoachReserveMessageAttachment> list, IMapper mapper) : base(dto, list, mapper)
        {
            CoachReserveMessageId = dto.CoachReserveMessageId;
            ContentType = dto.ContentType;
        }

        public long? CoachReserveMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
