using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionApproveDto : Id_FieldDto
    {
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
    }
}
