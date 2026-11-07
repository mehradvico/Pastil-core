using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionSrv;
using Application.Services.ProviderSrvs.ProviderSrv.Dto;
using Application.Services.ProviderSrvs.ProviderSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSrv
{
    public class ProviderService : CommonSrv<Provider, ProviderDto>, IProviderService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public ProviderService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        private IQueryable<Provider> WithDetails()
        {
            return _context.Providers
                .Include(x => x.User)
                .Include(x => x.RoleAssignments).ThenInclude(x => x.ProviderRole)
                .Include(x => x.Subscriptions).ThenInclude(x => x.Plan)
                .Include(x => x.Companions)
                .Include(x => x.Stores)
                .AsSplitQuery();
        }

        public override async Task<BaseResultDto<ProviderDto>> FindAsyncDto(long id)
        {
            var item = await _context.Providers.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            return new BaseResultDto<ProviderDto>(item != null, mapper.Map<ProviderDto>(item));
        }

        public async Task<BaseResultDto<ProviderVDto>> FindAsyncVDto(long id)
        {
            var item = await WithDetails().FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            var vdto = mapper.Map<ProviderVDto>(item);
            if (vdto != null)
                FillVenueCounts(new List<ProviderVDto> { vdto });
            return new BaseResultDto<ProviderVDto>(item != null, vdto);
        }

        public async Task<long?> GetProviderIdByUserAsync(long userId)
        {
            return await _context.Providers.AsNoTracking()
                .Where(x => x.UserId == userId && !x.Deleted)
                .Select(x => (long?)x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Provider> EnsureForUserAsync(long userId, bool approved)
        {
            var existing = await _context.Providers.FirstOrDefaultAsync(x => x.UserId == userId && !x.Deleted);
            if (existing != null)
                return existing;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.Deleted);
            if (user == null)
                return null;
            user.IsProvider = true;
            var provider = new Provider { UserId = userId, Active = true, Approved = approved, CreateDate = DateTime.Now };
            await _context.Providers.AddAsync(provider);
            return provider;
        }

        public ProviderSearchDto Search(ProviderInputDto input)
        {
            var model = WithDetails().Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                model = model.Where(x => x.User.Mobile.Contains(input.Q)
                    || x.User.FirstName.Contains(input.Q)
                    || x.User.LastName.Contains(input.Q));
            }
            if (input.Active.HasValue)
                model = model.Where(x => x.Active == input.Active.Value);
            if (input.Approved.HasValue)
                model = model.Where(x => x.Approved == input.Approved.Value);
            if (input.PlanId.HasValue)
            {
                var now = DateTime.Now;
                model = model.Where(x => x.Subscriptions.Any(s => !s.Deleted && s.PlanId == input.PlanId.Value && s.StartDate <= now && s.EndDate > now));
            }
            if (input.ProviderRoleId.HasValue)
                model = model.Where(x => x.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));
            model = input.SortBy == SortEnum.Old ? model.OrderBy(x => x.Id) : model.OrderByDescending(x => x.Id);
            var result = new ProviderSearchDto(input, model, mapper);
            FillVenueCounts(result.List);
            return result;
        }

        /// <summary>
        /// شمار مراکز هر ارائه‌دهنده از همه‌ی نوع‌ها، با همان قاعده‌ی هاب نمایندگی‌ها: نمایندگی قدیمی‌ای که کاملاً به آرایشگاه/کلینیک/مربی
        /// منتقل شده یا فقط ظرف پانسیون/مدرسه است شمرده نمی‌شود (خودِ آن مرکز در شمار نوع خودش هست).
        /// </summary>
        private void FillVenueCounts(List<ProviderVDto> items)
        {
            if (items == null || items.Count == 0)
                return;
            var ids = items.Select(x => x.Id).ToList();

            Dictionary<long, int> Count(IQueryable<long?> providerIds) => providerIds
                .Where(p => p != null && ids.Contains(p.Value))
                .GroupBy(p => p.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionary(x => x.Key, x => x.Count);

            var companions = Count(_context.Companions.AsNoTracking().Where(c => !c.Deleted)
                .Where(CompanionMigrationFilter.NotFullyMigrated(_context))
                .Where(CompanionMigrationFilter.NotOnlyPansionSchoolContainer(_context))
                .Select(c => c.ProviderId));
            var stores = Count(_context.Stores.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));
            var groomers = Count(_context.Groomers.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));
            var clinics = Count(_context.Clinics.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));
            var coaches = Count(_context.Coaches.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));
            var schools = Count(_context.Schools.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));
            var pansions = Count(_context.Pansions.AsNoTracking().Where(x => !x.Deleted).Select(x => x.ProviderId));

            foreach (var item in items)
            {
                item.CompanionCount = companions.GetValueOrDefault(item.Id);
                item.StoreCount = stores.GetValueOrDefault(item.Id);
                item.GroomerCount = groomers.GetValueOrDefault(item.Id);
                item.ClinicCount = clinics.GetValueOrDefault(item.Id);
                item.CoachCount = coaches.GetValueOrDefault(item.Id);
                item.SchoolCount = schools.GetValueOrDefault(item.Id);
                item.PansionCount = pansions.GetValueOrDefault(item.Id);
                item.VenueCount = item.CompanionCount + item.StoreCount + item.GroomerCount + item.ClinicCount + item.CoachCount + item.SchoolCount + item.PansionCount;
            }
        }

        public override async Task<BaseResultDto<ProviderDto>> InsertAsyncDto(ProviderDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.UserId && !x.Deleted);
            if (user == null)
                return new BaseResultDto<ProviderDto>(false, Resource.Notification.UserNotFound, dto);
            if (await _context.Providers.AnyAsync(x => x.UserId == dto.UserId && !x.Deleted))
                return new BaseResultDto<ProviderDto>(false, Resource.Notification.ProviderAlreadyExistsForUser, dto);

            var item = new Provider
            {
                UserId = dto.UserId,
                Active = dto.Active,
                Approved = dto.Approved,
                Note = dto.Note,
                CreateDate = DateTime.Now,
            };
            user.IsProvider = true;
            await _context.Providers.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<ProviderDto>(true, mapper.Map<ProviderDto>(item));
        }

        /// <summary>فقط Active/Approved/Note قابل ویرایش است؛ UserId بعد از ثبت ثابت می‌ماند.</summary>
        public override BaseResultDto UpdateDto(ProviderDto dto)
        {
            var item = _context.Providers.FirstOrDefault(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            item.Active = dto.Active;
            item.Approved = dto.Approved;
            item.Note = dto.Note;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }

        public override BaseResultDto DeleteDto(long id)
        {
            var item = _context.Providers.Include(x => x.User).FirstOrDefault(x => x.Id == id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.ProviderNotFound);
            item.Deleted = true;
            item.User.IsProvider = false;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }
    }
}
