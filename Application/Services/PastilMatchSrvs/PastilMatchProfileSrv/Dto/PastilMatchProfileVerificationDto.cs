using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileVerificationDto : Id_FieldDto
    {
        public bool IsVerified { get; set; }
        public string AdminDescription { get; set; }
    }
}
