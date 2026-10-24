using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Iface;
using AutoMapper;
using Entities.Entities.ClinicField;
using Entities.Entities.ProviderField;
using Application.Services.CompanionSrvs.AssistanceSrv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicCatalogSrv
{
    public class ClinicCatalogService : IClinicCatalogService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ClinicCatalogService> _logger;

        public ClinicCatalogService(IDataBaseContext context, IMapper mapper, ILogger<ClinicCatalogService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ------------------------------------------------------------------ لیست

        public async Task<BaseResultDto<List<ClinicServiceVDto>>> ListManageAsync(long clinicId, long? ownerUserId)
        {
            var ownerId = await OwnerOfClinicAsync(clinicId);
            if (ownerId == null)
                return new BaseResultDto<List<ClinicServiceVDto>>(false, Resource.Notification.NothingFound, null);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto<List<ClinicServiceVDto>>(false, Resource.Notification.AccessDenied, null);

            var query = _context.ClinicServices.AsNoTracking().Where(s => s.ClinicId == clinicId && !s.Deleted);
            return new BaseResultDto<List<ClinicServiceVDto>>(true, await ProjectAsync(query, petSize: null, onlyActivePackages: false));
        }

        public async Task<BaseResultDto<List<ClinicServiceVDto>>> ListPublicAsync(long clinicId, string petSize)
        {
            var size = NormalizePetSize(petSize);
            var query = _context.ClinicServices.AsNoTracking()
                .Where(s => s.ClinicId == clinicId && !s.Deleted && s.Active && s.Approved
                            && s.Clinic.Active && s.Clinic.Approved && !s.Clinic.Deleted);
            var list = await ProjectAsync(query, size, onlyActivePackages: true);
            // خدمتی که برای این سایز هیچ بسته‌ی قابل‌رزرو ندارد نمایش داده نمی‌شود.
            return new BaseResultDto<List<ClinicServiceVDto>>(true, list.Where(s => s.Packages.Count > 0).ToList());
        }

        // ------------------------------------------------------------------ خدمت

        public async Task<BaseResultDto<ClinicServiceVDto>> AddServiceAsync(ClinicServiceInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.ClinicId <= 0 || dto.AssistanceId <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.InvalidData, null);

                var ownerId = await OwnerOfClinicAsync(dto.ClinicId);
                if (ownerId == null)
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.NothingFound, null);
                if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.AccessDenied, null);

                var assistance = await _context.Assistances.AsNoTracking().Include(a => a.ProviderRole)
                    .Where(a => a.Id == dto.AssistanceId && a.Active && !a.Deleted).FirstOrDefaultAsync();
                if (assistance == null)
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.ClinicServiceAssistanceInvalid, null);
                // خدمت فقط برای مراکزِ نقشِ مالکش قابل ثبت است (کلینیک خدمت مربی/آرایشگاه را نمی‌گیرد و برعکس).
                if (!AssistanceRoleRules.IsAllowedForRole(assistance, ProviderRoleLabels.Clinic))
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.AssistanceNotForRole, null);
                if (await _context.ClinicServices.AnyAsync(s => s.ClinicId == dto.ClinicId && s.AssistanceId == dto.AssistanceId && !s.Deleted))
                    return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.ClinicServiceAlreadyExists, null);

                var service = new ClinicService
                {
                    ClinicId = dto.ClinicId,
                    AssistanceId = dto.AssistanceId,
                    DurationMinutes = dto.DurationMinutes,
                    Description = dto.Description?.Trim(),
                    Active = true,
                    // خدمتِ تعریف‌شده توسط مالک تا تأیید ادمین قابل رزرو نیست؛ ادمین مستقیم تأییدشده می‌سازد.
                    Approved = !ownerUserId.HasValue,
                };
                await _context.ClinicServices.AddAsync(service);
                await _context.SaveChangesAsync();

                var created = await ProjectAsync(_context.ClinicServices.AsNoTracking().Where(s => s.Id == service.Id), null, false);
                return new BaseResultDto<ClinicServiceVDto>(true, created.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding clinic service failed.");
                return new BaseResultDto<ClinicServiceVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdateServiceAsync(ClinicServiceInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var denied = await CheckServiceAccessAsync(dto.Id, ownerUserId);
            if (denied != null)
                return denied;

            var description = dto.Description?.Trim();
            var minutes = dto.DurationMinutes;
            await _context.ClinicServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DurationMinutes, minutes).SetProperty(x => x.Description, description));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ApproveServiceAsync(ClinicServiceApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
            if (dto.CommissionPercent.HasValue && (dto.CommissionPercent < 0 || dto.CommissionPercent > 100))
                return new BaseResultDto(false, Resource.Notification.ClinicValueOutOfRange);

            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.ClinicServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Approved, approve).SetProperty(x => x.ActivationValue, reason));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (dto.CommissionPercent.HasValue)
            {
                var percent = dto.CommissionPercent.Value;
                await _context.ClinicServices.Where(s => s.Id == dto.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.CommissionPercent, percent));
            }
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Deleted, true).SetProperty(x => x.Active, false));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId)
        {
            var service = await _context.ClinicServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted)
                .Select(s => new { s.ClinicId, OwnerId = s.Clinic.OwnerId })
                .FirstOrDefaultAsync();
            if (service == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && service.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            var wanted = (userIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            // فقط مالک و پرسنل فعالِ همین کلینیک می‌توانند گرومرِ یک خدمت باشند.
            var allowed = await _context.ClinicUsers.AsNoTracking()
                .Where(u => u.ClinicId == service.ClinicId && !u.Deleted && u.Active && u.UserAccept == true)
                .Select(u => u.UserId).ToListAsync();
            allowed.Add(service.OwnerId);
            if (wanted.Any(id => !allowed.Contains(id)))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.ClinicServiceUsers.AsTracking().Where(u => u.ClinicServiceId == serviceId).ToListAsync();
            foreach (var row in existing)
            {
                var keep = wanted.Contains(row.UserId);
                row.Deleted = !keep;
                row.Active = keep;
            }
            foreach (var userId in wanted.Where(id => existing.All(e => e.UserId != id)))
                await _context.ClinicServiceUsers.AddAsync(new ClinicServiceUser { ClinicServiceId = serviceId, UserId = userId, Active = true });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ بسته‌ی قیمتی

        public async Task<BaseResultDto<ClinicServicePackageVDto>> AddPackageAsync(ClinicServicePackageInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.ClinicServiceId <= 0)
                    return new BaseResultDto<ClinicServicePackageVDto>(false, Resource.Notification.InvalidData, null);
                var denied = await CheckServiceAccessAsync(dto.ClinicServiceId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<ClinicServicePackageVDto>(false, denied.Messages, null);
                var error = await ValidatePackageAsync(dto);
                if (error != null)
                    return new BaseResultDto<ClinicServicePackageVDto>(false, error, null);

                var types = NormalizeTypes(dto);
                var (basePrice, basePre) = BasePrice(dto, types);
                var package = new ClinicServicePackage
                {
                    ClinicServiceId = dto.ClinicServiceId,
                    Name = dto.Name.Trim(),
                    Price = basePrice,
                    PrePaymentPrice = basePre,
                    Types = types.Select(x => new ClinicServicePackageType { Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice }).ToList(),
                    PictureId = dto.PictureId,
                    Discription = dto.Discription?.Trim(),
                    PetSize = NormalizePetSize(dto.PetSize),
                    Active = true,
                };
                await _context.ClinicServicePackages.AddAsync(package);
                await _context.SaveChangesAsync();
                var created = _mapper.Map<ClinicServicePackageVDto>(package);
                created.Types = types.Select(x => new ClinicPackageTypeVDto { Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice, ModeName = ModeName(x.Mode) }).ToList();
                return new BaseResultDto<ClinicServicePackageVDto>(true, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding clinic package failed.");
                return new BaseResultDto<ClinicServicePackageVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdatePackageAsync(ClinicServicePackageInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var serviceId = await _context.ClinicServicePackages.AsNoTracking()
                .Where(p => p.Id == dto.Id && !p.Deleted).Select(p => (long?)p.ClinicServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;

            dto.ClinicServiceId = serviceId.Value;
            var error = await ValidatePackageAsync(dto);
            if (error != null)
                return new BaseResultDto(false, error);

            var name = dto.Name.Trim();
            var description = dto.Discription?.Trim();
            var size = NormalizePetSize(dto.PetSize);
            var types = NormalizeTypes(dto);
            var (basePrice, basePre) = BasePrice(dto, types);
            await _context.ClinicServicePackages.Where(p => p.Id == dto.Id && !p.Deleted)
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
        private async Task ReplaceTypesAsync(long packageId, List<ClinicPackageTypeDto> types)
        {
            var existing = await _context.ClinicServicePackageTypes.AsTracking().Where(x => x.ClinicServicePackageId == packageId && !x.Deleted).ToListAsync();
            foreach (var row in existing)
            {
                var wanted = types.FirstOrDefault(x => x.Mode == row.Mode);
                if (wanted == null) { row.Deleted = true; continue; }
                row.Price = wanted.Price;
                row.PrePaymentPrice = wanted.PrePaymentPrice;
            }
            foreach (var item in types.Where(x => existing.All(e => e.Mode != x.Mode)))
                await _context.ClinicServicePackageTypes.AddAsync(new ClinicServicePackageType { ClinicServicePackageId = packageId, Mode = item.Mode, Price = item.Price, PrePaymentPrice = item.PrePaymentPrice });
            await _context.SaveChangesAsync();
        }

        public async Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId)
        {
            var serviceId = await _context.ClinicServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.ClinicServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId)
        {
            var serviceId = await _context.ClinicServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.ClinicServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.ClinicServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true).SetProperty(p => p.Active, false));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        private async Task<long?> OwnerOfClinicAsync(long clinicId) =>
            await _context.Clinics.AsNoTracking().Where(g => g.Id == clinicId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();

        /// <summary>null = دسترسی دارد؛ در غیر این صورت نتیجه‌ی خطا.</summary>
        private async Task<BaseResultDto> CheckServiceAccessAsync(long serviceId, long? ownerUserId)
        {
            var ownerId = await _context.ClinicServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted && !s.Clinic.Deleted)
                .Select(s => (long?)s.Clinic.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task<string> ValidatePackageAsync(ClinicServicePackageInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 200)
                return Resource.Notification.PleaseEnterTheName;
            var types = dto.Types ?? new List<ClinicPackageTypeDto>();
            if (types.Count == 0)
            {
                if (dto.Price <= 0 || dto.PrePaymentPrice < 0 || dto.PrePaymentPrice > dto.Price || double.IsNaN(dto.Price) || double.IsInfinity(dto.Price))
                    return Resource.Notification.ClinicPackagePriceInvalid;
            }
            else
            {
                if (types.Select(x => x.Mode).Distinct().Count() != types.Count || types.Any(x => !Enum.IsDefined(typeof(ClinicServiceModeEnum), x.Mode)))
                    return Resource.Notification.ClinicPackageModeInvalid;
                if (types.Any(x => x.Price <= 0 || x.PrePaymentPrice < 0 || x.PrePaymentPrice > x.Price || double.IsNaN(x.Price) || double.IsInfinity(x.Price)))
                    return Resource.Notification.ClinicPackagePriceInvalid;
            }
            if (!string.IsNullOrWhiteSpace(dto.PetSize) && NormalizePetSize(dto.PetSize) == null)
                return Resource.Notification.ClinicPackagePetSizeInvalid;
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId.Value))
                return Resource.Notification.ClinicPictureInvalid;
            return null;
        }

        private static List<ClinicPackageTypeDto> NormalizeTypes(ClinicServicePackageInputDto dto) =>
            (dto.Types ?? new List<ClinicPackageTypeDto>()).GroupBy(x => x.Mode).Select(g => g.First()).ToList();

        /// <summary>اگر نحوه‌ی ارائه داده شده باشد قیمت پایه‌ی بسته = ارزان‌ترین حالت (برای «قیمت از ...» و مرتب‌سازی).</summary>
        private static (double Price, double Pre) BasePrice(ClinicServicePackageInputDto dto, List<ClinicPackageTypeDto> types)
        {
            if (types.Count == 0)
                return (dto.Price, dto.PrePaymentPrice);
            var cheapest = types.OrderBy(x => x.Price).First();
            return (cheapest.Price, cheapest.PrePaymentPrice);
        }

        private static string ModeName(int mode) => mode switch
        {
            (int)ClinicServiceModeEnum.Online => "online",
            (int)ClinicServiceModeEnum.InPerson => "in-person",
            (int)ClinicServiceModeEnum.InPlace => "in-place",
            _ => null,
        };

        /// <summary>اندازه‌ی نرمال‌شده؛ خالی یا نامعتبر = null (برای ورودی نامعتبر، ValidatePackageAsync قبلاً خطا می‌دهد).</summary>
        private static string NormalizePetSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return ClinicPetSizes.All.FirstOrDefault(s => string.Equals(s, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<ClinicServiceVDto>> ProjectAsync(IQueryable<ClinicService> query, string petSize, bool onlyActivePackages)
        {
            var rows = await query.OrderBy(s => s.Id).Select(s => new
            {
                s.Id, s.ClinicId, s.AssistanceId, AssistanceName = s.Assistance.Name, AssistancePicture = s.Assistance.Picture,
                s.Active, s.Approved, s.ActivationValue, s.CommissionPercent, s.DurationMinutes, s.Description,
                Packages = s.Packages
                    .Where(p => !p.Deleted && (!onlyActivePackages || p.Active) && (petSize == null || p.PetSize == null || p.PetSize == petSize))
                    .OrderBy(p => p.Price)
                    .Select(p => new { p.Id, p.ClinicServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Picture, p.Active, p.ActivationValue, p.Discription, p.PetSize, Types = p.Types.Where(x => !x.Deleted).OrderBy(x => x.Mode).Select(x => new { x.Id, x.Mode, x.Price, x.PrePaymentPrice }).ToList() })
                    .ToList(),
                UserIds = s.ServiceUsers.Where(u => !u.Deleted && u.Active).Select(u => u.UserId).ToList(),
            }).AsSplitQuery().ToListAsync();

            return rows.Select(r => new ClinicServiceVDto
            {
                Id = r.Id,
                ClinicId = r.ClinicId,
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
                Packages = r.Packages.Select(p => new ClinicServicePackageVDto
                {
                    Id = p.Id,
                    ClinicServiceId = p.ClinicServiceId,
                    Name = p.Name,
                    Price = p.Price,
                    PrePaymentPrice = p.PrePaymentPrice,
                    PictureId = p.PictureId,
                    Picture = p.Picture == null ? null : _mapper.Map<PictureVDto>(p.Picture),
                    Active = p.Active,
                    ActivationValue = p.ActivationValue,
                    Discription = p.Discription,
                    PetSize = p.PetSize,
                    Types = p.Types.Select(x => new ClinicPackageTypeVDto { Id = x.Id, Mode = x.Mode, Price = x.Price, PrePaymentPrice = x.PrePaymentPrice, ModeName = ModeName(x.Mode) }).ToList(),
                }).ToList(),
            }).ToList();
        }
    }
}
