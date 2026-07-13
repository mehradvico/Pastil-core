using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto
{
    public class PastilMatchBlockVDto : Id_FieldDto
    {
        public long BlockerUserId { get; set; }
        public long BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }
        public DateTime CreateDate { get; set; }

        public UserVDto BlockerUser { get; set; }
        public UserVDto BlockedUser { get; set; }
        public PastilMatchDto PastilMatch { get; set; }
    }
}
