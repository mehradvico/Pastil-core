using Application.Common.Dto.Input;
using Application.Common.Enumerable;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Dto
{
    public class PansionReserveInputDto : BaseInputDto, IPansionReserveSearchFields
    {
        public long? BookerId { get; set; }
        public long? UserPetId { get; set; }
        public long? PansionId { get; set; }
        public long? CompanionId { get; set; }
        public long? StatusId { get; set; }
        public bool? IsDaycare { get; set; }

    }
}
