using Application.Common.Dto.Result;
using Application.Services.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.ProductSrvs.StoreUserSrv.Dto;
using Application.Services.StoreSrvs.StoreUserSrv.Iface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.StoreSrv.StoreUserSrv
{
    public class StoreUserService : IStoreUserService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public StoreUserService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public async Task<BaseResultDto> GetAllAsync(StoreUserDto storeUser)
        {
            if (storeUser.UserId > 0)
            {
                var storesQuery = _context.Stores
                    .Include(s => s.Picture)
                    .Where(s =>
                        !s.Deleted &&
                        s.Users.Any(u => u.Id == storeUser.UserId));

                if (storeUser.Active.HasValue)
                {
                    storesQuery = storesQuery.Where(s => s.Active == storeUser.Active.Value);
                }

                var stores = await storesQuery
                    .AsNoTracking()
                    .ToListAsync();

                if (stores.Any())
                {
                    return new BaseResultDto<List<StoreMinVDto>>(
                        true,
                        data: mapper.Map<List<StoreMinVDto>>(stores)
                    );
                }
            }
            else if (storeUser.StoreId > 0)
            {
                var usersQuery = _context.Users
                    .Where(u =>
                        u.Stores.Any(s =>
                            s.Id == storeUser.StoreId &&
                            !s.Deleted));

                if (storeUser.Active.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.Locked == !storeUser.Active.Value);
                }

                var users = await usersQuery
                    .AsNoTracking()
                    .ToListAsync();

                if (users.Any())
                {
                    return new BaseResultDto<List<UserMinVDto>>(
                        true,
                        data: mapper.Map<List<UserMinVDto>>(users)
                    );
                }
            }

            return new BaseResultDto(false);
        }
        public async Task<BaseResultDto> InsertAsync(StoreUserDto storeUser)
        {
            try
            {
                var user = await _context.Users.Include(s => s.Stores).AsTracking().FirstOrDefaultAsync(s => s.Id == storeUser.UserId);
                var store = await _context.Stores.Include(s => s.Users).AsTracking().FirstOrDefaultAsync(s => s.Id == storeUser.StoreId);
                if (user != null && store != null)
                {
                    store.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                return new BaseResultDto(true);
            }
            catch
            {
                return new BaseResultDto(false, val: Resource.Notification.Unsuccess);
            }
        }
        public async Task<BaseResultDto> RemoveAsync(StoreUserDto storeUser)
        {
            try
            {
                var user = await _context.Users.Include(s => s.Stores).AsTracking().FirstOrDefaultAsync(s => s.Id == storeUser.UserId);
                var store = await _context.Stores.Include(s => s.Users).AsTracking().FirstOrDefaultAsync(s => s.Id == storeUser.StoreId);
                if (user != null && store != null)
                {
                    store.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
                return new BaseResultDto(true);
            }
            catch
            {
                return new BaseResultDto(false, val: Resource.Notification.Unsuccess);
            }
        }



    }
}
