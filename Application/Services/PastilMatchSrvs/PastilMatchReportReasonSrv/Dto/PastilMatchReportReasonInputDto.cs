using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto
{
    public class PastilMatchReportReasonInputDto : BaseInputDto, IPastilMatchReportReasonSearchFields
    {
        public bool? IsDescriptionRequired { get; set; }
    }
}
