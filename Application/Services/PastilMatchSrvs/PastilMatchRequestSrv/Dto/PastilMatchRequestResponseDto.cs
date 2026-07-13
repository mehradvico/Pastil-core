using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto
{
    public class PastilMatchRequestResponseDto : Id_FieldDto
    {
        public long StatusId { get; set; }
    }
}
