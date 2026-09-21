using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace Application.Services.CompanionSrv.CompanionAssistancePackageSrv
{
    public class CompanionAssistancePackageService : CommonSrv<CompanionAssistancePackage, CompanionAssistancePackageDto>, ICompanionAssistancePackageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        public CompanionAssistancePackageService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, INoticeService notificationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._notificationService = notificationService;    
        }

        public override async Task<BaseResultDto<CompanionAssistancePackageDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionAssistancePackages.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).ThenInclude(s => s.Picture).Include(s => s.Picture)
                .Include(s => s.PackageTypes).ThenInclude(s => s.CompanionAssistanceType).Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistancePackageDto>(true, mapper.Map<CompanionAssistancePackageDto>(item));
            }
            return new BaseResultDto<CompanionAssistancePackageDto>(false, mapper.Map<CompanionAssistancePackageDto>(item));
        }
        public async Task<BaseResultDto<CompanionAssistancePackageVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistancePackages.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).ThenInclude(s => s.Picture).Include(s => s.Picture)
                .Include(s => s.PackageTypes).ThenInclude(s => s.CompanionAssistanceType).Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistancePackageVDto>(true, mapper.Map<CompanionAssistancePackageVDto>(item));
            }
            return new BaseResultDto<CompanionAssistancePackageVDto>(false, mapper.Map<CompanionAssistancePackageVDto>(item));
        }

        public CompanionAssistancePackageSearchDto Search(CompanionAssistancePackageInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistancePackages.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).ThenInclude(s => s.Picture).Include(s => s.Picture)
                .Include(s => s.PackageTypes).ThenInclude(s => s.CompanionAssistanceType).Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (!string.IsNullOrWhiteSpace(baseSearchDto.PetSize))
            {
                // پکیج بدون سایز (PetSize خالی) یعنی مخصوص هیچ سایز خاصی نیست، پس برای همه‌ی
                // سایزها هم نمایش داده می‌شود؛ فقط پکیج‌هایی که برای یک سایز دیگه تعریف شدن حذف می‌شن.
                model = model.Where(s => s.PetSize == null || s.PetSize == baseSearchDto.PetSize);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionAssistancePackageSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<List<SearchCompanionAssistancePackageDto>> SearchMinAsync(
            SearchRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var query = _context.CompanionAssistancePackages
                .AsNoTracking()
                .Include(item => item.Picture)
                .Include(item => item.CompanionAssistance).ThenInclude(item => item.Companion)
                .Include(item => item.CompanionAssistance).ThenInclude(item => item.Assistance)
                .Where(item =>
                    !item.Deleted &&
                    item.Active &&
                    !item.CompanionAssistance.Deleted &&
                    item.CompanionAssistance.Active &&
                    item.CompanionAssistance.Approved &&
                    !item.CompanionAssistance.Companion.Deleted &&
                    item.CompanionAssistance.Companion.Active &&
                    item.CompanionAssistance.Companion.Approved &&
                    !item.CompanionAssistance.Assistance.Deleted &&
                    item.CompanionAssistance.Assistance.Active);

            var predicate = SearchQueryHelper.ContainsAny<CompanionAssistancePackage>(request.SearchTerms,
                item => item.Name,
                item => item.Discription,
                item => item.CompanionAssistance.Companion.Name,
                item => item.CompanionAssistance.Companion.SearchKey,
                item => item.CompanionAssistance.Assistance.Name);
            query = query.Where(predicate);

            var candidates = await query
                .OrderByDescending(item => item.CompanionAssistance.Companion.RateAvg)
                .ThenBy(item => item.Price)
                .Take(Math.Min(request.PackageCount * 3, SearchRequestDto.MaxPerTypeCount * 3))
                .ToListAsync(cancellationToken);

            return candidates.Select(item => new SearchCompanionAssistancePackageDto
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                PrePaymentPrice = item.PrePaymentPrice,
                CompanionAssistanceId = item.CompanionAssistanceId,
                CompanionId = item.CompanionAssistance.CompanionId,
                CompanionName = item.CompanionAssistance.Companion.Name,
                AssistanceId = item.CompanionAssistance.AssistanceId,
                AssistanceName = item.CompanionAssistance.Assistance.Name,
                Description = item.Discription,
                Picture = mapper.Map<PictureVDto>(item.Picture)
            }).ToList();
        }

        public override async Task<BaseResultDto<CompanionAssistancePackageDto>> InsertAsyncDto(CompanionAssistancePackageDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistancePackageDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                var validation = ValidatePackage(dto, isInsert: true);
                if (validation != null)
                {
                    return new BaseResultDto<CompanionAssistancePackageDto>(false, validation, dto);
                }

                var item = mapper.Map<CompanionAssistancePackage>(dto);
                item.PackageTypes = BuildTypeRows(dto);
                await _context.CompanionAssistancePackages.AddAsync(item);
                await _context.SaveChangesAsync();
                SyncServiceModes(item.CompanionAssistanceId);
                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistancePackageSubmitted, ReferenceType = "CompanionAssistancePackage", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistancePackageSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "companionAssistanceId", item.CompanionAssistanceId.ToString() } } });


                return new BaseResultDto<CompanionAssistancePackageDto>(true, mapper.Map<CompanionAssistancePackageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistancePackageDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }

        /// <summary>
        /// اعتبارسنجی قیمت/حالت‌ها. null = معتبر؛ در غیر این صورت پیام خطا.
        /// - هر «نحوه ارائه» باید جزو حالت‌های همین خدمت باشد، یک بار بیاید، قیمتش > ۰ و پیش‌پرداختش بین ۰ و قیمت باشد
        /// - وقتی حالت‌ها آمده‌اند، Price/PrePaymentPrice خود پکیج از ارزان‌ترین حالت مشتق می‌شود (لیست‌ها/مرتب‌سازی قدیمی سالم می‌مانند)
        /// - تکراری‌بودن پکیج با «نام + سایز پت» در همان خدمت سنجیده می‌شود (قبلاً فقط قیمت یکسان رد می‌شد)
        /// </summary>
        private string ValidatePackage(CompanionAssistancePackageDto dto, bool isInsert)
        {
            var name = dto.Name?.Trim();
            var petSize = string.IsNullOrWhiteSpace(dto.PetSize) ? null : dto.PetSize.Trim();
            var duplicate = _context.CompanionAssistancePackages.Any(a =>
                a.CompanionAssistanceId == dto.CompanionAssistanceId &&
                !a.Deleted &&
                a.Id != dto.Id &&
                a.Name == name &&
                a.PetSize == petSize);
            if (duplicate)
                return Resource.Notification.DuplicateValue;

            var types = dto.Types;
            if (types != null && types.Count > 0)
            {
                if (types.Select(t => t.CompanionAssistanceTypeId).Distinct().Count() != types.Count)
                    return Resource.Notification.InvalidData;

                // نحوه ارائه را خودِ پکیج تعیین می‌کند (دیگر لازم نیست قبلاً روی خدمت انتخاب شده باشد)؛ فقط یکی از سه حالت تعریف‌شده مجاز است
                var allowedModes = new[]
                {
                    (long)Application.Common.Enumerable.Code.CompanionAssistanceTypeEnum.CompanionAssistanceType_Online,
                    (long)Application.Common.Enumerable.Code.CompanionAssistanceTypeEnum.CompanionAssistanceType_InPerson,
                    (long)Application.Common.Enumerable.Code.CompanionAssistanceTypeEnum.CompanionAssistanceType_InPlace
                };

                foreach (var type in types)
                {
                    if (!allowedModes.Contains(type.CompanionAssistanceTypeId) ||
                        !(type.Price > 0) ||
                        type.PrePaymentPrice < 0 ||
                        type.PrePaymentPrice > type.Price)
                        return Resource.Notification.InvalidData;
                }

                var cheapest = types.OrderBy(t => t.Price).First();
                dto.Price = cheapest.Price;
                dto.PrePaymentPrice = cheapest.PrePaymentPrice;
                return null;
            }

            // مسیر قدیمی (بدون حالت‌ها): مبلغ نباید منفی/نامعتبر باشد و پیش‌پرداخت نباید از قیمت بیشتر شود
            if (dto.Price < 0 || dto.PrePaymentPrice < 0 || dto.PrePaymentPrice > dto.Price)
                return Resource.Notification.InvalidData;

            return null;
        }

        private static List<CompanionAssistancePackageType> BuildTypeRows(CompanionAssistancePackageDto dto, long? packageId = null)
        {
            var rows = new List<CompanionAssistancePackageType>();
            if (dto.Types == null)
                return rows;

            foreach (var type in dto.Types)
            {
                var row = new CompanionAssistancePackageType
                {
                    CompanionAssistanceTypeId = type.CompanionAssistanceTypeId,
                    Price = type.Price,
                    PrePaymentPrice = type.PrePaymentPrice
                };
                if (packageId.HasValue)
                    row.CompanionAssistancePackageId = packageId.Value;
                rows.Add(row);
            }
            return rows;
        }

        public override BaseResultDto UpdateDto(CompanionAssistancePackageDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistancePackageDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var validation = ValidatePackage(dto, isInsert: false);
                    if (validation != null)
                    {
                        return new BaseResultDto(false, validation);
                    }

                    var item = mapper.Map<CompanionAssistancePackage>(dto);
                    _context.CompanionAssistancePackages.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();

                    // فهرست «نحوه ارائه + قیمت» کامل جایگزین می‌شود؛ Types خالی/null یعنی بدون تغییر (کلاینت‌های قدیمی)
                    if (dto.Types != null && dto.Types.Count > 0)
                    {
                        _context.CompanionAssistancePackageTypes.Where(t => t.CompanionAssistancePackageId == dto.Id).ExecuteDelete();
                        _context.CompanionAssistancePackageTypes.AddRange(BuildTypeRows(dto, dto.Id));
                        _context.SaveChanges();
                    }
                    SyncServiceModes(dto.CompanionAssistanceId);
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto> UpdateAsyncDto(CompanionAssistancePackageDto dto)
        {
            var result = UpdateDto(dto);
            if (result.IsSuccess)
                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistancePackageUpdated, ReferenceType = "CompanionAssistancePackage", ReferenceId = dto.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistancePackageUpdated}:{dto.Id}:{DateTime.UtcNow.Ticks}", Metadata = new Dictionary<string, string> { { "companionAssistanceId", dto.CompanionAssistanceId.ToString() } } });
            return result;
        }

        public BaseResultDto ActivationDto(CompanionAssistancePackageActivationDto dto)
        {
            var item = _context.CompanionAssistancePackages.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (!dto.Active)
            {
                item.Active = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Active && string.IsNullOrEmpty(dto.ActivationValue))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else
            {
                item.Active = true;
                item.ActivationValue = dto.ActivationValue;
            }
            _context.CompanionAssistancePackages.Update(item);
            _context.SaveChanges();
            SyncServiceModes(item.CompanionAssistanceId);
            return new BaseResultDto(isSuccess: true);

        }

        public override BaseResultDto DeleteDto(long id)
        {
            var serviceId = _context.CompanionAssistancePackages.Where(p => p.Id == id).Select(p => (long?)p.CompanionAssistanceId).FirstOrDefault();
            var result = base.DeleteDto(id);
            if (result.IsSuccess && serviceId.HasValue)
                SyncServiceModes(serviceId.Value);
            return result;
        }

        /// <summary>
        /// «نحوه ارائه»ی خدمت (CompanionAssistance.Codes) از روی پکیج‌های فعال آن مشتق می‌شود: اجتماع حالت‌هایی که پکیج‌های فعالِ دارای حالت ارائه می‌دهند.
        /// اگر پکیج فعالِ قدیمی (بدون ردیف حالت) هم باشد، حالت‌های فعلی خدمت دست‌نخورده می‌ماند و فقط حالت‌های جدید اضافه می‌شود.
        /// اگر هیچ پکیج فعالِ دارای حالت نباشد، حالت‌های فعلی حفظ می‌شود. خطای همگام‌سازی هرگز ذخیره‌ی پکیج را نمی‌شکند.
        /// </summary>
        private void SyncServiceModes(long companionAssistanceId)
        {
            try
            {
                var modeIds = new long[] { 37, 38, 39 };
                var service = _context.CompanionAssistances.AsTracking().Include(s => s.Codes)
                    .FirstOrDefault(s => s.Id == companionAssistanceId && !s.Deleted);
                if (service == null)
                    return;

                var activePackages = _context.CompanionAssistancePackages.AsNoTracking().Include(p => p.PackageTypes)
                    .Where(p => p.CompanionAssistanceId == companionAssistanceId && !p.Deleted && p.Active)
                    .ToList();

                var offered = activePackages
                    .SelectMany(p => p.PackageTypes.Where(t => !t.Deleted).Select(t => t.CompanionAssistanceTypeId))
                    .Distinct().ToList();
                var hasLegacyActivePackage = activePackages.Any(p => !p.PackageTypes.Any(t => !t.Deleted));
                var current = service.Codes.Select(c => c.Id).Where(id => modeIds.Contains(id)).ToList();

                var desired = hasLegacyActivePackage
                    ? current.Union(offered).ToList()
                    : (offered.Count > 0 ? offered : current);

                if (desired.Count == current.Count && !desired.Except(current).Any())
                    return;

                foreach (var stale in service.Codes.Where(c => modeIds.Contains(c.Id) && !desired.Contains(c.Id)).ToList())
                    service.Codes.Remove(stale);

                var missing = desired.Where(id => service.Codes.All(c => c.Id != id)).ToList();
                if (missing.Count > 0)
                {
                    foreach (var code in _context.Codes.AsTracking().Where(c => missing.Contains(c.Id)).ToList())
                        service.Codes.Add(code);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ex.ToClientMessage(); // فقط لاگ می‌کند
            }
        }
    }
}
