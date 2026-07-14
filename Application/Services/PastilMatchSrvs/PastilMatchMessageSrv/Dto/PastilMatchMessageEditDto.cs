using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageEditDto : Id_FieldDto
    {
        public string Content { get; set; }
    }
}
