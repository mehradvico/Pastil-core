using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveUpdateDto : Id_FieldDto
    {
        public long? CompanionAssistanceTimeId { get; set; }
        public bool? IsFemale { get; set; }
        public List<long> UserPetIds { get; set; }
    }
}
