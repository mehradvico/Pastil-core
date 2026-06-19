using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv.Iface
{
    public interface IClubRewardService : ICommonSrv<ClubReward, ClubRewardDto>
    {
        ClubRewardSearchDto Search(ClubRewardInputDto baseSearchDto);
        Task<BaseResultDto<ClubRewardVDto>> FindAsyncVDto(long id);
    }
}
