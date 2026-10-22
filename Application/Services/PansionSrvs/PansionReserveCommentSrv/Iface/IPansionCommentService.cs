using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionCommentSrv.Iface
{
    public interface IPansionCommentService : ICommonSrv<PansionComment, PansionCommentDto>
    {
        Task<BaseResultDto> UpdateDtoAsync(PansionCommentDto dto);
        // پاسخ مالک مرکز به یک نظر (فقط Answer؛ وضعیت تأیید دست‌نخورده می‌ماند). companionId = نمایندگیِ مالک.
        Task<BaseResultDto> ReplyAsync(long commentId, string answer, long companionId);
        PansionCommentSearchDto Search(PansionCommentInputDto baseSearchDto);
        Task UpdatePansionCommentRateAsync(long Id);
    }
}
