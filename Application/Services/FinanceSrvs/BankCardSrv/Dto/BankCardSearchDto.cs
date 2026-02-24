using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.BankCardSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.BankCardSrv.Dto
{
    public class BankCardSearchDto : BaseSearchDto<BankCard, BankCardVDto>, IBankCardSearchFields
    {
        public BankCardSearchDto(BankCardInputDto dto, IQueryable<BankCard> list, IMapper mapper) : base(dto, list, mapper)
        {
        }
    }
}
