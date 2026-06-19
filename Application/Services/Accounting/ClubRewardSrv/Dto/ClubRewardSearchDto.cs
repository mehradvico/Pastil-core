using Application.Common.Dto.Result;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv.Dto
{
    public class ClubRewardSearchDto : BaseSearchDto<ClubReward, ClubRewardVDto>, IClubRewardSearchFields
    {
        public ClubRewardSearchDto(ClubRewardInputDto dto, IQueryable<ClubReward> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.RebateTypeId = dto.RebateTypeId;
        }
        public long? RebateTypeId { get; set; }
    }
}
