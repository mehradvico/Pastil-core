using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessagePinDto : Id_FieldDto
    {
        public bool IsPinned { get; set; }
    }
}
