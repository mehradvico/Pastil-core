using Application.Common.Dto.Input;
using Application.Common.Enumerable.Code;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionInputDto : BaseInputDto, ICompanionSearchFields
    {
        public long? OwnerId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public long? TypeId { get; set; }
        public long? PetId { get; set; }
        public List<long> NeighborhoodIds { get; set; }
        public bool? Approved { get; set; }
        public bool? GoldAccount { get; set; }
        public bool? SilverAccount { get; set; }
        public bool? HasInsurance { get; set; }
        public bool? IsPersonal { get; set; }
        public bool? ShowToSite { get; set; }
        public long? AssistanceId { get; set; }
        public CompanionAssistanceTypeEnum? AssistanceType { get; set; }
    }
}
