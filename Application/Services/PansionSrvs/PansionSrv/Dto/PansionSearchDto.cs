using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionSearchDto : BaseSearchDto<Pansion, PansionVDto>, IPansionSearchFields
    {
        public PansionSearchDto(PansionInputDto dto, IQueryable<Pansion> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.IsSchool = dto.IsSchool;
            this.Approve = dto.Approve;
            this.CompanionId = dto.CompanionId;
            this.NeighborhoodIds = dto.NeighborhoodIds;
            this.StateId = dto.StateId;
            this.CityId = dto.CityId;
            this.PetId = dto.PetId;
            this.Suggested = dto.Suggested;
        }
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
