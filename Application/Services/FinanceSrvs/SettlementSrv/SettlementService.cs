using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv
{
    public class SettlementService : CommonSrv<Settlement, SettlementDto>, ISettlementService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public SettlementService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<SettlementVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Settlements.Include(s => s.UserBankCard).Include(s => s.Companion).Include(s => s.Store).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<SettlementVDto>(true, mapper.Map<SettlementVDto>(item));
            }
            return new BaseResultDto<SettlementVDto>(false, mapper.Map<SettlementVDto>(item));
        }

        public SettlementSearchDto Search(SettlementInputDto baseSearchDto)
        {
            var model = _context.Settlements.Include(s => s.UserBankCard).Include(s => s.Companion).Include(s => s.Store).AsQueryable();

            if (baseSearchDto.StoreId.HasValue)
            {
                model = model.Where(s => s.StoreId == baseSearchDto.StoreId.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new SettlementSearchDto(baseSearchDto, model, mapper);
        }
    }
}
