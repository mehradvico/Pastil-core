using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto
{
    public class PastilMatchReportSearchDto : BaseSearchDto<PastilMatchReport, PastilMatchReportVDto>, IPastilMatchReportSearchFields
    {
        public PastilMatchReportSearchDto(PastilMatchReportInputDto dto, IQueryable<PastilMatchReport> list, IMapper mapper) : base(dto, list, mapper)
        {
            ReporterUserId = dto.ReporterUserId;
            ReportedUserId = dto.ReportedUserId;
            ReportedProfileId = dto.ReportedProfileId;
            PastilMatchId = dto.PastilMatchId;
            PastilMatchMessageId = dto.PastilMatchMessageId;
            PastilMatchReportReasonId = dto.PastilMatchReportReasonId;
            IsReviewed = dto.IsReviewed;
        }

        public long? ReporterUserId { get; set; }
        public long? ReportedUserId { get; set; }
        public long? ReportedProfileId { get; set; }
        public long? PastilMatchId { get; set; }
        public long? PastilMatchMessageId { get; set; }
        public long? PastilMatchReportReasonId { get; set; }
        public bool? IsReviewed { get; set; }
    }
}
