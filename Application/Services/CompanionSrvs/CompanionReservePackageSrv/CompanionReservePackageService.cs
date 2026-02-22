using Application.Services.CompanionSrvs.CompanionReservePackageSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReservePackageSrv
{
    public class CompanionReservePackageService : ICompanionReservePackageService
    {
        private readonly IDataBaseContext _context;

        public CompanionReservePackageService(IDataBaseContext _context)
        {
            this._context = _context;
        }

        public async Task InsertOrUpdateAsync(CompanionReserve companionReserve, long CompanionAssistancePackagesId)
        {
            var item = await _context.CompanionAssistancePackages.AsTracking().FirstOrDefaultAsync(s => s.Id == CompanionAssistancePackagesId);
            if (item != null)
            {
                companionReserve.CompanionAssistancePackages.Add(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task InsertOrUpdateAsync(CompanionReserve companionReserve, List<long> CompanionAssistancePackagesId)
        {
            if (companionReserve.CompanionAssistancePackages != null)
            {
                companionReserve.CompanionAssistancePackages.Clear();
                await _context.SaveChangesAsync();
            }
            else
            {
                companionReserve.CompanionAssistancePackages = new List<CompanionAssistancePackage>();
            }
            foreach (var item in CompanionAssistancePackagesId)
            {
                await InsertOrUpdateAsync(companionReserve, item);
            }
        }
    }
}
