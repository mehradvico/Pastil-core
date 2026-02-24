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
    }
}
