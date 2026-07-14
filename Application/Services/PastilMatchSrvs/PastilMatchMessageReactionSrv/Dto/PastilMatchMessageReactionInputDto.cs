using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto
{
    public class PastilMatchMessageReactionInputDto : BaseInputDto, IPastilMatchMessageReactionSearchFields
    {
        public long? PastilMatchMessageId { get; set; }
        public long? ReactorProfileId { get; set; }
        public string Reaction { get; set; }
    }
}
