using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto
{
    public class PastilMatchMessageAttachmentSearchDto : BaseSearchDto<PastilMatchMessageAttachment, PastilMatchMessageAttachmentVDto>, IPastilMatchMessageAttachmentSearchFields
    {
        public PastilMatchMessageAttachmentSearchDto(PastilMatchMessageAttachmentInputDto dto, IQueryable<PastilMatchMessageAttachment> list, IMapper mapper) : base(dto, list, mapper)
        {
            PastilMatchMessageId = dto.PastilMatchMessageId;
            ContentType = dto.ContentType;
        }

        public long? PastilMatchMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
