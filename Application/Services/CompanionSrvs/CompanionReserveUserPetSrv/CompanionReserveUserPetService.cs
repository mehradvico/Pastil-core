using Application.Services.CompanionSrvs.CompanionReserveUserPetSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveUserPetSrv
{
    public class CompanionReserveUserPetService : ICompanionReserveUserPetService
    {
        private readonly IDataBaseContext _context;

        public CompanionReserveUserPetService(IDataBaseContext _context)
        {
            this._context = _context;
        }

        public async Task InsertOrUpdateAsync(CompanionReserve companionReserve, long userPetId)
        {
            var item = await _context.UserPets.AsTracking().FirstOrDefaultAsync(s =>
                s.Id == userPetId && s.UserId == companionReserve.BookerId);
            if (item != null)
            {
                companionReserve.UserPets.Add(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task InsertOrUpdateAsync(CompanionReserve companionReserve, List<long> userPetId)
        {
            if (companionReserve.UserPets != null)
            {
                companionReserve.UserPets.Clear();
                await _context.SaveChangesAsync();
            }
            else
            {
                companionReserve.UserPets = new List<UserPet>();
            }
            foreach (var item in userPetId)
            {
                await InsertOrUpdateAsync(companionReserve, item);
            }
        }
    }
}
