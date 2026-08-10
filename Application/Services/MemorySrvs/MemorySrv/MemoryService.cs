using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.MemorySrvs.MemorySrv.Dto;
using Application.Services.MemorySrvs.MemorySrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.MemorySrvs.MemorySrv
{
    public class MemoryService : IMemoryService
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        private readonly IDataBaseContext _context;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;

        public MemoryService(
            IDataBaseContext context,
            IPushNotificationService pushNotificationService,
            IClubPointIntegrationService clubPointIntegrationService)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
            _clubPointIntegrationService = clubPointIntegrationService;
        }

        public async Task<BaseResultDto<MemoryVDto>> FindAsync(
            long id,
            long? userId,
            CancellationToken cancellationToken = default)
        {
            var query = BaseQuery().Where(item => item.MemoryId == id);
            if (userId.HasValue)
                query = query.Where(item => item.UserId == userId.Value);

            var item = await Project(query).FirstOrDefaultAsync(cancellationToken);
            return item == null
                ? new BaseResultDto<MemoryVDto>(false, "خاطره موردنظر پیدا نشد.", null)
                : new BaseResultDto<MemoryVDto>(true, item);
        }

        public async Task<MemorySearchDto> SearchAsync(
            MemoryInputDto dto,
            long? userId,
            CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);

            var query = BaseQuery();
            if (userId.HasValue)
                query = query.Where(item => item.UserId == userId.Value);
            else if (dto.UserId.HasValue)
                query = query.Where(item => item.UserId == dto.UserId.Value);

            if (dto.UserPetId.HasValue)
                query = query.Where(item => item.UserPetId == dto.UserPetId.Value);

            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item => item.Memory.Text.Contains(text) || item.UserPet.Name.Contains(text));
            }

            if (dto.Date.HasValue)
            {
                var from = TehranStartOfDay(dto.Date.Value);
                var to = TehranStartOfDay(dto.Date.Value.AddDays(1));
                query = query.Where(item => item.Memory.MemoryDate >= from && item.Memory.MemoryDate < to);
            }
            else
            {
                if (dto.FromDate.HasValue)
                {
                    var from = TehranStartOfDay(dto.FromDate.Value);
                    query = query.Where(item => item.Memory.MemoryDate >= from);
                }

                if (dto.ToDate.HasValue)
                {
                    var to = TehranStartOfDay(dto.ToDate.Value.AddDays(1));
                    query = query.Where(item => item.Memory.MemoryDate < to);
                }
            }

            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Memory.MemoryDate).ThenBy(item => item.MemoryId)
                : query.OrderByDescending(item => item.Memory.MemoryDate).ThenByDescending(item => item.MemoryId);

            var totalCount = await query.CountAsync(cancellationToken);
            var list = await Project(query)
                .Skip((dto.PageIndex - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync(cancellationToken);

            return new MemorySearchDto
            {
                PageIndex = dto.PageIndex,
                PageSize = dto.PageSize,
                Q = dto.Q,
                SortBy = dto.SortBy,
                TotalCount = totalCount,
                List = list,
                UserId = userId ?? dto.UserId,
                UserPetId = dto.UserPetId,
                Date = dto.Date,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            };
        }

        public async Task<BaseResultDto<MemoryVDto>> InsertAsync(
            long userId,
            MemoryDto dto,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateAsync(userId, dto, cancellationToken);
            if (validation != null)
                return validation;

            var now = DateTime.Now;
            var memory = new Memory
            {
                Text = dto.Text.Trim(),
                MemoryDate = dto.MemoryDate,
                PictureId = dto.PictureId,
                CreateDate = now,
                Deleted = false
            };

            var userMemory = new UserMemory
            {
                UserId = userId,
                UserPetId = dto.UserPetId,
                Memory = memory,
                CreateDate = now,
                Deleted = false
            };

            await _context.UserMemories.AddAsync(userMemory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _clubPointIntegrationService.MemoryCreatedAsync(
                userId,
                memory.Id,
                memory.MemoryDate,
                cancellationToken);

            return await FindAsync(memory.Id, userId, cancellationToken);
        }

        public async Task<BaseResultDto<MemoryVDto>> UpdateAsync(
            long userId,
            MemoryDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Id <= 0)
                return new BaseResultDto<MemoryVDto>(false, "شناسه خاطره معتبر نیست.", null);

            var validation = await ValidateAsync(userId, dto, cancellationToken);
            if (validation != null)
                return validation;

            var userMemory = await _context.UserMemories
                .Include(item => item.Memory)
                .FirstOrDefaultAsync(item =>
                    item.MemoryId == dto.Id &&
                    item.UserId == userId &&
                    !item.Deleted &&
                    !item.Memory.Deleted,
                    cancellationToken);

            if (userMemory == null)
                return new BaseResultDto<MemoryVDto>(false, "خاطره موردنظر پیدا نشد.", null);

            var previousMemoryDate = userMemory.Memory.MemoryDate;
            userMemory.UserPetId = dto.UserPetId;
            userMemory.Memory.Text = dto.Text.Trim();
            userMemory.Memory.MemoryDate = dto.MemoryDate;
            userMemory.Memory.PictureId = dto.PictureId;
            userMemory.Memory.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);

            if (!IsSameTehranDay(previousMemoryDate, dto.MemoryDate))
            {
                await _clubPointIntegrationService.MemoryReversedAsync(
                    userId,
                    userMemory.MemoryId,
                    previousMemoryDate,
                    cancellationToken);
            }

            await _clubPointIntegrationService.MemoryCreatedAsync(
                userId,
                userMemory.MemoryId,
                dto.MemoryDate,
                cancellationToken);

            return await FindAsync(dto.Id, userId, cancellationToken);
        }

        public async Task<BaseResultDto> DeleteAsync(
            long userId,
            long id,
            CancellationToken cancellationToken = default)
        {
            var userMemory = await _context.UserMemories
                .Include(item => item.Memory)
                .FirstOrDefaultAsync(item =>
                    item.MemoryId == id &&
                    item.UserId == userId &&
                    !item.Deleted &&
                    !item.Memory.Deleted,
                    cancellationToken);

            if (userMemory == null)
                return new BaseResultDto(false, "خاطره موردنظر پیدا نشد.");

            userMemory.Deleted = true;
            userMemory.Memory.Deleted = true;
            userMemory.Memory.UpdateDate = DateTime.Now;
            await _context.SaveChangesAsync(cancellationToken);

            await _clubPointIntegrationService.MemoryReversedAsync(
                userId,
                userMemory.MemoryId,
                userMemory.Memory.MemoryDate,
                cancellationToken);

            return new BaseResultDto(true);
        }

        public async Task SendDailyReminderAsync(CancellationToken cancellationToken = default)
        {
            var tehranToday = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TehranTimeZone).Date;
            var today = TehranStartOfDay(tehranToday);
            var tomorrow = TehranStartOfDay(tehranToday.AddDays(1));
            var notificationToday = DateTime.Today;
            var notificationTomorrow = notificationToday.AddDays(1);
            var pushTypeId = (long)PushTypeEnum.PushMemoryReminder;

            var alreadyNotifiedUserIds = _context.PushNotifications
                .AsNoTracking()
                .Where(item =>
                    item.PushPattern.PushTypeId == pushTypeId &&
                    item.CreateDate >= notificationToday &&
                    item.CreateDate < notificationTomorrow)
                .Select(item => item.UserId);

            var usersWithMemoryToday = _context.UserMemories
                .AsNoTracking()
                .Where(item =>
                    !item.Deleted &&
                    !item.Memory.Deleted &&
                    item.Memory.MemoryDate >= today &&
                    item.Memory.MemoryDate < tomorrow)
                .Select(item => item.UserId);

            var users = await _context.UserPets
                .AsNoTracking()
                .Where(item =>
                    item.Active &&
                    !item.Deleted &&
                    !item.User.Deleted &&
                    !item.User.Locked &&
                    !alreadyNotifiedUserIds.Contains(item.UserId) &&
                    !usersWithMemoryToday.Contains(item.UserId))
                .Select(item => new
                {
                    item.UserId,
                    item.User.FirstName
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fa-IR");
                CultureInfo.CurrentUICulture = new CultureInfo("fa-IR");

                foreach (var user in users)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushMemoryReminder,
                        user.UserId,
                        user.FirstName);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        private async Task<BaseResultDto<MemoryVDto>> ValidateAsync(
            long userId,
            MemoryDto dto,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return new BaseResultDto<MemoryVDto>(false, "متن خاطره الزامی است.", null);

            if (dto.Text.Trim().Length > 4000)
                return new BaseResultDto<MemoryVDto>(false, "متن خاطره حداکثر می‌تواند ۴۰۰۰ کاراکتر باشد.", null);

            if (dto.MemoryDate == default)
                return new BaseResultDto<MemoryVDto>(false, "تاریخ و ساعت خاطره الزامی است.", null);

            if (dto.MemoryDate > DateTimeOffset.UtcNow.AddMinutes(5))
                return new BaseResultDto<MemoryVDto>(false, "تاریخ خاطره نمی‌تواند مربوط به آینده باشد.", null);

            var ownsPet = await _context.UserPets.AnyAsync(item =>
                item.Id == dto.UserPetId &&
                item.UserId == userId &&
                item.Active &&
                !item.Deleted,
                cancellationToken);

            if (!ownsPet)
                return new BaseResultDto<MemoryVDto>(false, "پت انتخاب‌شده متعلق به این کاربر نیست.", null);

            if (dto.PictureId.HasValue &&
                !await _context.Pictures.AnyAsync(item => item.Id == dto.PictureId.Value, cancellationToken))
            {
                return new BaseResultDto<MemoryVDto>(false, "تصویر انتخاب‌شده پیدا نشد.", null);
            }

            var memoryDate = TimeZoneInfo.ConvertTime(dto.MemoryDate, TehranTimeZone).Date;
            var from = TehranStartOfDay(memoryDate);
            var to = TehranStartOfDay(memoryDate.AddDays(1));
            var duplicateDay = await _context.UserMemories.AnyAsync(item =>
                item.UserId == userId &&
                item.MemoryId != dto.Id &&
                !item.Deleted &&
                !item.Memory.Deleted &&
                item.Memory.MemoryDate >= from &&
                item.Memory.MemoryDate < to,
                cancellationToken);
            if (duplicateDay)
                return new BaseResultDto<MemoryVDto>(false, "برای این روز قبلاً خاطره ثبت شده است.", null);

            return null;
        }

        private IQueryable<UserMemory> BaseQuery() =>
            _context.UserMemories
                .AsNoTracking()
                .Where(item => !item.Deleted && !item.Memory.Deleted);

        private static DateTimeOffset TehranStartOfDay(DateTime value)
        {
            var date = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
            return new DateTimeOffset(date, TehranTimeZone.GetUtcOffset(date));
        }

        private static bool IsSameTehranDay(DateTimeOffset first, DateTimeOffset second)
        {
            return TimeZoneInfo.ConvertTime(first, TehranTimeZone).Date ==
                   TimeZoneInfo.ConvertTime(second, TehranTimeZone).Date;
        }

        private static IQueryable<MemoryVDto> Project(IQueryable<UserMemory> query) =>
            query.Select(item => new MemoryVDto
            {
                Id = item.MemoryId,
                Text = item.Memory.Text,
                MemoryDate = item.Memory.MemoryDate,
                CreateDate = item.Memory.CreateDate,
                UpdateDate = item.Memory.UpdateDate,
                PictureId = item.Memory.PictureId,
                Picture = item.Memory.Picture == null
                    ? null
                    : new PictureVDto
                    {
                        Id = item.Memory.Picture.Id,
                        Url = item.Memory.Picture.Url + "/" + item.Memory.Picture.Name,
                        BaseUrl = item.Memory.Picture.Url,
                        GuidName = item.Memory.Picture.GuidName,
                        Extension = item.Memory.Picture.Extension,
                        OrginalName = item.Memory.Picture.OrginalName
                    },
                UserPetId = item.UserPetId,
                UserPetName = item.UserPet.Name,
                UserPetPicture = item.UserPet.Picture == null
                    ? null
                    : new PictureVDto
                    {
                        Id = item.UserPet.Picture.Id,
                        Url = item.UserPet.Picture.Url + "/" + item.UserPet.Picture.Name,
                        BaseUrl = item.UserPet.Picture.Url,
                        GuidName = item.UserPet.Picture.GuidName,
                        Extension = item.UserPet.Picture.Extension,
                        OrginalName = item.UserPet.Picture.OrginalName
                    },
                UserId = item.UserId,
                UserFullName = (item.User.FirstName + " " + item.User.LastName).Trim(),
                UserMobile = item.User.Mobile
            });
    }
}
