using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto
{
    public class PastilMatchReportReasonActiveDto : Id_FieldDto
    {
        public bool Active { get; set; }
    }
}
