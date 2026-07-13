using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto
{
    public class PastilMatchReportReasonSearchDto : BaseSearchDto<PastilMatchReportReason, PastilMatchReportReasonVDto>, IPastilMatchReportReasonSearchFields
    {
        public PastilMatchReportReasonSearchDto(PastilMatchReportReasonInputDto dto, IQueryable<PastilMatchReportReason> list, IMapper mapper) : base(dto, list, mapper)
        {
            IsDescriptionRequired = dto.IsDescriptionRequired;
        }

        public bool? IsDescriptionRequired { get; set; }
    }
}
