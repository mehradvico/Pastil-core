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
                .Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
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
                .Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
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
                .Include(s => s.CompanionAssistancePackagePictures).ThenInclude(s => s.Picture).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
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
                bool exists = await _context.CompanionAssistancePackages.AnyAsync(a => a.CompanionAssistanceId == dto.CompanionAssistanceId && a.Price == dto.Price && !a.Deleted);

                if (exists)
                {
                    return new BaseResultDto<CompanionAssistancePackageDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<CompanionAssistancePackage>(dto);
                await _context.CompanionAssistancePackages.AddAsync(item);
                await _context.SaveChangesAsync();
                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistancePackageSubmitted, ReferenceType = "CompanionAssistancePackage", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistancePackageSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "companionAssistanceId", item.CompanionAssistanceId.ToString() } } });


                return new BaseResultDto<CompanionAssistancePackageDto>(true, mapper.Map<CompanionAssistancePackageDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistancePackageDto>(isSuccess: false, val: ex.Message, data: dto);
            }
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
                    var item = mapper.Map<CompanionAssistancePackage>(dto);
                    _context.CompanionAssistancePackages.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
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
            return new BaseResultDto(isSuccess: true);

        }
    }
}
