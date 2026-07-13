using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto
{
    public class PastilMatchReportReasonDto : Id_FieldDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public bool IsDescriptionRequired { get; set; }
        public bool Active { get; set; }
    }
}
