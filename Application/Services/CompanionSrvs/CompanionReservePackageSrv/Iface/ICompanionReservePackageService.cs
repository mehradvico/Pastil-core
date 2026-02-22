using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReservePackageSrv.Iface
{
    public interface ICompanionReservePackageService
    {
        Task InsertOrUpdateAsync(CompanionReserve companionReserve, long CompanionAssistancePackageId);
        Task InsertOrUpdateAsync(CompanionReserve companionReserve, List<long> CompanionAssistancePackagesIds);
    }
}
