using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv
{
    public class CompanionAssistancePackageOnlineSelectionService : CommonSrv<CompanionAssistancePackageOnlineSelection, CompanionAssistancePackageOnlineSelectionDto>, ICompanionAssistancePackageOnlineSelectionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionAssistancePackageOnlineSelectionService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<CompanionAssistancePackageOnlineSelectionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistancePackageOnlineSelections
                .Include(s => s.CompanionAssistancePackage).ThenInclude(s => s.CompanionAssistance)
                .Include(s => s.CompanionAssistancePackageOnline)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistancePackageOnlineSelectionVDto>(true, mapper.Map<CompanionAssistancePackageOnlineSelectionVDto>(item));
            }
            return new BaseResultDto<CompanionAssistancePackageOnlineSelectionVDto>(false, mapper.Map<CompanionAssistancePackageOnlineSelectionVDto>(item));
        }

        public CompanionAssistancePackageOnlineSelectionSearchDto Search(CompanionAssistancePackageOnlineSelectionInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistancePackageOnlineSelections
                .Include(s => s.CompanionAssistancePackage).ThenInclude(s => s.CompanionAssistance)
                .Include(s => s.CompanionAssistancePackageOnline)
                .AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.CompanionAssistancePackageId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistancePackageId == baseSearchDto.CompanionAssistancePackageId.Value);
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
            return new CompanionAssistancePackageOnlineSelectionSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>> InsertAsyncDto(CompanionAssistancePackageOnlineSelectionDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistancePackageOnlineSelectionDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                bool exists = await _context.CompanionAssistancePackageOnlineSelections.AnyAsync(a =>
                    a.CompanionAssistancePackageId == dto.CompanionAssistancePackageId &&
                    a.CompanionAssistancePackageOnlineId == dto.CompanionAssistancePackageOnlineId &&
                    !a.Deleted);

                if (exists)
                {
                    return new BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<CompanionAssistancePackageOnlineSelection>(dto);
                await _context.CompanionAssistancePackageOnlineSelections.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>(true, mapper.Map<CompanionAssistancePackageOnlineSelectionDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public BaseResultDto ActivationDto(CompanionAssistancePackageOnlineSelectionActivationDto dto)
        {
            var item = _context.CompanionAssistancePackageOnlineSelections.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (!dto.Active && string.IsNullOrEmpty(dto.ActivationValue))
            {
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);
            }

            item.Active = dto.Active;
            item.ActivationValue = dto.ActivationValue;
            _context.CompanionAssistancePackageOnlineSelections.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }
    }
}
