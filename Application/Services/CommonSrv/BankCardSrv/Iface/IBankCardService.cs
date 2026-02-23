using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.BankCardSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.BankCardSrv.Iface
{
    public interface IBankCardService : ICommonSrv<BankCard, BankCardDto>
    {
        BaseSearchDto<BankCardVDto> Search(BankCardInputDto baseSearchDto);
    }
}
