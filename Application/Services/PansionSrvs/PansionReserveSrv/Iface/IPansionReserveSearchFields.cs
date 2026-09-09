using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Iface
{
    public interface IPansionReserveSearchFields
    {
        public long? BookerId { get; set; }
        public long? UserPetId { get; set; }
        public long? PansionId { get; set; }
        public long? CompanionId { get; set; }
        public long? StatusId { get; set; }
        public bool? IsDaycare { get; set; }
    }
}
