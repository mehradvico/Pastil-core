using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Iface;
using AutoMapper;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerCatalogSrv
{
    public class GroomerCatalogService : IGroomerCatalogService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GroomerCatalogService> _logger;

        public GroomerCatalogService(IDataBaseContext context, IMapper mapper, ILogger<GroomerCatalogService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ------------------------------------------------------------------ لیست

        public async Task<BaseResultDto<List<GroomerServiceVDto>>> ListManageAsync(long groomerId, long? ownerUserId)
        {
            var ownerId = await OwnerOfGroomerAsync(groomerId);
            if (ownerId == null)
                return new BaseResultDto<List<GroomerServiceVDto>>(false, Resource.Notification.NothingFound, null);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto<List<GroomerServiceVDto>>(false, Resource.Notification.AccessDenied, null);

            var query = _context.GroomerServices.AsNoTracking().Where(s => s.GroomerId == groomerId && !s.Deleted);
            return new BaseResultDto<List<GroomerServiceVDto>>(true, await ProjectAsync(query, petSize: null, onlyActivePackages: false));
        }

        public async Task<BaseResultDto<List<GroomerServiceVDto>>> ListPublicAsync(long groomerId, string petSize)
        {
            var size = NormalizePetSize(petSize);
            var query = _context.GroomerServices.AsNoTracking()
                .Where(s => s.GroomerId == groomerId && !s.Deleted && s.Active && s.Approved
                            && s.Groomer.Active && s.Groomer.Approved && !s.Groomer.Deleted);
            var list = await ProjectAsync(query, size, onlyActivePackages: true);
            // خدمتی که برای این سایز هیچ بسته‌ی قابل‌رزرو ندارد نمایش داده نمی‌شود.
            return new BaseResultDto<List<GroomerServiceVDto>>(true, list.Where(s => s.Packages.Count > 0).ToList());
        }

        // ------------------------------------------------------------------ خدمت

        public async Task<BaseResultDto<GroomerServiceVDto>> AddServiceAsync(GroomerServiceInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.GroomerId <= 0 || dto.AssistanceId <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.InvalidData, null);

                var ownerId = await OwnerOfGroomerAsync(dto.GroomerId);
                if (ownerId == null)
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.NothingFound, null);
                if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.AccessDenied, null);

                var assistance = await _context.Assistances.AsNoTracking().Where(a => a.Id == dto.AssistanceId && a.Active && !a.Deleted)
                    .Select(a => new { a.IsGrooming }).FirstOrDefaultAsync();
                if (assistance == null)
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.GroomerServiceAssistanceInvalid, null);
                if (!assistance.IsGrooming)
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.GroomerServiceNotGrooming, null);
                if (await _context.GroomerServices.AnyAsync(s => s.GroomerId == dto.GroomerId && s.AssistanceId == dto.AssistanceId && !s.Deleted))
                    return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.GroomerServiceAlreadyExists, null);

                var service = new GroomerService
                {
                    GroomerId = dto.GroomerId,
                    AssistanceId = dto.AssistanceId,
                    DurationMinutes = dto.DurationMinutes,
                    Description = dto.Description?.Trim(),
                    Active = true,
                    // خدمتِ تعریف‌شده توسط مالک تا تأیید ادمین قابل رزرو نیست؛ ادمین مستقیم تأییدشده می‌سازد.
                    Approved = !ownerUserId.HasValue,
                };
                await _context.GroomerServices.AddAsync(service);
                await _context.SaveChangesAsync();

                var created = await ProjectAsync(_context.GroomerServices.AsNoTracking().Where(s => s.Id == service.Id), null, false);
                return new BaseResultDto<GroomerServiceVDto>(true, created.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding groomer service failed.");
                return new BaseResultDto<GroomerServiceVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdateServiceAsync(GroomerServiceInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0 || dto.DurationMinutes < 0 || dto.DurationMinutes > 24 * 60)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var denied = await CheckServiceAccessAsync(dto.Id, ownerUserId);
            if (denied != null)
                return denied;

            var description = dto.Description?.Trim();
            var minutes = dto.DurationMinutes;
            await _context.GroomerServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DurationMinutes, minutes).SetProperty(x => x.Description, description));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceActiveAsync(long serviceId, bool active, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ApproveServiceAsync(GroomerServiceApproveDto dto)
        {
            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
            if (dto.CommissionPercent.HasValue && (dto.CommissionPercent < 0 || dto.CommissionPercent > 100))
                return new BaseResultDto(false, Resource.Notification.GroomerValueOutOfRange);

            var approve = dto.Approve;
            var reason = approve ? null : dto.ApprovalValue.Trim();
            var affected = await _context.GroomerServices.Where(s => s.Id == dto.Id && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Approved, approve).SetProperty(x => x.ActivationValue, reason));
            if (affected == 0)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (dto.CommissionPercent.HasValue)
            {
                var percent = dto.CommissionPercent.Value;
                await _context.GroomerServices.Where(s => s.Id == dto.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.CommissionPercent, percent));
            }
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteServiceAsync(long serviceId, long? ownerUserId)
        {
            var denied = await CheckServiceAccessAsync(serviceId, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerServices.Where(s => s.Id == serviceId && !s.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Deleted, true).SetProperty(x => x.Active, false));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetServiceUsersAsync(long serviceId, IReadOnlyCollection<long> userIds, long? ownerUserId)
        {
            var service = await _context.GroomerServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted)
                .Select(s => new { s.GroomerId, OwnerId = s.Groomer.OwnerId })
                .FirstOrDefaultAsync();
            if (service == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && service.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            var wanted = (userIds ?? Array.Empty<long>()).Where(id => id > 0).Distinct().ToList();
            // فقط مالک و پرسنل فعالِ همین آرایشگاه می‌توانند گرومرِ یک خدمت باشند.
            var allowed = await _context.GroomerUsers.AsNoTracking()
                .Where(u => u.GroomerId == service.GroomerId && !u.Deleted && u.Active && u.UserAccept == true)
                .Select(u => u.UserId).ToListAsync();
            allowed.Add(service.OwnerId);
            if (wanted.Any(id => !allowed.Contains(id)))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var existing = await _context.GroomerServiceUsers.AsTracking().Where(u => u.GroomerServiceId == serviceId).ToListAsync();
            foreach (var row in existing)
            {
                var keep = wanted.Contains(row.UserId);
                row.Deleted = !keep;
                row.Active = keep;
            }
            foreach (var userId in wanted.Where(id => existing.All(e => e.UserId != id)))
                await _context.GroomerServiceUsers.AddAsync(new GroomerServiceUser { GroomerServiceId = serviceId, UserId = userId, Active = true });
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ بسته‌ی قیمتی

        public async Task<BaseResultDto<GroomerServicePackageVDto>> AddPackageAsync(GroomerServicePackageInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.GroomerServiceId <= 0)
                    return new BaseResultDto<GroomerServicePackageVDto>(false, Resource.Notification.InvalidData, null);
                var denied = await CheckServiceAccessAsync(dto.GroomerServiceId, ownerUserId);
                if (denied != null)
                    return new BaseResultDto<GroomerServicePackageVDto>(false, denied.Messages, null);
                var error = await ValidatePackageAsync(dto);
                if (error != null)
                    return new BaseResultDto<GroomerServicePackageVDto>(false, error, null);

                var package = new GroomerServicePackage
                {
                    GroomerServiceId = dto.GroomerServiceId,
                    Name = dto.Name.Trim(),
                    Price = dto.Price,
                    PrePaymentPrice = dto.PrePaymentPrice,
                    PictureId = dto.PictureId,
                    Discription = dto.Discription?.Trim(),
                    PetSize = NormalizePetSize(dto.PetSize),
                    Active = true,
                };
                await _context.GroomerServicePackages.AddAsync(package);
                await _context.SaveChangesAsync();
                return new BaseResultDto<GroomerServicePackageVDto>(true, _mapper.Map<GroomerServicePackageVDto>(package));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding groomer package failed.");
                return new BaseResultDto<GroomerServicePackageVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> UpdatePackageAsync(GroomerServicePackageInputDto dto, long? ownerUserId)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var serviceId = await _context.GroomerServicePackages.AsNoTracking()
                .Where(p => p.Id == dto.Id && !p.Deleted).Select(p => (long?)p.GroomerServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;

            dto.GroomerServiceId = serviceId.Value;
            var error = await ValidatePackageAsync(dto);
            if (error != null)
                return new BaseResultDto(false, error);

            var name = dto.Name.Trim();
            var description = dto.Discription?.Trim();
            var size = NormalizePetSize(dto.PetSize);
            await _context.GroomerServicePackages.Where(p => p.Id == dto.Id && !p.Deleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Name, name)
                    .SetProperty(p => p.Price, dto.Price)
                    .SetProperty(p => p.PrePaymentPrice, dto.PrePaymentPrice)
                    .SetProperty(p => p.PictureId, dto.PictureId)
                    .SetProperty(p => p.Discription, description)
                    .SetProperty(p => p.PetSize, size));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetPackageActiveAsync(long packageId, bool active, long? ownerUserId)
        {
            var serviceId = await _context.GroomerServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.GroomerServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, active));
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeletePackageAsync(long packageId, long? ownerUserId)
        {
            var serviceId = await _context.GroomerServicePackages.AsNoTracking()
                .Where(p => p.Id == packageId && !p.Deleted).Select(p => (long?)p.GroomerServiceId).FirstOrDefaultAsync();
            if (serviceId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            var denied = await CheckServiceAccessAsync(serviceId.Value, ownerUserId);
            if (denied != null)
                return denied;
            await _context.GroomerServicePackages.Where(p => p.Id == packageId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true).SetProperty(p => p.Active, false));
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        private async Task<long?> OwnerOfGroomerAsync(long groomerId) =>
            await _context.Groomers.AsNoTracking().Where(g => g.Id == groomerId && !g.Deleted).Select(g => (long?)g.OwnerId).FirstOrDefaultAsync();

        /// <summary>null = دسترسی دارد؛ در غیر این صورت نتیجه‌ی خطا.</summary>
        private async Task<BaseResultDto> CheckServiceAccessAsync(long serviceId, long? ownerUserId)
        {
            var ownerId = await _context.GroomerServices.AsNoTracking()
                .Where(s => s.Id == serviceId && !s.Deleted && !s.Groomer.Deleted)
                .Select(s => (long?)s.Groomer.OwnerId).FirstOrDefaultAsync();
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && ownerId.Value != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            return null;
        }

        private async Task<string> ValidatePackageAsync(GroomerServicePackageInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 200)
                return Resource.Notification.PleaseEnterTheName;
            if (dto.Price <= 0 || dto.PrePaymentPrice < 0 || dto.PrePaymentPrice > dto.Price || double.IsNaN(dto.Price) || double.IsInfinity(dto.Price))
                return Resource.Notification.GroomerPackagePriceInvalid;
            if (!string.IsNullOrWhiteSpace(dto.PetSize) && NormalizePetSize(dto.PetSize) == null)
                return Resource.Notification.GroomerPackagePetSizeInvalid;
            if (dto.PictureId.HasValue && dto.PictureId.Value > 0 && !await _context.Pictures.AnyAsync(p => p.Id == dto.PictureId.Value))
                return Resource.Notification.GroomerPictureInvalid;
            return null;
        }

        /// <summary>اندازه‌ی نرمال‌شده؛ خالی یا نامعتبر = null (برای ورودی نامعتبر، ValidatePackageAsync قبلاً خطا می‌دهد).</summary>
        private static string NormalizePetSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return GroomerPetSizes.All.FirstOrDefault(s => string.Equals(s, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<GroomerServiceVDto>> ProjectAsync(IQueryable<GroomerService> query, string petSize, bool onlyActivePackages)
        {
            var rows = await query.OrderBy(s => s.Id).Select(s => new
            {
                s.Id, s.GroomerId, s.AssistanceId, AssistanceName = s.Assistance.Name, AssistancePicture = s.Assistance.Picture,
                s.Active, s.Approved, s.ActivationValue, s.CommissionPercent, s.DurationMinutes, s.Description,
                Packages = s.Packages
                    .Where(p => !p.Deleted && (!onlyActivePackages || p.Active) && (petSize == null || p.PetSize == null || p.PetSize == petSize))
                    .OrderBy(p => p.Price)
                    .Select(p => new { p.Id, p.GroomerServiceId, p.Name, p.Price, p.PrePaymentPrice, p.PictureId, p.Picture, p.Active, p.ActivationValue, p.Discription, p.PetSize })
                    .ToList(),
                UserIds = s.ServiceUsers.Where(u => !u.Deleted && u.Active).Select(u => u.UserId).ToList(),
            }).AsSplitQuery().ToListAsync();

            return rows.Select(r => new GroomerServiceVDto
            {
                Id = r.Id,
                GroomerId = r.GroomerId,
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
                Packages = r.Packages.Select(p => new GroomerServicePackageVDto
                {
                    Id = p.Id,
                    GroomerServiceId = p.GroomerServiceId,
                    Name = p.Name,
                    Price = p.Price,
                    PrePaymentPrice = p.PrePaymentPrice,
                    PictureId = p.PictureId,
                    Picture = p.Picture == null ? null : _mapper.Map<PictureVDto>(p.Picture),
                    Active = p.Active,
                    ActivationValue = p.ActivationValue,
                    Discription = p.Discription,
                    PetSize = p.PetSize,
                }).ToList(),
            }).ToList();
        }
    }
}
