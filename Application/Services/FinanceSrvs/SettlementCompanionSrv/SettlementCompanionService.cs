using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Iface;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementCompanionSrv
{
    public class SettlementCompanionService : CommonSrv<SettlementCompanion, SettlementCompanionDto>, ISettlementCompanionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public SettlementCompanionService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<SettlementCompanionDto>> InsertAsyncDto(SettlementCompanionDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<SettlementCompanionDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<SettlementCompanion>(dto);
                    await _context.SettlementCompanions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<SettlementCompanionDto>(true, mapper.Map<SettlementCompanionDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<SettlementCompanionDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
