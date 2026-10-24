using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Application.Common.Helpers;

namespace Application.Services.CompanionSrvs.AssistanceSrv
{
    public class AssistanceService : CommonSrv<Assistance, AssistanceDto>, IAssistanceService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public AssistanceService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<AssistanceVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Assistances
                .AsNoTracking()
                .Include(s => s.AssistanceGroup)
                .Include(s => s.Picture)
                .Include(s => s.ProviderRole)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (item != null)
            {
                return new BaseResultDto<AssistanceVDto>(true, mapper.Map<AssistanceVDto>(item));
            }

            return new BaseResultDto<AssistanceVDto>(false, mapper.Map<AssistanceVDto>(item));
        }

        public async Task<BaseResultDto> UpdateSiteVisibilityAsync(long id, bool showToSite)
        {
            var affectedRows = await _context.Assistances
                .Where(x => x.Id == id && !x.Deleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ShowToSite, showToSite));

            if (affectedRows == 0)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            return new BaseResultDto(true);
        }

        private static readonly string[] AssignableRoleLabels =
        {
            ProviderRoleLabels.Groomer, ProviderRoleLabels.Clinic, ProviderRoleLabels.Coach,
            ProviderRoleLabels.Pansion, ProviderRoleLabels.School,
        };

        /// <summary>نقش معتبر برای خدمت؛ برمی‌گرداند Label یا null اگر نامعتبر باشد.</summary>
        private async Task<string> RoleLabelAsync(long roleId) =>
            await _context.ProviderRoles.AsNoTracking()
                .Where(x => x.Id == roleId && !x.Deleted && x.Active && AssignableRoleLabels.Contains(x.Label))
                .Select(x => x.Label).FirstOrDefaultAsync();

        public async Task<BaseResultDto<AssistanceDto>> InsertValidatedAsync(AssistanceDto dto)
        {
            if (!dto.ProviderRoleId.HasValue || dto.ProviderRoleId.Value <= 0)
                return new BaseResultDto<AssistanceDto>(false, Resource.Notification.AssistanceProviderRoleRequired, nameof(dto.ProviderRoleId), dto);
            var label = await RoleLabelAsync(dto.ProviderRoleId.Value);
            if (label == null)
                return new BaseResultDto<AssistanceDto>(false, Resource.Notification.AssistanceProviderRoleInvalid, nameof(dto.ProviderRoleId), dto);
            dto.IsGrooming = label == ProviderRoleLabels.Groomer;
            return await InsertAsyncDto(dto);
        }

        public async Task<BaseResultDto> UpdateValidatedAsync(AssistanceDto dto)
        {
            var current = await _context.Assistances.AsNoTracking()
                .Where(x => x.Id == dto.Id && !x.Deleted).Select(x => new { x.ProviderRoleId }).FirstOrDefaultAsync();
            if (current == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (!dto.ProviderRoleId.HasValue || dto.ProviderRoleId.Value <= 0)
            {
                // خدمتِ قدیمیِ بدون نقش را می‌توان بدون نقش نگه داشت، ولی نمی‌شود نقشِ موجود را برداشت.
                if (current.ProviderRoleId.HasValue)
                    return new BaseResultDto(false, Resource.Notification.AssistanceProviderRoleRequired, nameof(dto.ProviderRoleId));
                dto.ProviderRoleId = null;
                return UpdateDto(dto);
            }

            var label = await RoleLabelAsync(dto.ProviderRoleId.Value);
            if (label == null)
                return new BaseResultDto(false, Resource.Notification.AssistanceProviderRoleInvalid, nameof(dto.ProviderRoleId));

            if (current.ProviderRoleId != dto.ProviderRoleId)
            {
                // تغییر نقشِ خدمتی که مرکزی از نقش دیگر هم‌اکنون با آن کار می‌کند، آن مراکز را بی‌صاحب می‌کند.
                var usedByOtherRole =
                    (label != ProviderRoleLabels.Clinic && await _context.ClinicServices.AnyAsync(s => s.AssistanceId == dto.Id && !s.Deleted)) ||
                    (label != ProviderRoleLabels.Coach && await _context.CoachServices.AnyAsync(s => s.AssistanceId == dto.Id && !s.Deleted)) ||
                    (label != ProviderRoleLabels.Groomer && await _context.GroomerServices.AnyAsync(s => s.AssistanceId == dto.Id && !s.Deleted));
                if (usedByOtherRole)
                    return new BaseResultDto(false, Resource.Notification.AssistanceProviderRoleInUse, nameof(dto.ProviderRoleId));
            }

            dto.IsGrooming = label == ProviderRoleLabels.Groomer;
            return UpdateDto(dto);
        }

        public AssistanceSearchDto Search(AssistanceInputDto baseSearchDto)
        {
            var model = _context.Assistances
                .AsNoTracking()
                .Include(s => s.AssistanceGroup)
                .Include(s => s.Picture)
                .Include(s => s.ProviderRole)
                .Where(s => !s.Deleted)
                .AsQueryable();

            if (baseSearchDto.Available == true)
            {
                model = model.Where(
                    s => s.Active &&
                         (!s.AssistanceGroupId.HasValue ||
                          (s.AssistanceGroup.Active &&
                           !s.AssistanceGroup.Deleted)));
            }
            else if (baseSearchDto.Available == false)
            {
                model = model.Where(s => !s.Active);
            }

            if (baseSearchDto.ShowToSite.HasValue)
            {
                model = model.Where(s => s.ShowToSite == baseSearchDto.ShowToSite.Value);
            }

            if (baseSearchDto.IsPersonal.HasValue)
            {
                model = model.Where(s => s.IsPersonal == baseSearchDto.IsPersonal.Value);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.ProviderRole))
            {
                // فیلتر نقش (کلید ثابت) بر IsGrooming غلبه دارد؛ قاعده‌ی یکپارچه در AssistanceRoleRules.
                model = model.ForRole(baseSearchDto.ProviderRole.Trim());
            }
            else if (baseSearchDto.IsGrooming.HasValue)
            {
                model = model.Where(s => s.IsGrooming == baseSearchDto.IsGrooming.Value);
            }

            if (baseSearchDto.ProviderRoleId == 0)
            {
                model = model.Where(s => s.ProviderRoleId == null);
            }
            else if (baseSearchDto.ProviderRoleId.HasValue)
            {
                model = model.Where(s => s.ProviderRoleId == baseSearchDto.ProviderRoleId.Value);
            }

            if (baseSearchDto.AssistanceGroupId.HasValue)
            {
                model = model.Where(
                    s => s.AssistanceGroupId == baseSearchDto.AssistanceGroupId.Value);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();
                model = model.Where(
                    s => s.Name.Contains(q) ||
                         (s.Summary != null && s.Summary.Contains(q)) ||
                         (s.Description != null && s.Description.Contains(q)));
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

            return new AssistanceSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<List<SearchAssistanceDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<Assistance>(request.SearchTerms, item => item.Name, item => item.AssistanceGroup.Name);
            var query = _context.Assistances.Where(s => s.Deleted == false && s.Active && !s.IsGrooming);
            var list = await query.Where(predicate)
                .Take(SearchQueryHelper.CandidateCount(request.AssistanceCount))
                .Select(s => new SearchAssistanceDto { Id = s.Id, Name = s.Name, PictureId = s.PictureId, Picture = mapper.Map<PictureVDto>(s.Picture) })
                .ToListAsync();
            return list;
        }
    }
}
