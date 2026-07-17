using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionSearchDto : BaseSearchDto<Companion, CompanionVDto>, ICompanionSearchFields
    {
        public CompanionSearchDto(CompanionInputDto dto, IQueryable<Companion> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.Approved = dto.Approved;
            this.OwnerId = dto.OwnerId;
            this.CityId = dto.CityId;
            this.StateId = dto.StateId;
            this.NeighborhoodIds = dto.NeighborhoodIds;
            this.GoldAccount = dto.GoldAccount;
            this.SilverAccount = dto.SilverAccount;
            this.HasInsurance = dto.HasInsurance;
            this.IsPersonal = dto.IsPersonal;
            this.AssistanceId = dto.AssistanceId;
            this.PetId = dto.PetId;
            this.AssistanceType = dto.AssistanceType;
        }
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
        public long? AssistanceId { get; set; }
        public CompanionAssistanceTypeEnum? AssistanceType { get; set; }

    }
}
