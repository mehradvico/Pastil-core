using Application.Common.Dto.Result;
using Application.Services.Accounting.PetMicrochipSrv;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using AutoMapper;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicProfileSrv
{
    public class ClinicProfileService : IClinicProfileService
    {
        private const int MaxTimeRows = 100;
        private const int MaxZones = 500;
        private const int MaxPictures = 30;

        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ClinicProfileService> _logger;
        private readonly IClinicNotifier _notifier;

        public ClinicProfileService(IDataBaseContext context, IMapper mapper, ILogger<ClinicProfileService> logger, IClinicNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        // ------------------------------------------------------------------ پرسنل

        public async Task<BaseResultDto<List<ClinicStaffVDto>>> ListStaffAsync(long clinicId, long? ownerUserId)
        {
            var denied = await CheckClinicAccessAsync(clinicId, ownerUserId);
            if (denied != null)
                return new BaseResultDto<List<ClinicStaffVDto>>(false, denied.Messages, null);

            var list = await _context.ClinicUsers.AsNoTracking()
                .Where(u => u.ClinicId == clinicId && !u.Deleted)
                .OrderBy(u => u.Id)
                .Select(u => new ClinicStaffVDto
                {
                    Id = u.Id, ClinicId = u.ClinicId, UserId = u.UserId,
                    FirstName = u.User.FirstName, LastName = u.User.LastName, Mobile = u.User.Mobile,
                    Active = u.Active, ActivationValue = u.ActivationValue, UserAccept = u.UserAccept, InviteDate = u.InviteDate,
                }).ToListAsync();
            return new BaseResultDto<List<ClinicStaffVDto>>(true, list);
        }

        public async Task<BaseResultDto<ClinicStaffVDto>> AddStaffAsync(ClinicStaffAddDto dto, long? ownerUserId)
        {
            try
            {
                var denied = await CheckClinicAccessAsync(dto?.ClinicId ?? 0, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<ClinicStaffVDto>(false, denied.Messages, null);

                var mobile = PetMicrochipRules.NormalizeMobile(dto.Mobile);
                if (mobile == null)
                    return new BaseResultDto<ClinicStaffVDto>(false, Resource.Notification.InvalidData, null);
                var user = await _context.Users.AsNoTracking()
                    .Where(u => !u.Deleted && !u.Locked && u.Mobile == mobile)
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Mobile })
                    .FirstOrDefaultAsync();
                if (user == null)
                    return new BaseResultDto<ClinicStaffVDto>(false, Resource.Notification.NothingFound, null);

                var ownerId = await _context.Clinics.AsNoTracking().Where(g => g.Id == dto.ClinicId).Select(g => g.OwnerId).FirstAsync();
                if (user.Id == ownerId)
                    return new BaseResultDto<ClinicStaffVDto>(false, Resource.Notification.ClinicUserAlreadyExists, null);

                // دعوت: پرسنل تا پذیرفتنِ خودِ کاربر کار نمی‌کند. دعوتِ ردشده را می‌شود دوباره فرستاد؛ دعوت منتظر یا عضو فعلی تکراری است.
                var now = DateTime.Now;
                var existing = await _context.ClinicUsers.AsTracking()
                    .FirstOrDefaultAsync(u => u.ClinicId == dto.ClinicId && u.UserId == user.Id && !u.Deleted);
                if (existing != null && existing.UserAccept != false)
                    return new BaseResultDto<ClinicStaffVDto>(false, Resource.Notification.ClinicUserAlreadyExists, null);

                var staff = existing ?? new ClinicUser { ClinicId = dto.ClinicId, UserId = user.Id };
                staff.Active = true;
                staff.UserAccept = null;
                staff.InviteDate = now;
                if (existing == null)
                    await _context.ClinicUsers.AddAsync(staff);
                await _context.SaveChangesAsync();
                await RefreshStaffCountAsync(dto.ClinicId);
                if (_notifier != null)
                    await _notifier.StaffInvitedAsync(dto.ClinicId, user.Id);
                return new BaseResultDto<ClinicStaffVDto>(true, new ClinicStaffVDto
                {
                    Id = staff.Id, ClinicId = staff.ClinicId, UserId = user.Id,
                    FirstName = user.FirstName, LastName = user.LastName, Mobile = user.Mobile, Active = true,
                    UserAccept = null, InviteDate = now,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding clinic staff failed.");
                return new BaseResultDto<ClinicStaffVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId)
        {
            var staff = await _context.ClinicUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.ClinicId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckClinicAccessAsync(staff.ClinicId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicUsers.Where(u => u.Id == staffId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, active));
            await RefreshStaffCountAsync(staff.ClinicId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId)
        {
            var staff = await _context.ClinicUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.ClinicId, u.UserId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckClinicAccessAsync(staff.ClinicId, ownerUserId);
            if (denied != null)
                return denied;

            await _context.ClinicUsers.Where(u => u.Id == staffId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            // گرومر حذف‌شده دیگر نباید روی خدمات کلینیک بماند.
            await _context.ClinicServiceUsers
                .Where(u => u.UserId == staff.UserId && u.ClinicService.ClinicId == staff.ClinicId && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            await RefreshStaffCountAsync(staff.ClinicId);
            return new BaseResultDto(true);
        }

        public async Task<List<ClinicStaffInviteVDto>> ListInvitesAsync(long userId) =>
            await _context.ClinicUsers.AsNoTracking()
                .Where(u => u.UserId == userId && !u.Deleted && u.UserAccept == null && !u.Clinic.Deleted)
                .OrderByDescending(u => u.Id)
                .Select(u => new ClinicStaffInviteVDto { Id = u.Id, ClinicId = u.ClinicId, ClinicName = u.Clinic.Name, ClinicPhone = u.Clinic.Phone, InviteDate = u.InviteDate })
                .ToListAsync();

        public async Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept)
        {
            var staff = await _context.ClinicUsers.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == staffId && u.UserId == userId && !u.Deleted);
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.ClinicStaffInviteNotFound);
            if (staff.UserAccept != null)
                return new BaseResultDto(false, Resource.Notification.ClinicStaffInviteAnswered);

            staff.UserAccept = accept;
            if (!accept)
                staff.Active = false;
            await _context.SaveChangesAsync();
            await RefreshStaffCountAsync(staff.ClinicId);
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ ساعت کاری

        public async Task<BaseResultDto<List<ClinicTimeVDto>>> GetTimesAsync(long clinicId)
        {
            if (!await _context.Clinics.AsNoTracking().AnyAsync(g => g.Id == clinicId && !g.Deleted))
                return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<ClinicTimeVDto>>(true, await LoadTimesAsync(clinicId));
        }

        public async Task<BaseResultDto<List<ClinicTimeVDto>>> SetTimesAsync(long clinicId, IReadOnlyList<ClinicTimeInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckClinicAccessAsync(clinicId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<ClinicTimeVDto>>(false, denied.Messages, null);

                items ??= Array.Empty<ClinicTimeInputDto>();
                if (items.Count > MaxTimeRows)
                    return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.ClinicTimeInvalid, null);

                var parsed = new List<(ClinicTimeInputDto Item, TimeSpan Start, TimeSpan End)>();
                foreach (var item in items)
                {
                    if (!TryParseTime(item.StartTime, out var start) || !TryParseTime(item.EndTime, out var end) || start >= end || item.Capacity < 1 || item.Capacity > 1000)
                        return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.ClinicTimeInvalid, null);
                    parsed.Add((item, start, end));
                }
                // هم‌پوشانی در یک روز هفته (فقط بین ردیف‌های فعال).
                foreach (var day in parsed.Where(p => p.Item.Active).GroupBy(p => p.Item.WeekDayId))
                {
                    var ordered = day.OrderBy(p => p.Start).ToList();
                    for (var i = 1; i < ordered.Count; i++)
                        if (ordered[i].Start < ordered[i - 1].End)
                            return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.ClinicTimeInvalid, null);
                }
                var weekDayIds = parsed.Select(p => p.Item.WeekDayId).Distinct().ToList();
                if (weekDayIds.Count > 0 && await _context.WeekDays.CountAsync(w => weekDayIds.Contains(w.Id)) != weekDayIds.Count)
                    return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.ClinicTimeInvalid, null);

                // رزروهای آینده به ردیف‌های ساعت کاری وابسته می‌شوند؛ بنابراین ردیف قبلی را حذف نرم می‌کنیم و ردیف جدید می‌سازیم.
                await _context.ClinicTimes.Where(t => t.ClinicId == clinicId && !t.Deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.Deleted, true).SetProperty(t => t.Active, false));
                foreach (var (item, _, _) in parsed)
                {
                    await _context.ClinicTimes.AddAsync(new ClinicTime
                    {
                        ClinicId = clinicId,
                        WeekDayId = item.WeekDayId,
                        StartTime = item.StartTime.Trim(),
                        EndTime = item.EndTime.Trim(),
                        Capacity = item.Capacity,
                        Active = item.Active,
                    });
                }
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<ClinicTimeVDto>>(true, await LoadTimesAsync(clinicId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting clinic times for {ClinicId} failed.", clinicId);
                return new BaseResultDto<List<ClinicTimeVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ محدوده‌ی پوشش

        public async Task<BaseResultDto<List<ClinicZoneVDto>>> GetZonesAsync(long clinicId)
        {
            if (!await _context.Clinics.AsNoTracking().AnyAsync(g => g.Id == clinicId && !g.Deleted))
                return new BaseResultDto<List<ClinicZoneVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<ClinicZoneVDto>>(true, await LoadZonesAsync(clinicId));
        }

        public async Task<BaseResultDto<List<ClinicZoneVDto>>> SetZonesAsync(long clinicId, IReadOnlyList<ClinicZoneInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckClinicAccessAsync(clinicId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<ClinicZoneVDto>>(false, denied.Messages, null);

                var wanted = (items ?? Array.Empty<ClinicZoneInputDto>())
                    .GroupBy(z => (z.StateId, z.CityId, z.NeighborhoodId)).Select(g => g.First()).ToList();
                if (wanted.Count > MaxZones)
                    return new BaseResultDto<List<ClinicZoneVDto>>(false, Resource.Notification.InvalidData, null);

                var cityIds = wanted.Select(z => z.CityId).Distinct().ToList();
                var validCities = await _context.Cities.AsNoTracking().Where(c => cityIds.Contains(c.Id)).Select(c => new { c.Id, c.StateId }).ToListAsync();
                var neighborhoodIds = wanted.Where(z => z.NeighborhoodId.HasValue).Select(z => z.NeighborhoodId.Value).Distinct().ToList();
                var validNeighborhoods = await _context.Neighborhoods.AsNoTracking().Where(n => neighborhoodIds.Contains(n.Id)).Select(n => new { n.Id, n.CityId }).ToListAsync();
                foreach (var zone in wanted)
                {
                    if (!validCities.Any(c => c.Id == zone.CityId && c.StateId == zone.StateId))
                        return new BaseResultDto<List<ClinicZoneVDto>>(false, Resource.Notification.ClinicSelectedCityNotInState, null);
                    if (zone.NeighborhoodId.HasValue && !validNeighborhoods.Any(n => n.Id == zone.NeighborhoodId && n.CityId == zone.CityId))
                        return new BaseResultDto<List<ClinicZoneVDto>>(false, Resource.Notification.ClinicSelectedCityNotInState, null);
                }

                var existing = await _context.ClinicZones.AsTracking().Where(z => z.ClinicId == clinicId).ToListAsync();
                foreach (var row in existing)
                    row.Deleted = !wanted.Any(w => w.StateId == row.StateId && w.CityId == row.CityId && w.NeighborhoodId == row.NeighborhoodId);
                foreach (var zone in wanted.Where(w => !existing.Any(e => e.StateId == w.StateId && e.CityId == w.CityId && e.NeighborhoodId == w.NeighborhoodId)))
                    await _context.ClinicZones.AddAsync(new ClinicZone { ClinicId = clinicId, StateId = zone.StateId, CityId = zone.CityId, NeighborhoodId = zone.NeighborhoodId });
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<ClinicZoneVDto>>(true, await LoadZonesAsync(clinicId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting clinic zones for {ClinicId} failed.", clinicId);
                return new BaseResultDto<List<ClinicZoneVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ گالری

        public async Task<BaseResultDto<List<ClinicPictureVDto>>> GetPicturesAsync(long clinicId)
        {
            if (!await _context.Clinics.AsNoTracking().AnyAsync(g => g.Id == clinicId && !g.Deleted))
                return new BaseResultDto<List<ClinicPictureVDto>>(false, Resource.Notification.NothingFound, null);
            var rows = await _context.ClinicPictures.AsNoTracking().Include(p => p.Picture)
                .Where(p => p.ClinicId == clinicId && !p.Deleted).OrderBy(p => p.Id).ToListAsync();
            return new BaseResultDto<List<ClinicPictureVDto>>(true, rows.Select(ToView).ToList());
        }

        public async Task<BaseResultDto<ClinicPictureVDto>> AddPictureAsync(ClinicPictureAddDto dto, long? ownerUserId)
        {
            var denied = await CheckClinicAccessAsync(dto?.ClinicId ?? 0, ownerUserId);
            if (denied != null)
                return new BaseResultDto<ClinicPictureVDto>(false, denied.Messages, null);
            if (dto.PictureId <= 0 || !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId))
                return new BaseResultDto<ClinicPictureVDto>(false, Resource.Notification.ClinicPictureInvalid, null);
            if (await _context.ClinicPictures.CountAsync(p => p.ClinicId == dto.ClinicId && !p.Deleted) >= MaxPictures)
                return new BaseResultDto<ClinicPictureVDto>(false, Resource.Notification.InvalidData, null);
            if (await _context.ClinicPictures.AnyAsync(p => p.ClinicId == dto.ClinicId && p.PictureId == dto.PictureId && !p.Deleted))
                return new BaseResultDto<ClinicPictureVDto>(false, Resource.Notification.ClinicPictureInvalid, null);

            var row = new ClinicPicture { ClinicId = dto.ClinicId, PictureId = dto.PictureId, Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim() };
            await _context.ClinicPictures.AddAsync(row);
            await _context.SaveChangesAsync();
            var saved = await _context.ClinicPictures.AsNoTracking().Include(p => p.Picture).FirstAsync(p => p.Id == row.Id);
            return new BaseResultDto<ClinicPictureVDto>(true, ToView(saved));
        }

        public async Task<BaseResultDto> RemovePictureAsync(long clinicPictureId, long? ownerUserId)
        {
            var row = await _context.ClinicPictures.AsNoTracking().Where(p => p.Id == clinicPictureId && !p.Deleted)
                .Select(p => new { p.Id, p.ClinicId }).FirstOrDefaultAsync();
            if (row == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckClinicAccessAsync(row.ClinicId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicPictures.Where(p => p.Id == clinicPictureId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        /// <summary>null = دسترسی دارد.</summary>
        private async Task<BaseResultDto> CheckClinicAccessAsync(long clinicId, long? ownerUserId)
        {
            var ownerId = await _context.Clinics.AsNoTracking().Where(g => g.Id == clinicId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task RefreshStaffCountAsync(long clinicId)
        {
            var count = await _context.ClinicUsers.CountAsync(u => u.ClinicId == clinicId && !u.Deleted && u.Active && u.UserAccept == true);
            await _context.Clinics.Where(g => g.Id == clinicId).ExecuteUpdateAsync(s => s.SetProperty(g => g.StaffCount, count));
        }

        private static bool TryParseTime(string value, out TimeSpan time) =>
            TimeSpan.TryParseExact(value?.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out time) && time < TimeSpan.FromHours(24);

        private async Task<List<ClinicTimeVDto>> LoadTimesAsync(long clinicId) =>
            (await _context.ClinicTimes.AsNoTracking()
                .Where(t => t.ClinicId == clinicId && !t.Deleted)
                .Select(t => new ClinicTimeVDto
                {
                    Id = t.Id, WeekDayId = t.WeekDayId, WeekDayName = t.WeekDay.Name,
                    StartTime = t.StartTime, EndTime = t.EndTime, Capacity = t.Capacity, Active = t.Active,
                }).ToListAsync())
            .OrderBy(t => t.WeekDayId).ThenBy(t => t.StartTime, StringComparer.Ordinal).ToList();

        private async Task<List<ClinicZoneVDto>> LoadZonesAsync(long clinicId) =>
            await _context.ClinicZones.AsNoTracking()
                .Where(z => z.ClinicId == clinicId && !z.Deleted)
                .OrderBy(z => z.Id)
                .Select(z => new ClinicZoneVDto
                {
                    Id = z.Id, StateId = z.StateId, CityId = z.CityId, NeighborhoodId = z.NeighborhoodId,
                    StateName = z.State.Name, CityName = z.City.Name, NeighborhoodName = z.Neighborhood.Name,
                }).ToListAsync();

        private ClinicPictureVDto ToView(ClinicPicture row) => new()
        {
            Id = row.Id,
            ClinicId = row.ClinicId,
            PictureId = row.PictureId,
            Label = row.Label,
            Picture = row.Picture == null ? null : _mapper.Map<PictureVDto>(row.Picture),
        };
    }
}
