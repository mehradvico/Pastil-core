using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface
{
    public interface IPastilMatchReportReasonSearchFields
    {
        public bool? IsDescriptionRequired { get; set; }
    }
}
