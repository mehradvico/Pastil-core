using Application.Common.Service;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Iface;
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
    }
}
