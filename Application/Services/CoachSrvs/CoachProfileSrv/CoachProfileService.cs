using Application.Common.Dto.Result;
using Application.Services.Accounting.PetMicrochipSrv;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using AutoMapper;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachProfileSrv
{
    public class CoachProfileService : ICoachProfileService
    {
        private const int MaxTimeRows = 100;
        private const int MaxZones = 500;
        private const int MaxPictures = 30;

        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CoachProfileService> _logger;
        private readonly ICoachNotifier _notifier;

        public CoachProfileService(IDataBaseContext context, IMapper mapper, ILogger<CoachProfileService> logger, ICoachNotifier notifier = null)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notifier = notifier;
        }

        // ------------------------------------------------------------------ پرسنل

        public async Task<BaseResultDto<List<CoachStaffVDto>>> ListStaffAsync(long coachId, long? ownerUserId)
        {
            var denied = await CheckCoachAccessAsync(coachId, ownerUserId);
            if (denied != null)
                return new BaseResultDto<List<CoachStaffVDto>>(false, denied.Messages, null);

            var list = await _context.CoachUsers.AsNoTracking()
                .Where(u => u.CoachId == coachId && !u.Deleted)
                .OrderBy(u => u.Id)
                .Select(u => new CoachStaffVDto
                {
                    Id = u.Id, CoachId = u.CoachId, UserId = u.UserId,
                    FirstName = u.User.FirstName, LastName = u.User.LastName, Mobile = u.User.Mobile,
                    Active = u.Active, ActivationValue = u.ActivationValue, UserAccept = u.UserAccept, InviteDate = u.InviteDate,
                    ExpertiseId = u.ExpertiseId,
                    Expertises = u.Expertises.Where(e => e.Expertise.Active && !e.Expertise.Deleted).OrderBy(e => e.Id)
                        .Select(e => new Application.Services.CompanionSrvs.ExpertiseSrv.Dto.ExpertiseVDto { Id = e.ExpertiseId, Name = e.Expertise.Name, Active = true })
                        .ToList(),
                }).ToListAsync();
            list.ForEach(x => x.ExpertiseName = Application.Services.CompanionSrvs.ExpertiseSrv.StaffExpertiseRules.JoinNames(x.Expertises.Select(e => e.Name)));
            return new BaseResultDto<List<CoachStaffVDto>>(true, list);
        }

        public async Task<BaseResultDto<CoachStaffVDto>> AddStaffAsync(CoachStaffAddDto dto, long? ownerUserId)
        {
            try
            {
                var denied = await CheckCoachAccessAsync(dto?.CoachId ?? 0, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<CoachStaffVDto>(false, denied.Messages, null);

                var mobile = PetMicrochipRules.NormalizeMobile(dto.Mobile);
                if (mobile == null)
                    return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.InvalidData, null);
                var user = await _context.Users.AsNoTracking()
                    .Where(u => !u.Deleted && !u.Locked && u.Mobile == mobile)
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Mobile })
                    .FirstOrDefaultAsync();
                if (user == null)
                    return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.NothingFound, null);

                var ownerId = await _context.Coaches.AsNoTracking().Where(g => g.Id == dto.CoachId).Select(g => g.OwnerId).FirstAsync();
                if (user.Id == ownerId)
                    return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.CoachUserAlreadyExists, null);

                // دعوت: پرسنل تا پذیرفتنِ خودِ کاربر کار نمی‌کند. دعوتِ ردشده را می‌شود دوباره فرستاد؛ دعوت منتظر یا عضو فعلی تکراری است.
                var now = DateTime.Now;
                var existing = await _context.CoachUsers.AsTracking()
                    .FirstOrDefaultAsync(u => u.CoachId == dto.CoachId && u.UserId == user.Id && !u.Deleted);
                if (existing != null && existing.UserAccept != false)
                    return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.CoachUserAlreadyExists, null);

                var staff = existing ?? new CoachUser { CoachId = dto.CoachId, UserId = user.Id };
                staff.Active = true;
                staff.UserAccept = null;
                staff.InviteDate = now;
                if (existing == null)
                    await _context.CoachUsers.AddAsync(staff);
                var addExpertises = await Application.Services.CompanionSrvs.ExpertiseSrv.StaffExpertiseRules.ResolveAsync(_context, dto.ExpertiseIds);
                if (addExpertises == null)
                    return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.StaffExpertiseInvalid, null);
                await _context.SaveChangesAsync();
                if (addExpertises.Count > 0)
                    await ReplaceStaffExpertisesAsync(staff.Id, addExpertises);
                await RefreshStaffCountAsync(dto.CoachId);
                if (_notifier != null)
                    await _notifier.StaffInvitedAsync(dto.CoachId, user.Id);
                return new BaseResultDto<CoachStaffVDto>(true, new CoachStaffVDto
                {
                    Id = staff.Id, CoachId = staff.CoachId, UserId = user.Id,
                    FirstName = user.FirstName, LastName = user.LastName, Mobile = user.Mobile, Active = true,
                    UserAccept = null, InviteDate = now,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding coach staff failed.");
                return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> SetStaffActiveAsync(long staffId, bool active, long? ownerUserId)
        {
            var staff = await _context.CoachUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.CoachId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckCoachAccessAsync(staff.CoachId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachUsers.Where(u => u.Id == staffId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, active));
            await RefreshStaffCountAsync(staff.CoachId);
            return new BaseResultDto(true);
        }

        /// <summary>نقش/تخصص‌های یک عضو تیم را جایگزین می‌کند (فقط مالک/مدیر مرکز). لیست خالی = پاک کردن.</summary>
        public async Task<BaseResultDto<CoachStaffVDto>> SetStaffExpertiseAsync(CoachStaffExpertiseDto dto, long? ownerUserId)
        {
            var staff = await _context.CoachUsers.AsNoTracking().Where(u => u.Id == (dto == null ? 0 : dto.Id) && !u.Deleted)
                .Select(u => new { u.Id, u.CoachId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.NothingFound, null);
            var denied = await CheckCoachAccessAsync(staff.CoachId, ownerUserId);
            if (denied != null)
                return new BaseResultDto<CoachStaffVDto>(false, denied.Messages, null);
            var expertises = await Application.Services.CompanionSrvs.ExpertiseSrv.StaffExpertiseRules.ResolveAsync(_context, dto.ExpertiseIds);
            if (expertises == null)
                return new BaseResultDto<CoachStaffVDto>(false, Resource.Notification.StaffExpertiseInvalid, null);
            await ReplaceStaffExpertisesAsync(staff.Id, expertises);
            var list = await ListStaffAsync(staff.CoachId, ownerUserId);
            return new BaseResultDto<CoachStaffVDto>(true, list.Data?.FirstOrDefault(x => x.Id == staff.Id));
        }

        // جایگزینی کامل ردیف‌های تخصص + تخصص اصلی (اولین مورد)
        private async Task ReplaceStaffExpertisesAsync(long staffId, List<Entities.Entities.CompanionField.Expertise> expertises)
        {
            await _context.CoachUserExpertises.Where(e => e.CoachUserId == staffId).ExecuteDeleteAsync();
            if (expertises.Count > 0)
                await _context.CoachUserExpertises.AddRangeAsync(expertises.Select(e => new Entities.Entities.CoachField.CoachUserExpertise { CoachUserId = staffId, ExpertiseId = e.Id }));
            await _context.SaveChangesAsync();
            long? primary = expertises.Count > 0 ? expertises[0].Id : null;
            await _context.CoachUsers.Where(u => u.Id == staffId).ExecuteUpdateAsync(x => x.SetProperty(u => u.ExpertiseId, primary));
        }

        public async Task<BaseResultDto> RemoveStaffAsync(long staffId, long? ownerUserId)
        {
            var staff = await _context.CoachUsers.AsNoTracking().Where(u => u.Id == staffId && !u.Deleted)
                .Select(u => new { u.Id, u.CoachId, u.UserId }).FirstOrDefaultAsync();
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckCoachAccessAsync(staff.CoachId, ownerUserId);
            if (denied != null)
                return denied;

            await _context.CoachUsers.Where(u => u.Id == staffId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            // گرومر حذف‌شده دیگر نباید روی خدمات مربی بماند.
            await _context.CoachServiceUsers
                .Where(u => u.UserId == staff.UserId && u.CoachService.CoachId == staff.CoachId && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            await RefreshStaffCountAsync(staff.CoachId);
            return new BaseResultDto(true);
        }

        public async Task<List<CoachStaffInviteVDto>> ListInvitesAsync(long userId) =>
            await _context.CoachUsers.AsNoTracking()
                .Where(u => u.UserId == userId && !u.Deleted && u.UserAccept == null && !u.Coach.Deleted)
                .OrderByDescending(u => u.Id)
                .Select(u => new CoachStaffInviteVDto { Id = u.Id, CoachId = u.CoachId, CoachName = u.Coach.Name, CoachPhone = u.Coach.Phone, InviteDate = u.InviteDate })
                .ToListAsync();

        public async Task<BaseResultDto> RespondInviteAsync(long staffId, long userId, bool accept)
        {
            var staff = await _context.CoachUsers.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == staffId && u.UserId == userId && !u.Deleted);
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.CoachStaffInviteNotFound);
            if (staff.UserAccept != null)
                return new BaseResultDto(false, Resource.Notification.CoachStaffInviteAnswered);

            staff.UserAccept = accept;
            if (!accept)
                staff.Active = false;
            await _context.SaveChangesAsync();
            await RefreshStaffCountAsync(staff.CoachId);
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ ساعت کاری

        public async Task<BaseResultDto<List<CoachTimeVDto>>> GetTimesAsync(long coachId)
        {
            if (!await _context.Coaches.AsNoTracking().AnyAsync(g => g.Id == coachId && !g.Deleted))
                return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<CoachTimeVDto>>(true, await LoadTimesAsync(coachId));
        }

        public async Task<BaseResultDto<List<CoachTimeVDto>>> SetTimesAsync(long coachId, IReadOnlyList<CoachTimeInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckCoachAccessAsync(coachId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<CoachTimeVDto>>(false, denied.Messages, null);

                items ??= Array.Empty<CoachTimeInputDto>();
                if (items.Count > MaxTimeRows)
                    return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.CoachTimeInvalid, null);

                var parsed = new List<(CoachTimeInputDto Item, TimeSpan Start, TimeSpan End)>();
                foreach (var item in items)
                {
                    if (!TryParseTime(item.StartTime, out var start) || !TryParseTime(item.EndTime, out var end) || start >= end || item.Capacity < 1 || item.Capacity > 1000)
                        return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.CoachTimeInvalid, null);
                    parsed.Add((item, start, end));
                }
                // هم‌پوشانی در یک روز هفته (فقط بین ردیف‌های فعال).
                foreach (var day in parsed.Where(p => p.Item.Active).GroupBy(p => p.Item.WeekDayId))
                {
                    var ordered = day.OrderBy(p => p.Start).ToList();
                    for (var i = 1; i < ordered.Count; i++)
                        if (ordered[i].Start < ordered[i - 1].End)
                            return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.CoachTimeInvalid, null);
                }
                var weekDayIds = parsed.Select(p => p.Item.WeekDayId).Distinct().ToList();
                if (weekDayIds.Count > 0 && await _context.WeekDays.CountAsync(w => weekDayIds.Contains(w.Id)) != weekDayIds.Count)
                    return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.CoachTimeInvalid, null);

                // رزروهای آینده به ردیف‌های ساعت کاری وابسته می‌شوند؛ بنابراین ردیف قبلی را حذف نرم می‌کنیم و ردیف جدید می‌سازیم.
                await _context.CoachTimes.Where(t => t.CoachId == coachId && !t.Deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.Deleted, true).SetProperty(t => t.Active, false));
                foreach (var (item, _, _) in parsed)
                {
                    await _context.CoachTimes.AddAsync(new CoachTime
                    {
                        CoachId = coachId,
                        WeekDayId = item.WeekDayId,
                        StartTime = item.StartTime.Trim(),
                        EndTime = item.EndTime.Trim(),
                        Capacity = item.Capacity,
                        Active = item.Active,
                    });
                }
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<CoachTimeVDto>>(true, await LoadTimesAsync(coachId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting coach times for {CoachId} failed.", coachId);
                return new BaseResultDto<List<CoachTimeVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ محدوده‌ی پوشش

        public async Task<BaseResultDto<List<CoachZoneVDto>>> GetZonesAsync(long coachId)
        {
            if (!await _context.Coaches.AsNoTracking().AnyAsync(g => g.Id == coachId && !g.Deleted))
                return new BaseResultDto<List<CoachZoneVDto>>(false, Resource.Notification.NothingFound, null);
            return new BaseResultDto<List<CoachZoneVDto>>(true, await LoadZonesAsync(coachId));
        }

        public async Task<BaseResultDto<List<CoachZoneVDto>>> SetZonesAsync(long coachId, IReadOnlyList<CoachZoneInputDto> items, long? ownerUserId)
        {
            try
            {
                var denied = await CheckCoachAccessAsync(coachId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<List<CoachZoneVDto>>(false, denied.Messages, null);

                var wanted = (items ?? Array.Empty<CoachZoneInputDto>())
                    .GroupBy(z => (z.StateId, z.CityId, z.NeighborhoodId)).Select(g => g.First()).ToList();
                if (wanted.Count > MaxZones)
                    return new BaseResultDto<List<CoachZoneVDto>>(false, Resource.Notification.InvalidData, null);

                var cityIds = wanted.Select(z => z.CityId).Distinct().ToList();
                var validCities = await _context.Cities.AsNoTracking().Where(c => cityIds.Contains(c.Id)).Select(c => new { c.Id, c.StateId }).ToListAsync();
                var neighborhoodIds = wanted.Where(z => z.NeighborhoodId.HasValue).Select(z => z.NeighborhoodId.Value).Distinct().ToList();
                var validNeighborhoods = await _context.Neighborhoods.AsNoTracking().Where(n => neighborhoodIds.Contains(n.Id)).Select(n => new { n.Id, n.CityId }).ToListAsync();
                foreach (var zone in wanted)
                {
                    if (!validCities.Any(c => c.Id == zone.CityId && c.StateId == zone.StateId))
                        return new BaseResultDto<List<CoachZoneVDto>>(false, Resource.Notification.CoachSelectedCityNotInState, null);
                    if (zone.NeighborhoodId.HasValue && !validNeighborhoods.Any(n => n.Id == zone.NeighborhoodId && n.CityId == zone.CityId))
                        return new BaseResultDto<List<CoachZoneVDto>>(false, Resource.Notification.CoachSelectedCityNotInState, null);
                }

                var existing = await _context.CoachZones.AsTracking().Where(z => z.CoachId == coachId).ToListAsync();
                foreach (var row in existing)
                    row.Deleted = !wanted.Any(w => w.StateId == row.StateId && w.CityId == row.CityId && w.NeighborhoodId == row.NeighborhoodId);
                foreach (var zone in wanted.Where(w => !existing.Any(e => e.StateId == w.StateId && e.CityId == w.CityId && e.NeighborhoodId == w.NeighborhoodId)))
                    await _context.CoachZones.AddAsync(new CoachZone { CoachId = coachId, StateId = zone.StateId, CityId = zone.CityId, NeighborhoodId = zone.NeighborhoodId });
                await _context.SaveChangesAsync();
                return new BaseResultDto<List<CoachZoneVDto>>(true, await LoadZonesAsync(coachId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setting coach zones for {CoachId} failed.", coachId);
                return new BaseResultDto<List<CoachZoneVDto>>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ------------------------------------------------------------------ گالری

        public async Task<BaseResultDto<List<CoachPictureVDto>>> GetPicturesAsync(long coachId)
        {
            if (!await _context.Coaches.AsNoTracking().AnyAsync(g => g.Id == coachId && !g.Deleted))
                return new BaseResultDto<List<CoachPictureVDto>>(false, Resource.Notification.NothingFound, null);
            var rows = await _context.CoachPictures.AsNoTracking().Include(p => p.Picture)
                .Where(p => p.CoachId == coachId && !p.Deleted).OrderBy(p => p.Id).ToListAsync();
            return new BaseResultDto<List<CoachPictureVDto>>(true, rows.Select(ToView).ToList());
        }

        public async Task<BaseResultDto<CoachPictureVDto>> AddPictureAsync(CoachPictureAddDto dto, long? ownerUserId)
        {
            var denied = await CheckCoachAccessAsync(dto?.CoachId ?? 0, ownerUserId);
            if (denied != null)
                return new BaseResultDto<CoachPictureVDto>(false, denied.Messages, null);
            if (dto.PictureId <= 0 || !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId))
                return new BaseResultDto<CoachPictureVDto>(false, Resource.Notification.CoachPictureInvalid, null);
            if (await _context.CoachPictures.CountAsync(p => p.CoachId == dto.CoachId && !p.Deleted) >= MaxPictures)
                return new BaseResultDto<CoachPictureVDto>(false, Resource.Notification.InvalidData, null);
            if (await _context.CoachPictures.AnyAsync(p => p.CoachId == dto.CoachId && p.PictureId == dto.PictureId && !p.Deleted))
                return new BaseResultDto<CoachPictureVDto>(false, Resource.Notification.CoachPictureInvalid, null);

            var row = new CoachPicture { CoachId = dto.CoachId, PictureId = dto.PictureId, Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim() };
            await _context.CoachPictures.AddAsync(row);
            await _context.SaveChangesAsync();
            var saved = await _context.CoachPictures.AsNoTracking().Include(p => p.Picture).FirstAsync(p => p.Id == row.Id);
            return new BaseResultDto<CoachPictureVDto>(true, ToView(saved));
        }

        public async Task<BaseResultDto> RemovePictureAsync(long coachPictureId, long? ownerUserId)
        {
            var row = await _context.CoachPictures.AsNoTracking().Where(p => p.Id == coachPictureId && !p.Deleted)
                .Select(p => new { p.Id, p.CoachId }).FirstOrDefaultAsync();
            if (row == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckCoachAccessAsync(row.CoachId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachPictures.Where(p => p.Id == coachPictureId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        /// <summary>null = دسترسی دارد.</summary>
        private async Task<BaseResultDto> CheckCoachAccessAsync(long coachId, long? ownerUserId)
        {
            var ownerId = await _context.Coaches.AsNoTracking().Where(g => g.Id == coachId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task RefreshStaffCountAsync(long coachId)
        {
            var count = await _context.CoachUsers.CountAsync(u => u.CoachId == coachId && !u.Deleted && u.Active && u.UserAccept == true);
            await _context.Coaches.Where(g => g.Id == coachId).ExecuteUpdateAsync(s => s.SetProperty(g => g.StaffCount, count));
        }

        private static bool TryParseTime(string value, out TimeSpan time) =>
            TimeSpan.TryParseExact(value?.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out time) && time < TimeSpan.FromHours(24);

        private async Task<List<CoachTimeVDto>> LoadTimesAsync(long coachId) =>
            (await _context.CoachTimes.AsNoTracking()
                .Where(t => t.CoachId == coachId && !t.Deleted)
                .Select(t => new CoachTimeVDto
                {
                    Id = t.Id, WeekDayId = t.WeekDayId, WeekDayName = t.WeekDay.Name,
                    StartTime = t.StartTime, EndTime = t.EndTime, Capacity = t.Capacity, Active = t.Active,
                }).ToListAsync())
            .OrderBy(t => t.WeekDayId).ThenBy(t => t.StartTime, StringComparer.Ordinal).ToList();

        private async Task<List<CoachZoneVDto>> LoadZonesAsync(long coachId) =>
            await _context.CoachZones.AsNoTracking()
                .Where(z => z.CoachId == coachId && !z.Deleted)
                .OrderBy(z => z.Id)
                .Select(z => new CoachZoneVDto
                {
                    Id = z.Id, StateId = z.StateId, CityId = z.CityId, NeighborhoodId = z.NeighborhoodId,
                    StateName = z.State.Name, CityName = z.City.Name, NeighborhoodName = z.Neighborhood.Name,
                }).ToListAsync();

        private CoachPictureVDto ToView(CoachPicture row) => new()
        {
            Id = row.Id,
            CoachId = row.CoachId,
            PictureId = row.PictureId,
            Label = row.Label,
            Picture = row.Picture == null ? null : _mapper.Map<PictureVDto>(row.Picture),
        };
    }
}
