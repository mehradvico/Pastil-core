using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface;
using AutoMapper;
using Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv.Iface;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv
{
    public class ProviderSubscriptionService : IProviderSubscriptionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProviderSubscriptionSyncService _sync;

        public ProviderSubscriptionService(IDataBaseContext _context, IMapper mapper, IProviderSubscriptionSyncService sync)
        {
            this._context = _context;
            this.mapper = mapper;
            this._sync = sync;
        }

        public ProviderSubscriptionSearchDto Search(ProviderSubscriptionInputDto input)
        {
            var model = _context.ProviderSubscriptions.Include(x => x.Plan).Where(x => !x.Deleted);
            if (input.ProviderId.HasValue)
                model = model.Where(x => x.ProviderId == input.ProviderId.Value);
            if (input.PlanId.HasValue)
                model = model.Where(x => x.PlanId == input.PlanId.Value);
            if (input.ActiveNow == true)
            {
                var now = DateTime.Now;
                model = model.Where(x => x.StartDate <= now && x.EndDate > now);
            }
            model = model.OrderByDescending(x => x.StartDate);
            return new ProviderSubscriptionSearchDto(input, model, mapper);
        }

        public async Task<BaseResultDto<ProviderSubscriptionVDto>> GetCurrentAsync(long providerId)
        {
            var now = DateTime.Now;
            var item = await _context.ProviderSubscriptions.Include(x => x.Plan)
                .Where(x => x.ProviderId == providerId && !x.Deleted && x.StartDate <= now && x.EndDate > now)
                .OrderByDescending(x => x.Plan.Level)
                .FirstOrDefaultAsync();
            return new BaseResultDto<ProviderSubscriptionVDto>(true, item == null ? null : mapper.Map<ProviderSubscriptionVDto>(item));
        }

        public async Task<BaseResultDto<ProviderSubscriptionVDto>> AssignAsync(ProviderSubscriptionDto dto, long? adminUserId)
        {
            if (!ProviderSubscriptionRules.IsValidPeriod(dto.StartDate, dto.EndDate))
                return new BaseResultDto<ProviderSubscriptionVDto>(false, Resource.Notification.SubscriptionPeriodInvalid, null);
            if (!await _context.Providers.AnyAsync(x => x.Id == dto.ProviderId && !x.Deleted))
                return new BaseResultDto<ProviderSubscriptionVDto>(false, Resource.Notification.ProviderNotFound, null);
            var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == dto.PlanId && !x.Deleted && x.Active);
            if (plan == null)
                return new BaseResultDto<ProviderSubscriptionVDto>(false, Resource.Notification.SubscriptionPlanNotFound, null);

            // اشتراک‌هایی که در لحظه‌ی شروع جدید هنوز باز هستند، همان‌جا بسته می‌شوند.
            var overlapping = await _context.ProviderSubscriptions
                .Where(x => x.ProviderId == dto.ProviderId && !x.Deleted && x.EndDate > dto.StartDate)
                .ToListAsync();
            foreach (var old in overlapping)
            {
                if (old.StartDate >= dto.StartDate)
                    old.Deleted = true;
                else
                    old.EndDate = dto.StartDate;
            }

            var item = new ProviderSubscription
            {
                ProviderId = dto.ProviderId,
                PlanId = dto.PlanId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Note = dto.Note,
                CreatedByUserId = adminUserId,
                CreateDate = DateTime.Now,
            };
            await _context.ProviderSubscriptions.AddAsync(item);
            await _context.SaveChangesAsync();
            await _sync.SyncProviderAsync(item.ProviderId);
            item.Plan = plan;
            return new BaseResultDto<ProviderSubscriptionVDto>(true, mapper.Map<ProviderSubscriptionVDto>(item));
        }

        public async Task<BaseResultDto> CancelAsync(long id)
        {
            var item = await _context.ProviderSubscriptions.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            var now = DateTime.Now;
            if (item.EndDate > now)
                item.EndDate = item.StartDate > now ? item.StartDate : now;
            await _context.SaveChangesAsync();
            await _sync.SyncProviderAsync(item.ProviderId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteAsync(long id)
        {
            var item = await _context.ProviderSubscriptions.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            item.Deleted = true;
            await _context.SaveChangesAsync();
            await _sync.SyncProviderAsync(item.ProviderId);
            return new BaseResultDto(true);
        }
    }
}
