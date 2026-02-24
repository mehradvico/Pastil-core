using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv.Iface
{
    public interface IUserBankCardService : ICommonSrv<UserBankCard, UserBankCardDto>
    {
        UserBankCardSearchDto Search(UserBankCardInputDto baseSearchDto);
        Task<BaseResultDto<UserBankCardVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> UpdateUserBankCardApproveAsyncDto(UserBankCardApproveDto dto);


    }
}
