using Application.Common.Dto.Result;
using Application.Services.Accounting.PetMicrochipSrv;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using AutoMapper;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerProfileSrv
{
    public class GroomerProfileService : IGroomerProfileService
    {
        private const int MaxTimeRows = 100;
        private const int MaxZones = 500;
        private const int MaxPictures = 30;

        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GroomerProfileService> _logger;
        private readonly IGroomerNotifier _notifier;

        public GroomerProfileService(IDataBaseContext context, IMapper mapper, ILogger<GroomerProfileService> logger, IGroomerNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        // ------------------------------------------------------------------ پرسنل

        public async Task<BaseResultDto<List<GroomerStaffVDto>>> ListStaffAsync(long groomerId, long? ownerUserId)
        {
            var denied = await CheckGroomerAccessAsync(groomerId, ownerUserId);
            if (denied != null)
                return new BaseResultDto<List<GroomerStaffVDto>>(false, denied.Messages, null);

            var list = await _context.GroomerUsers.AsNoTracking()
                .Where(u => u.GroomerId == groomerId && !u.Deleted)
                .OrderBy(u => u.Id)
                .Select(u => new GroomerStaffVDto
                {
                    Id = u.Id, GroomerId = u.GroomerId, UserId = u.UserId,
                    FirstName = u.User.FirstName, LastName = u.User.LastName, Mobile = u.User.Mobile,
                    Active = u.Active, ActivationValue = u.ActivationValue, UserAccept = u.UserAccept, InviteDate = u.InviteDate,
                }).ToListAsync();
            return new BaseResultDto<List<GroomerStaffVDto>>(true, list);
        }

        public async Task<BaseResultDto<GroomerStaffVDto>> AddStaffAsync(GroomerStaffAddDto dto, long? ownerUserId)
        {
            try
            {
                var denied = await CheckGroomerAccessAsync(dto?.GroomerId ?? 0, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<GroomerStaffVDto>(false, denied.Messages, null);

                var mobile = PetMicrochipRules.NormalizeMobile(dto.Mobile);
                if (mobile == null)
                    return new BaseResultDto<GroomerStaffVDto>(false, Resource.Notification.InvalidData, null);
                var user = await _context.Users.AsNoTracking()
                    .Where(u => !u.Deleted && !u.Locked && u.Mobile == mobile)
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Mobile })
                    .FirstOrDefaultAsync();
                if (user == null)
                    return new BaseResultDto<GroomerStaffVDto>(false, Resource.Notification.NothingFound, null);

                var ownerId = await _context.Groomers.AsNoTracking().Where(g => g.Id == dto.GroomerId).Select(g => g.OwnerId).FirstAsync();
                if (user.Id == ownerId)
                    return new BaseResultDto<GroomerStaffVDto>(false, Resource.Notification.GroomerUserAlreadyExists, null);

                // دعوت: پرسنل تا پذیرفتنِ خودِ کاربر کار نمی‌کند. دعوتِ ردشده را می‌شود دوباره فرستاد؛ دعوت منتظر یا عضو فعلی تکراری است.
                var now = DateTime.Now;
                var existing = await _context.GroomerUsers.AsTracking()
                    .FirstOrDefaultAsync(u => u.GroomerId == dto.GroomerId && u.UserId == user.Id && !u.Deleted);
                if (existing != null && existing.UserAccept != false)
                    return new BaseResultDto<GroomerStaffVDto>(false, Resource.Notification.GroomerUserAlreadyExists, null);

                var staff = existing ?? new GroomerUser { GroomerId = dto.GroomerId, UserId = user.Id };
                staff.Active = true;
                staff.UserAccept = null;
                staff.InviteDate = now;
                if (existing == null)
                    await _context.GroomerUsers.AddAsync(staff);
                await _context.SaveChangesAsync();
                await RefreshStaffCountAsync(dto.GroomerId);
                if (_notifier != null)
                    await _notifier.StaffInvitedAsync(dto.GroomerId, user.Id);
                return new BaseResultDto<GroomerStaffVDto>(true, new GroomerStaffVDto
                {
                    Id = staff.Id, GroomerId = staff.GroomerId, UserId = user.Id,
                    FirstName = user.FirstName, LastName = user.LastName, Mobile = user.Mobile, Active = true,
                    UserAccept = null, InviteDate = now,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding groomer staff failed.");
                return new BaseResultDto<GroomerStaffVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId)
        {
            var staff = await _context.GroomerUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.GroomerId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckGroomerAccessAsync(staff.GroomerId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerUsers.Where(u => u.Id == staffId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, active));
            await RefreshStaffCountAsync(staff.GroomerId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId)
        {
            var staff = await _context.GroomerUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.GroomerId, u.UserId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckGroomerAccessAsync(staff.GroomerId, ownerUserId);
            if (denied != null)
                return denied;

            await _context.GroomerUsers.Where(u => u.Id == staffId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            // گرومر حذف‌شده دیگر نباید روی خدمات آرایشگاه بماند.
            await _context.GroomerServiceUsers
                .Where(u => u.UserId == staff.UserId && u.GroomerService.GroomerId == staff.GroomerId && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            await RefreshStaffCountAsync(staff.GroomerId);
            return new BaseResultDto(true);
        }

        public async Task<List<GroomerStaffInviteVDto>> ListInvitesAsync(long userId) =>
            await _context.GroomerUsers.AsNoTracking()
                .Where(u => u.UserId == userId && !u.Deleted && u.UserAccept == null && !u.Groomer.Deleted)
                .OrderByDescending(u => u.Id)
                .Select(u => new GroomerStaffInviteVDto { Id = u.Id, GroomerId = u.GroomerId, GroomerName = u.Groomer.Name, GroomerPhone = u.Groomer.Phone, InviteDate = u.InviteDate })
                .ToListAsync();

        public async Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept)
        {
            var staff = await _context.GroomerUsers.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == staffId && u.UserId == userId && !u.Deleted);
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.GroomerStaffInviteNotFound);
            if (staff.UserAccept != null)
                return new BaseResultDto(false, Resource.Notification.GroomerStaffInviteAnswered);

            staff.UserAccept = accept;
            if (!accept)
                staff.Active = false;
            await _context.SaveChangesAsync();
            await RefreshStaffCountAsync(staff.GroomerId);
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ ساعت کاری

        public async Task<BaseResultDto<List<GroomerTimeVDto>>> GetTimesAsync(long groomerId)
        {
            if (!await _context.Groomers.AsNoTracking().AnyAsync(g => g.Id == groomerId && !g.Deleted))
                return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<GroomerTimeVDto>>(true, await LoadTimesAsync(groomerId));
        }

        public async Task<BaseResultDto<List<GroomerTimeVDto>>> SetTimesAsync(long groomerId, IReadOnlyList<GroomerTimeInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckGroomerAccessAsync(groomerId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<GroomerTimeVDto>>(false, denied.Messages, null);

                items ??= Array.Empty<GroomerTimeInputDto>();
                if (items.Count > MaxTimeRows)
                    return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.GroomerTimeInvalid, null);

                var parsed = new List<(GroomerTimeInputDto Item, TimeSpan Start, TimeSpan End)>();
                foreach (var item in items)
                {
                    if (!TryParseTime(item.StartTime, out var start) || !TryParseTime(item.EndTime, out var end) || start >= end || item.Capacity < 1 || item.Capacity > 1000)
                        return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.GroomerTimeInvalid, null);
                    parsed.Add((item, start, end));
                }
                // هم‌پوشانی در یک روز هفته (فقط بین ردیف‌های فعال).
                foreach (var day in parsed.Where(p => p.Item.Active).GroupBy(p => p.Item.WeekDayId))
                {
                    var ordered = day.OrderBy(p => p.Start).ToList();
                    for (var i = 1; i < ordered.Count; i++)
                        if (ordered[i].Start < ordered[i - 1].End)
                            return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.GroomerTimeInvalid, null);
                }
                var weekDayIds = parsed.Select(p => p.Item.WeekDayId).Distinct().ToList();
                if (weekDayIds.Count > 0 && await _context.WeekDays.CountAsync(w => weekDayIds.Contains(w.Id)) != weekDayIds.Count)
                    return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.GroomerTimeInvalid, null);

                // رزروهای آینده به ردیف‌های ساعت کاری وابسته می‌شوند؛ بنابراین ردیف قبلی را حذف نرم می‌کنیم و ردیف جدید می‌سازیم.
                await _context.GroomerTimes.Where(t => t.GroomerId == groomerId && !t.Deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.Deleted, true).SetProperty(t => t.Active, false));
                foreach (var (item, _, _) in parsed)
                {
                    await _context.GroomerTimes.AddAsync(new GroomerTime
                    {
                        GroomerId = groomerId,
                        WeekDayId = item.WeekDayId,
                        StartTime = item.StartTime.Trim(),
                        EndTime = item.EndTime.Trim(),
                        Capacity = item.Capacity,
                        Active = item.Active,
                    });
                }
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<GroomerTimeVDto>>(true, await LoadTimesAsync(groomerId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting groomer times for {GroomerId} failed.", groomerId);
                return new BaseResultDto<List<GroomerTimeVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ محدوده‌ی پوشش

        public async Task<BaseResultDto<List<GroomerZoneVDto>>> GetZonesAsync(long groomerId)
        {
            if (!await _context.Groomers.AsNoTracking().AnyAsync(g => g.Id == groomerId && !g.Deleted))
                return new BaseResultDto<List<GroomerZoneVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<GroomerZoneVDto>>(true, await LoadZonesAsync(groomerId));
        }

        public async Task<BaseResultDto<List<GroomerZoneVDto>>> SetZonesAsync(long groomerId, IReadOnlyList<GroomerZoneInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckGroomerAccessAsync(groomerId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<GroomerZoneVDto>>(false, denied.Messages, null);

                var wanted = (items ?? Array.Empty<GroomerZoneInputDto>())
                    .GroupBy(z => (z.StateId, z.CityId, z.NeighborhoodId)).Select(g => g.First()).ToList();
                if (wanted.Count > MaxZones)
                    return new BaseResultDto<List<GroomerZoneVDto>>(false, Resource.Notification.InvalidData, null);

                var cityIds = wanted.Select(z => z.CityId).Distinct().ToList();
                var validCities = await _context.Cities.AsNoTracking().Where(c => cityIds.Contains(c.Id)).Select(c => new { c.Id, c.StateId }).ToListAsync();
                var neighborhoodIds = wanted.Where(z => z.NeighborhoodId.HasValue).Select(z => z.NeighborhoodId.Value).Distinct().ToList();
                var validNeighborhoods = await _context.Neighborhoods.AsNoTracking().Where(n => neighborhoodIds.Contains(n.Id)).Select(n => new { n.Id, n.CityId }).ToListAsync();
                foreach (var zone in wanted)
                {
                    if (!validCities.Any(c => c.Id == zone.CityId && c.StateId == zone.StateId))
                        return new BaseResultDto<List<GroomerZoneVDto>>(false, Resource.Notification.GroomerSelectedCityNotInState, null);
                    if (zone.NeighborhoodId.HasValue && !validNeighborhoods.Any(n => n.Id == zone.NeighborhoodId && n.CityId == zone.CityId))
                        return new BaseResultDto<List<GroomerZoneVDto>>(false, Resource.Notification.GroomerSelectedCityNotInState, null);
                }

                var existing = await _context.GroomerZones.AsTracking().Where(z => z.GroomerId == groomerId).ToListAsync();
                foreach (var row in existing)
                    row.Deleted = !wanted.Any(w => w.StateId == row.StateId && w.CityId == row.CityId && w.NeighborhoodId == row.NeighborhoodId);
                foreach (var zone in wanted.Where(w => !existing.Any(e => e.StateId == w.StateId && e.CityId == w.CityId && e.NeighborhoodId == w.NeighborhoodId)))
                    await _context.GroomerZones.AddAsync(new GroomerZone { GroomerId = groomerId, StateId = zone.StateId, CityId = zone.CityId, NeighborhoodId = zone.NeighborhoodId });
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<GroomerZoneVDto>>(true, await LoadZonesAsync(groomerId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting groomer zones for {GroomerId} failed.", groomerId);
                return new BaseResultDto<List<GroomerZoneVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ گالری

        public async Task<BaseResultDto<List<GroomerPictureVDto>>> GetPicturesAsync(long groomerId)
        {
            if (!await _context.Groomers.AsNoTracking().AnyAsync(g => g.Id == groomerId && !g.Deleted))
                return new BaseResultDto<List<GroomerPictureVDto>>(false, Resource.Notification.NothingFound, null);
            var rows = await _context.GroomerPictures.AsNoTracking().Include(p => p.Picture)
                .Where(p => p.GroomerId == groomerId && !p.Deleted).OrderBy(p => p.Id).ToListAsync();
            return new BaseResultDto<List<GroomerPictureVDto>>(true, rows.Select(ToView).ToList());
        }

        public async Task<BaseResultDto<GroomerPictureVDto>> AddPictureAsync(GroomerPictureAddDto dto, long? ownerUserId)
        {
            var denied = await CheckGroomerAccessAsync(dto?.GroomerId ?? 0, ownerUserId);
            if (denied != null)
                return new BaseResultDto<GroomerPictureVDto>(false, denied.Messages, null);
            if (dto.PictureId <= 0 || !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId))
                return new BaseResultDto<GroomerPictureVDto>(false, Resource.Notification.GroomerPictureInvalid, null);
            if (await _context.GroomerPictures.CountAsync(p => p.GroomerId == dto.GroomerId && !p.Deleted) >= MaxPictures)
                return new BaseResultDto<GroomerPictureVDto>(false, Resource.Notification.InvalidData, null);
            if (await _context.GroomerPictures.AnyAsync(p => p.GroomerId == dto.GroomerId && p.PictureId == dto.PictureId && !p.Deleted))
                return new BaseResultDto<GroomerPictureVDto>(false, Resource.Notification.GroomerPictureInvalid, null);

            var row = new GroomerPicture { GroomerId = dto.GroomerId, PictureId = dto.PictureId, Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim() };
            await _context.GroomerPictures.AddAsync(row);
            await _context.SaveChangesAsync();
            var saved = await _context.GroomerPictures.AsNoTracking().Include(p => p.Picture).FirstAsync(p => p.Id == row.Id);
            return new BaseResultDto<GroomerPictureVDto>(true, ToView(saved));
        }

        public async Task<BaseResultDto> RemovePictureAsync(long groomerPictureId, long? ownerUserId)
        {
            var row = await _context.GroomerPictures.AsNoTracking().Where(p => p.Id == groomerPictureId && !p.Deleted)
                .Select(p => new { p.Id, p.GroomerId }).FirstOrDefaultAsync();
            if (row == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckGroomerAccessAsync(row.GroomerId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerPictures.Where(p => p.Id == groomerPictureId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        /// <summary>null = دسترسی دارد.</summary>
        private async Task<BaseResultDto> CheckGroomerAccessAsync(long groomerId, long? ownerUserId)
        {
            var ownerId = await _context.Groomers.AsNoTracking().Where(g => g.Id == groomerId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task RefreshStaffCountAsync(long groomerId)
        {
            var count = await _context.GroomerUsers.CountAsync(u => u.GroomerId == groomerId && !u.Deleted && u.Active && u.UserAccept == true);
            await _context.Groomers.Where(g => g.Id == groomerId).ExecuteUpdateAsync(s => s.SetProperty(g => g.StaffCount, count));
        }

        private static bool TryParseTime(string value, out TimeSpan time) =>
            TimeSpan.TryParseExact(value?.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out time) && time < TimeSpan.FromHours(24);

        private async Task<List<GroomerTimeVDto>> LoadTimesAsync(long groomerId) =>
            (await _context.GroomerTimes.AsNoTracking()
                .Where(t => t.GroomerId == groomerId && !t.Deleted)
                .Select(t => new GroomerTimeVDto
                {
                    Id = t.Id, WeekDayId = t.WeekDayId, WeekDayName = t.WeekDay.Name,
                    StartTime = t.StartTime, EndTime = t.EndTime, Capacity = t.Capacity, Active = t.Active,
                }).ToListAsync())
            .OrderBy(t => t.WeekDayId).ThenBy(t => t.StartTime, StringComparer.Ordinal).ToList();

        private async Task<List<GroomerZoneVDto>> LoadZonesAsync(long groomerId) =>
            await _context.GroomerZones.AsNoTracking()
                .Where(z => z.GroomerId == groomerId && !z.Deleted)
                .OrderBy(z => z.Id)
                .Select(z => new GroomerZoneVDto
                {
                    Id = z.Id, StateId = z.StateId, CityId = z.CityId, NeighborhoodId = z.NeighborhoodId,
                    StateName = z.State.Name, CityName = z.City.Name, NeighborhoodName = z.Neighborhood.Name,
                }).ToListAsync();

        private GroomerPictureVDto ToView(GroomerPicture row) => new()
        {
            Id = row.Id,
            GroomerId = row.GroomerId,
            PictureId = row.PictureId,
            Label = row.Label,
            Picture = row.Picture == null ? null : _mapper.Map<PictureVDto>(row.Picture),
        };
    }
}
