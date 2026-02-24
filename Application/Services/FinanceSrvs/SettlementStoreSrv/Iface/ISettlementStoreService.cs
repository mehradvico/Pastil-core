using Application.Common.Interface;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreSrv.Iface
{
    public interface ISettlementStoreService : ICommonSrv<SettlementStore, SettlementStoreDto>
    {
    }
}
