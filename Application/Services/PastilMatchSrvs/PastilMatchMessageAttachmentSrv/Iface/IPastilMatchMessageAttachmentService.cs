using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface
{
    public interface IPastilMatchMessageAttachmentService : ICommonSrv<PastilMatchMessageAttachment, PastilMatchMessageAttachmentDto>
    {
        PastilMatchMessageAttachmentSearchDto Search(PastilMatchMessageAttachmentInputDto dto);
        Task<BaseResultDto<PastilMatchMessageAttachmentVDto>> FindAsyncVDto(long id);
    }
}
