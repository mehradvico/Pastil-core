using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreSrv
{
    public class SettlementStoreService : CommonSrv<SettlementStore, SettlementStoreDto>, ISettlementStoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public SettlementStoreService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<SettlementStoreDto>> InsertAsyncDto(SettlementStoreDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<SettlementStoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<SettlementStore>(dto);
                    await _context.SettlementStores.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<SettlementStoreDto>(true, mapper.Map<SettlementStoreDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<SettlementStoreDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
