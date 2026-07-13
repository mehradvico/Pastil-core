using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto
{
    public class PastilMatchBlockInputDto : BaseInputDto, IPastilMatchBlockSearchFields
    {
        public long? BlockerUserId { get; set; }
        public long? BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }
    }
}
