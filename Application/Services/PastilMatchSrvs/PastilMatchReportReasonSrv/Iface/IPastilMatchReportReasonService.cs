using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface
{
    public interface IPastilMatchReportReasonService : ICommonSrv<PastilMatchReportReason, PastilMatchReportReasonDto>
    {
        PastilMatchReportReasonSearchDto Search(PastilMatchReportReasonInputDto dto);
        Task<BaseResultDto<PastilMatchReportReasonVDto>> FindAsyncVDto(long id);
        BaseResultDto UpdateActiveDto(PastilMatchReportReasonActiveDto dto);
    }
}
