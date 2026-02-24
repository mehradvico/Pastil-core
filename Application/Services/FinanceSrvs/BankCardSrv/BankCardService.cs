using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.FinanceSrvs.BankCardSrv.Dto;
using Application.Services.FinanceSrvs.BankCardSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.BankCardSrv
{
    public class BankCardService : CommonSrv<BankCard, BankCardDto>, IBankCardService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public BankCardService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }


        public BaseSearchDto<BankCardVDto> Search(BankCardInputDto baseSearchDto)
        {
            var model = _context.BankCards.AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.BankName.Contains(baseSearchDto.Q)).OrderBy(o => o.BankName);
            }
            return new BaseSearchDto<BankCard, BankCardVDto>(baseSearchDto, model, mapper);
        }
    }
}
