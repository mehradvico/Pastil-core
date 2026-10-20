using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachCatalogSrv.Iface;
using AutoMapper;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachCatalogSrv
{
    public class CoachCatalogService : ICoachCatalogService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CoachCatalogService> _logger;

        public CoachCatalogService(IDataBaseContext context, IMapper mapper, ILogger<CoachCatalogService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ------------------------------------------------------------------ لیست

        public async Task<BaseResultDto<List<CoachServiceVDto>>> ListManageAsync(long coachId, long? ownerUserId)
        {
            var ownerId = await OwnerOfCoachAsync(coachId);
            if (ownerId == null)
                return new BaseResultDto<List<CoachServiceVDto>>(false, Resource.Notification.NothingFound, null);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto<List<CoachServiceVDto>>(false, Resource.Notification.AccessDenied, null);

            var query = _context.CoachServices.AsNoTracking().Where(s => s.CoachId == coachId && !s.Deleted);
            return new BaseResultDto<List<CoachServiceVDto>>(true, await ProjectAsync(query, petSize: null, onlyActivePackages: false));
        }

        public async Task<BaseResultDto<List<CoachServiceVDto>>> ListPublicAsync(long coachId, string petSize)
        {
            var size = NormalizePetSize(petSize);
            var query = _context.CoachServices.AsNoTracking()
                .Where(s => s.CoachId == coachId && !s.Deleted && s.Active && s.Approved
                            && s.Coach.Active && s.Coach.Approved && !s.Coach.Deleted);
            var list = await ProjectAsync(query, size, onlyActivePackages: true);
            // خدمتی که برای این سایز هیچ بسته‌ی قابل‌رزرو ندارد نمایش داده نمی‌شود.
            return new BaseResultDto<List<CoachServiceVDto>>(true, list.Where(s => s.Packages.Count > 0).ToList());
        }

        // ------------------------------------------------------------------ خدمت

        public async Task<BaseResultDto<CoachServiceVDto>> AddServiceAsync(CoachServiceInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.CoachId <= 0 || dto.AssistanceId <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.InvalidData, null);

                var ownerId = await OwnerOfCoachAsync(dto.CoachId);
                if (ownerId == null)
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.NothingFound, null);
                if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.AccessDenied, null);

                var assistance = await _context.Assistances.AsNoTracking().Where(a => a.Id == dto.AssistanceId && a.Active && !a.Deleted)
                    .Select(a => new { a.IsGrooming }).FirstOrDefaultAsync();
                if (assistance == null)
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.CoachServiceAssistanceInvalid, null);
                // خدمات گرومینگ فقط برای آرایشگاه است.
                if (assistance.IsGrooming)
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.CoachServiceIsGrooming, null);
                if (await _context.CoachServices.AnyAsync(s => s.CoachId == dto.CoachId && s.AssistanceId == dto.AssistanceId && !s.Deleted))
                    return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.CoachServiceAlreadyExists, null);

                var service = new CoachService
                {
                    CoachId = dto.CoachId,
                    AssistanceId = dto.AssistanceId,
                    DurationMinutes = dto.DurationMinutes,
                    Description = dto.Description?.Trim(),
                    Active = true,
                    // خدمتِ تعریف‌شده توسط مالک تا تأیید ادمین قابل رزرو نیست؛ ادمین مستقیم تأییدشده می‌سازد.
                    Approved = !ownerUserId.HasValue,
                };
                await _context.CoachServices.AddAsync(service);
                await _context.SaveChangesAsync();

                var created = await ProjectAsync(_context.CoachServices.AsNoTracking().Where(s => s.Id == service.Id), null, false);
                return new BaseResultDto<CoachServiceVDto>(true, created.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding coach service failed.");
                return new BaseResultDto<CoachServiceVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdateServiceAsync(CoachServiceInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var denied = await CheckServiceAccessAsync(dto.Id, ownerUserId);
            if (denied != null)
                return denied;

            var description = dto.Description?.Trim();
            var minutes = dto.DurationMinutes;
            await _context.CoachServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DurationMinutes, minutes).SetProperty(x => x.Description, description));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ApproveServiceAsync(CoachServiceApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
            if (dto.CommissionPercent.HasValue && (dto.CommissionPercent < 0 || dto.CommissionPercent > 100))
                return new BaseResultDto(false, Resource.Notification.CoachValueOutOfRange);

            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.CoachServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Approved, approve).SetProperty(x => x.ActivationValue, reason));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (dto.CommissionPercent.HasValue)
            {
                var percent = dto.CommissionPercent.Value;
                await _context.CoachServices.Where(s => s.Id == dto.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.CommissionPercent, percent));
            }
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Deleted, true).SetProperty(x => x.Active, false));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId)
        {
            var service = await _context.CoachServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted)
                .Select(s => new { s.CoachId, OwnerId = s.Coach.OwnerId })
                .FirstOrDefaultAsync();
            if (service == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && service.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            var wanted = (userIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            // فقط مالک و پرسنل فعالِ همین مربی می‌توانند گرومرِ یک خدمت باشند.
            var allowed = await _context.CoachUsers.AsNoTracking()
                .Where(u => u.CoachId == service.CoachId && !u.Deleted && u.Active && u.UserAccept == true)
                .Select(u => u.UserId).ToListAsync();
            allowed.Add(service.OwnerId);
            if (wanted.Any(id => !allowed.Contains(id)))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.CoachServiceUsers.AsTracking().Where(u => u.CoachServiceId == serviceId).ToListAsync();
            foreach (var row in existing)
            {
                var keep = wanted.Contains(row.UserId);
                row.Deleted = !keep;
                row.Active = keep;
            }
            foreach (var userId in wanted.Where(id => existing.All(e => e.UserId != id)))
                await _context.CoachServiceUsers.AddAsync(new CoachServiceUser { CoachServiceId = serviceId, UserId = userId, Active = true });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ بسته‌ی قیمتی

        public async Task<BaseResultDto<CoachServicePackageVDto>> AddPackageAsync(CoachServicePackageInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.CoachServiceId <= 0)
                    return new BaseResultDto<CoachServicePackageVDto>(false, Resource.Notification.InvalidData, null);
                var denied = await CheckServiceAccessAsync(dto.CoachServiceId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<CoachServicePackageVDto>(false, denied.Messages, null);
                var error = await ValidatePackageAsync(dto);
                if (error != null)
                    return new BaseResultDto<CoachServicePackageVDto>(false, error, null);

                var types = NormalizeTypes(dto);
                var (basePrice, basePre) = BasePrice(dto, types);
                var package = new CoachServicePackage
                {
                    CoachServiceId = dto.CoachServiceId,
                    Name = dto.Name.Trim(),
                    Price = basePrice,
                    PrePaymentPrice = basePre,
                    Types = types.Select(x => new CoachServicePackageType { Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice }).ToList(),
                    PictureId = dto.PictureId,
                    Discription = dto.Discription?.Trim(),
                    PetSize = NormalizePetSize(dto.PetSize),
                    Active = true,
                };
                await _context.CoachServicePackages.AddAsync(package);
                await _context.SaveChangesAsync();
                var created = _mapper.Map<CoachServicePackageVDto>(package);
                created.Types = types.Select(x => new CoachPackageTypeVDto { Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice, ModeName = ModeName(x.Mode) }).ToList();
                return new BaseResultDto<CoachServicePackageVDto>(true, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding coach package failed.");
                return new BaseResultDto<CoachServicePackageVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdatePackageAsync(CoachServicePackageInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var serviceId = await _context.CoachServicePackages.AsNoTracking()
                .Where(p => p.Id == dto.Id && !p.Deleted).Select(p => (long?)p.CoachServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;

            dto.CoachServiceId = serviceId.Value;
            var error = await ValidatePackageAsync(dto);
            if (error != null)
                return new BaseResultDto(false, error);

            var name = dto.Name.Trim();
            var description = dto.Discription?.Trim();
            var size = NormalizePetSize(dto.PetSize);
            var types = NormalizeTypes(dto);
            var (basePrice, basePre) = BasePrice(dto, types);
            await _context.CoachServicePackages.Where(p => p.Id == dto.Id && !p.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Name, name)
                    .SetProperty(p => p.Price, basePrice)
                    .SetProperty(p => p.PrePaymentPrice, basePre)
                    .SetProperty(p => p.PictureId, dto.PictureId)
                    .SetProperty(p => p.Discription, description)
                    .SetProperty(p => p.PetSize, size));
            await ReplaceTypesAsync(dto.Id, types);
            return new BaseResultDto(true);
        }

        /// <summary>ردیف‌های قیمت به‌ازای نحوه‌ی ارائه را جایگزین می‌کند (ردیف قدیمی حذف نرم).</summary>
        private async Task ReplaceTypesAsync(long packageId, List<CoachPackageTypeDto> types)
        {
            var existing = await _context.CoachServicePackageTypes.AsTracking().Where(x => x.CoachServicePackageId == packageId && !x.Deleted).ToListAsync();
            foreach (var row in existing)
            {
                var wanted = types.FirstOrDefault(x => x.Mode == row.Mode);
                if (wanted == null) { row.Deleted = true; continue; }
                row.Price = wanted.Price;
                row.PrePaymentPrice = wanted.PrePaymentPrice;
            }
            foreach (var item in types.Where(x => existing.All(e => e.Mode != x.Mode)))
                await _context.CoachServicePackageTypes.AddAsync(new CoachServicePackageType { CoachServicePackageId = packageId, Mode = item.Mode, Price = item.Price, PrePaymentPrice = item.PrePaymentPrice });
            await _context.SaveChangesAsync();
        }

        public async Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId)
        {
            var serviceId = await _context.CoachServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.CoachServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId)
        {
            var serviceId = await _context.CoachServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.CoachServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.CoachServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true).SetProperty(p => p.Active, false));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        private async Task<long?> OwnerOfCoachAsync(long coachId) =>
            await _context.Coaches.AsNoTracking().Where(g => g.Id == coachId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();

        /// <summary>null = دسترسی دارد؛ در غیر این صورت نتیجه‌ی خطا.</summary>
        private async Task<BaseResultDto> CheckServiceAccessAsync(long serviceId, long? ownerUserId)
        {
            var ownerId = await _context.CoachServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted && !s.Coach.Deleted)
                .Select(s => (long?)s.Coach.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task<string> ValidatePackageAsync(CoachServicePackageInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 200)
                return Resource.Notification.PleaseEnterTheName;
            var types = dto.Types ?? new List<CoachPackageTypeDto>();
            if (types.Count == 0)
            {
                if (dto.Price <= 0 || dto.PrePaymentPrice < 0 || dto.PrePaymentPrice > dto.Price || double.IsNaN(dto.Price) || double.IsInfinity(dto.Price))
                    return Resource.Notification.CoachPackagePriceInvalid;
            }
            else
            {
                if (types.Select(x => x.Mode).Distinct().Count() != types.Count || types.Any(x => !Enum.IsDefined(typeof(CoachServiceModeEnum), x.Mode)))
                    return Resource.Notification.CoachPackageModeInvalid;
                if (types.Any(x => x.Price <= 0 || x.PrePaymentPrice < 0 || x.PrePaymentPrice > x.Price || double.IsNaN(x.Price) || double.IsInfinity(x.Price)))
                    return Resource.Notification.CoachPackagePriceInvalid;
            }
            if (!string.IsNullOrWhiteSpace(dto.PetSize) && NormalizePetSize(dto.PetSize) == null)
                return Resource.Notification.CoachPackagePetSizeInvalid;
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId.Value))
                return Resource.Notification.CoachPictureInvalid;
            return null;
        }

        private static List<CoachPackageTypeDto> NormalizeTypes(CoachServicePackageInputDto dto) =>
            (dto.Types ?? new List<CoachPackageTypeDto>()).GroupBy(x => x.Mode).Select(g => g.First()).ToList();

        /// <summary>اگر نحوه‌ی ارائه داده شده باشد قیمت پایه‌ی بسته = ارزان‌ترین حالت (برای «قیمت از ...» و مرتب‌سازی).</summary>
        private static (double Price, double Pre) BasePrice(CoachServicePackageInputDto dto, List<CoachPackageTypeDto> types)
        {
            if (types.Count == 0)
                return (dto.Price, dto.PrePaymentPrice);
            var cheapest = types.OrderBy(x => x.Price).First();
            return (cheapest.Price, cheapest.PrePaymentPrice);
        }

        private static string ModeName(int mode) => mode switch
        {
            (int)CoachServiceModeEnum.Online => "online",
            (int)CoachServiceModeEnum.InPerson => "in-person",
            (int)CoachServiceModeEnum.InPlace => "in-place",
            _ => null,
        };

        /// <summary>اندازه‌ی نرمال‌شده؛ خالی یا نامعتبر = null (برای ورودی نامعتبر، ValidatePackageAsync قبلاً خطا می‌دهد).</summary>
        private static string NormalizePetSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return CoachPetSizes.All.FirstOrDefault(s => string.Equals(s, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<CoachServiceVDto>> ProjectAsync(IQueryable<CoachService> query, string petSize, bool onlyActivePackages)
        {
            var rows = await query.OrderBy(s => s.Id).Select(s => new
            {
                s.Id, s.CoachId, s.AssistanceId, AssistanceName = s.Assistance.Name, AssistancePicture = s.Assistance.Picture,
                s.Active, s.Approved, s.ActivationValue, s.CommissionPercent, s.DurationMinutes, s.Description,
                Packages = s.Packages
                    .Where(p => !p.Deleted && (!onlyActivePackages || p.Active) && (petSize == null || p.PetSize == null || p.PetSize == petSize))
                    .OrderBy(p => p.Price)
                    .Select(p => new { p.Id, p.CoachServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Picture, p.Active, p.ActivationValue, p.Discription, p.PetSize, Types = p.Types.Where(x => !x.Deleted).OrderBy(x => x.Mode).Select(x => new { x.Id, x.Mode, x.Price, x.PrePaymentPrice }).ToList() })
                    .ToList(),
                UserIds = s.ServiceUsers.Where(u => !u.Deleted && u.Active).Select(u => u.UserId).ToList(),
            }).AsSplitQuery().ToListAsync();

            return rows.Select(r => new CoachServiceVDto
            {
                Id = r.Id,
                CoachId = r.CoachId,
                AssistanceId = r.AssistanceId,
                AssistanceName = r.AssistanceName,
                AssistancePicture = r.AssistancePicture == null ? null : _mapper.Map<PictureVDto>(r.AssistancePicture),
                Active = r.Active,
                Approved = r.Approved,
                ActivationValue = r.ActivationValue,
                CommissionPercent = r.CommissionPercent,
                DurationMinutes = r.DurationMinutes,
                Description = r.Description,
                UserIds = r.UserIds,
                Packages = r.Packages.Select(p => new CoachServicePackageVDto
                {
                    Id = p.Id,
                    CoachServiceId = p.CoachServiceId,
                    Name = p.Name,
                    Price = p.Price,
                    PrePaymentPrice = p.PrePaymentPrice,
                    PictureId = p.PictureId,
                    Picture = p.Picture == null ? null : _mapper.Map<PictureVDto>(p.Picture),
                    Active = p.Active,
                    ActivationValue = p.ActivationValue,
                    Discription = p.Discription,
                    PetSize = p.PetSize,
                    Types = p.Types.Select(x => new CoachPackageTypeVDto { Id = x.Id, Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice, ModeName = ModeName(x.Mode) }).ToList(),
                }).ToList(),
            }).ToList();
        }
    }
}
