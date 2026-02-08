using Application.Common.Dto.Input;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionInputDto : BaseInputDto, IPansionSearchFields
    {
        public bool? IsSchool { get; set; }
        public long? CompanionId { get; set; }
        public bool? Approve { get; set; }
        public List<long>? NeighborhoodIds { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public bool? Suggested { get; set; }
        public long? PetId { get; set; }
    }
}
