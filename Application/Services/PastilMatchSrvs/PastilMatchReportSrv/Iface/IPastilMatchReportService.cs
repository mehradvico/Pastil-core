using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface
{
    public interface IPastilMatchReportService : ICommonSrv<PastilMatchReport, PastilMatchReportDto>
    {
        PastilMatchReportSearchDto Search(PastilMatchReportInputDto dto);
        Task<BaseResultDto<PastilMatchReportVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateReviewDto(PastilMatchReportReviewDto dto);
    }
}
