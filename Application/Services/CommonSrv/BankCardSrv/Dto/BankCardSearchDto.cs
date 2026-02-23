using Application.Common.Dto.Result;
using Application.Services.CommonSrv.BankCardSrv.Dto;
using Application.Services.CommonSrv.BankCardSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.BankCardSrv.Dto
{
    public class BankCardSearchDto : BaseSearchDto<BankCard, BankCardVDto>, IBankCardSearchFields
    {
        public BankCardSearchDto(BankCardInputDto dto, IQueryable<BankCard> list, IMapper mapper) : base(dto, list, mapper)
        {
        }
    }
}
