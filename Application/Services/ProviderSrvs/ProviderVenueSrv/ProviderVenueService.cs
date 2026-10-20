using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Dto;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Iface;
using Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderVenueSrv
{
    public class ProviderVenueService : IProviderVenueService
    {
        private readonly IDataBaseContext _context;
        private readonly IProviderSubscriptionSyncService _sync;

        public ProviderVenueService(IDataBaseContext _context, IProviderSubscriptionSyncService sync)
        {
            this._context = _context;
            this._sync = sync;
        }

        public async Task<BaseResultDto<ProviderVenuesVDto>> GetVenuesAsync(long providerId)
        {
            if (!await _context.Providers.AnyAsync(x => x.Id == providerId && !x.Deleted))
                return new BaseResultDto<ProviderVenuesVDto>(false, Resource.Notification.ProviderNotFound, null);

            var result = new ProviderVenuesVDto
            {
                Companions = await _context.Companions.AsNoTracking()
                    .Where(x => x.ProviderId == providerId && !x.Deleted)
                    .Select(x => new ProviderVenueVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved })
                    .ToListAsync(),
                Groomers = await _context.Groomers.AsNoTracking()
                    .Where(x => x.ProviderId == providerId && !x.Deleted)
                    .Select(x => new ProviderVenueVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved })
                    .ToListAsync(),

                Clinics = await _context.Clinics.AsNoTracking()
                    .Where(x => x.ProviderId == providerId && !x.Deleted)
                    .Select(x => new ProviderVenueVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved })
                    .ToListAsync(),
                Stores = await _context.Stores.AsNoTracking()
                    .Where(x => x.ProviderId == providerId && !x.Deleted)
                    .Select(x => new ProviderVenueVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved })
                    .ToListAsync(),
            };
            return new BaseResultDto<ProviderVenuesVDto>(true, result);
        }

        /// <summary>فقط ProviderId عوض می‌شود؛ OwnerId قدیمی دست‌نخورده می‌ماند تا ردیابی‌های مبتنی بر آن نشکند.</summary>
        public async Task<BaseResultDto> AssignCompanionAsync(ProviderVenueAssignDto dto)
        {
            if (!await _context.Providers.AnyAsync(x => x.Id == dto.ProviderId && !x.Deleted))
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            var item = await _context.Companions.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.CompanionNotFound);
            item.ProviderId = dto.ProviderId;
            await _context.SaveChangesAsync();
            // نمایندگی تازه‌وصل‌شده باید سطح اشتراک Provider جدید را بگیرد (یا اگر اشتراک ندارد، سطح قبلی پاک شود).
            await _sync.SyncProviderAsync(dto.ProviderId);
            return new BaseResultDto(true);
        }

        /// <summary>آرایشگاه (Groomer) را به ارائه‌دهنده وصل می‌کند؛ مالک آرایشگاه = کاربر ارائه‌دهنده و سطح اشتراک همگام می‌شود.</summary>
        public async Task<BaseResultDto> AssignGroomerAsync(ProviderVenueAssignDto dto)
        {
            var provider = await _context.Providers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProviderId && !x.Deleted);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            var item = await _context.Groomers.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            item.ProviderId = provider.Id;
            item.OwnerId = provider.UserId;
            await _context.SaveChangesAsync();
            await _sync.SyncProviderAsync(provider.Id);
            return new BaseResultDto(true);
        }

        /// <summary>کلینیک (Clinic) را به ارائه‌دهنده وصل می‌کند؛ مالک کلینیک = کاربر ارائه‌دهنده و سطح اشتراک همگام می‌شود.</summary>
        public async Task<BaseResultDto> AssignClinicAsync(ProviderVenueAssignDto dto)
        {
            var provider = await _context.Providers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.ProviderId && !x.Deleted);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            var item = await _context.Clinics.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            item.ProviderId = provider.Id;
            item.OwnerId = provider.UserId;
            await _context.SaveChangesAsync();
            await _sync.SyncProviderAsync(provider.Id);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AssignStoreAsync(ProviderVenueAssignDto dto)
        {
            if (!await _context.Providers.AnyAsync(x => x.Id == dto.ProviderId && !x.Deleted))
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            var item = await _context.Stores.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            item.ProviderId = dto.ProviderId;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
    }
}
