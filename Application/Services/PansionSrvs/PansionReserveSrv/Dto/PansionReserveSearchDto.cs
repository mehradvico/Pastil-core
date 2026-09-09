using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Dto
{
    public class PansionReserveSearchDto : BaseSearchDto<PansionReserve, PansionReserveVDto>, IPansionReserveSearchFields
    {
        public PansionReserveSearchDto(PansionReserveInputDto dto, IQueryable<PansionReserve> list, IMapper mapper) : base(dto, list, mapper)
        {

            this.BookerId = dto.BookerId;
            this.UserPetId = dto.UserPetId;
            this.PansionId = dto.PansionId;
            this.CompanionId = dto.CompanionId;
            this.StatusId = dto.StatusId;
            this.IsDaycare = dto.IsDaycare;
        }

        public long? BookerId { get; set; }
        public long? UserPetId { get; set; }
        public long? PansionId { get; set; }
        public long? CompanionId { get; set; }
        public long? StatusId { get; set; }
        public bool? IsDaycare { get; set; }
    }
}
