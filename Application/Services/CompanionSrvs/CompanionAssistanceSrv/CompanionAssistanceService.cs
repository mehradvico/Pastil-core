using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistanceTypeSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceSrv
{
    public class CompanionAssistanceService : CommonSrv<CompanionAssistance, CompanionAssistanceDto>, ICompanionAssistanceService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICompanionAssistanceTypeService _companionAssistanceTypeService;
        private readonly CurrentUserDto _currentUser;
        private readonly INoticeService _notificationService;
        private readonly ICodeService _codeService;
        public CompanionAssistanceService(IDataBaseContext _context, IMapper mapper, ICompanionAssistanceTypeService companionAssistanceTypeService, ICurrentUserHelper currentUserHelper, ICodeService codeService, INoticeService notificationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._companionAssistanceTypeService = companionAssistanceTypeService;
            this._currentUser = currentUserHelper.CurrentUser;
            this._codeService = codeService;
            this._notificationService = notificationService;
        }
        public override async Task<BaseResultDto<CompanionAssistanceDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionAssistances.Include(s => s.Codes).Include(s => s.CompanionType).Include(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Companion).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.City).Include(s => s.Companion).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .FirstOrDefaultAsync(s => s.Id == id && s.Deleted != true);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceDto>(true, mapper.Map<CompanionAssistanceDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceDto>(false, mapper.Map<CompanionAssistanceDto>(item));
        }

        public async Task<BaseResultDto<CompanionAssistanceVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistances.Include(s => s.Codes).Include(s => s.CompanionType).Include(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Companion).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.City).Include(s => s.Companion).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted && s.Active && s.Approved);
            
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceVDto>(true, mapper.Map<CompanionAssistanceVDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceVDto>(false, mapper.Map<CompanionAssistanceVDto>(item));
        }

        public CompanionAssistanceSearchDto Search(CompanionAssistanceInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistances.Include(s => s.Codes).Include(s => s.CompanionType).Include(s => s.Assistance).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.Owner).Include(s => s.Companion).ThenInclude(s => s.Picture)
                .Include(s => s.Companion).ThenInclude(s => s.City).Include(s => s.Companion).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .AsQueryable().Where(s => !s.Deleted & !s.Assistance.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => (s.Active && s.Approved) == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.Companion.CompanionPets.Any(p => p.PetId == baseSearchDto.PetId.Value));
            }
            if (baseSearchDto.AssistanceId.HasValue)
            {
                model = model.Where(s => s.AssistanceId == baseSearchDto.AssistanceId.Value);
            }
            if (baseSearchDto.Type.HasValue)
            {
                model = model.Where(s => s.Codes.Any(c => c.Label == baseSearchDto.Type.ToString()));
            }
            if (baseSearchDto.IsPersonal.HasValue)
            {
                model = model.Where(s => s.Companion.IsPersonal == baseSearchDto.IsPersonal.Value);
            }
            if (baseSearchDto.CompanionTypeId.HasValue)
            {
                model = model.Where(s => s.CompanionTypeId == baseSearchDto.CompanionTypeId.Value);
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
            return new CompanionAssistanceSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionAssistanceDto>> InsertAsyncDto(CompanionAssistanceDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistanceDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                bool exists = await _context.CompanionAssistances.AnyAsync(a => a.CompanionId == dto.CompanionId && a.AssistanceId == dto.AssistanceId && a.Deleted != true);

                if (exists)
                {
                    return new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<CompanionAssistance>(dto);
                await _context.CompanionAssistances.AddAsync(item);
                await _context.SaveChangesAsync();
                if (dto.CompanionAssistanceTypeIds == null)
                {
                    dto.CompanionAssistanceTypeIds = new List<long>();
                }
                if (!dto.CompanionAssistanceTypeIds.Any())
                {
                    return new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                }
                dto.CompanionAssistanceTypeIds = dto.CompanionAssistanceTypeIds.Distinct().ToList();
                await _companionAssistanceTypeService.InsertOrUpdateAsync(item, dto.CompanionAssistanceTypeIds);
                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistanceSubmitted, ReferenceType = "CompanionAssistance", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistanceSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "companionId", item.CompanionId.ToString() } } });
                return new BaseResultDto<CompanionAssistanceDto>(true, mapper.Map<CompanionAssistanceDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistanceDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateAsyncDto(CompanionAssistanceDto dto, long? companionId = null)
        {
            try
            {

                var item = await _context.CompanionAssistances.Include(s => s.Codes).AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
                if (item == null)
                    return new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.NothingFound, dto);
                if (companionId.HasValue && item.CompanionId != companionId.Value)
                    return new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.AccessDenied, dto);
                item.Active = dto.Active;
                item.AssistanceId = dto.AssistanceId;
                item.IsSinglePackage = dto.IsSinglePackage;
                if (_currentUser.RoleEnum == RoleEnum.Admin.ToString())
                {
                    item.Approved = dto.Approved;
                }
                if (dto.CompanionAssistanceTypeIds == null)
                {
                    dto.CompanionAssistanceTypeIds = new List<long>();
                }
                if (!dto.CompanionAssistanceTypeIds.Any())
                {
                    return new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.SelectAtLeastOneType, dto);
                }
                item.CompanionTypeId = dto.CompanionTypeId;
                await (_context as DbContext).Database.ExecuteSqlRawAsync("DELETE FROM CodeCompanionAssistance WHERE CompanionAssistancesId = {0}", item.Id);
                _context.CompanionAssistances.Update(item);
                await _context.SaveChangesAsync();
                dto.CompanionAssistanceTypeIds = dto.CompanionAssistanceTypeIds.Distinct().ToList();
                await _companionAssistanceTypeService.InsertOrUpdateAsync(item, dto.CompanionAssistanceTypeIds);
                await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistanceUpdated, ReferenceType = "CompanionAssistance", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistanceUpdated}:{item.Id}:{DateTime.UtcNow.Ticks}", Metadata = new Dictionary<string, string> { { "companionId", item.CompanionId.ToString() } } });
                return new BaseResultDto<CompanionAssistanceDto>(true, mapper.Map<CompanionAssistanceDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistanceDto>(isSuccess: false, val: ex.Message, data: dto);
            }

        }

        public BaseResultDto ActivationDto(CompanionAssistanceActivationDto dto)
        {
            var item = _context.CompanionAssistances.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (!dto.Active && !dto.Approved)
            {
                item.Active = false;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Active && string.IsNullOrEmpty(dto.ActivationValue))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else if (dto.Active && !dto.Approved)
            {
                item.Active = true;
                item.Approved = false;
                item.ActivationValue = dto.ActivationValue;

                if (!item.Approved && (string.IsNullOrWhiteSpace(dto.ActivationValue) || dto.ActivationValue.Trim().ToLower() == "string"))
                {
                    return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
                }
            }
            else
            {
                item.Active = true;
                item.Approved = true;
                item.ActivationValue = dto.ActivationValue;
            }
            _context.CompanionAssistances.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }
    }
}
