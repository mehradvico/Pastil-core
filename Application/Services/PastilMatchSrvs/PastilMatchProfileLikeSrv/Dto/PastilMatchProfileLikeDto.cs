using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Dto
{
    public class PastilMatchProfileLikeDto : Id_FieldDto
    {
        public long LikerProfileId { get; set; }
        public long LikedProfileId { get; set; }
    }
}
