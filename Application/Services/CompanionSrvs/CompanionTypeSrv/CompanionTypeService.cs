using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionTypeSrv
{
    public class CompanionTypeService : CommonSrv<CompanionType, CompanionTypeDto>, ICompanionTypeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionTypeService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<CompanionTypeDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionTypes.Include(s => s.Companion).Include(s => s.Type).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionTypeDto>(true, mapper.Map<CompanionTypeDto>(item));
            return new BaseResultDto<CompanionTypeDto>(false, mapper.Map<CompanionTypeDto>(item));
        }

        public async Task<BaseResultDto<CompanionTypeVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionTypes.Include(s => s.Companion).Include(s => s.Type).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionTypeVDto>(true, mapper.Map<CompanionTypeVDto>(item));
            return new BaseResultDto<CompanionTypeVDto>(false, mapper.Map<CompanionTypeVDto>(item));
        }

        public CompanionTypeSearchDto Search(CompanionTypeInputDto baseSearchDto)
        {
            var model = _context.CompanionTypes.Include(s => s.Type).Include(s => s.Companion).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.TypeId == baseSearchDto.TypeId.Value);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
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
            }
            return new CompanionTypeSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionTypeDto>> InsertAsyncDto(CompanionTypeDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionTypeDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionType>(dto);
                    bool existed = await _context.CompanionTypes.AnyAsync(x => x.CompanionId == dto.CompanionId && x.TypeId == dto.TypeId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionTypeDto>(false, Resource.Notification.DuplicateValue, dto);
                    }
                    await _context.CompanionTypes.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<CompanionTypeDto>(true, mapper.Map<CompanionTypeDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionTypeDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public override BaseResultDto UpdateDto(CompanionTypeDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionTypeDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionType>(dto);
                    bool existed = _context.CompanionTypes.Any(x => x.CompanionId == dto.CompanionId && x.TypeId == dto.TypeId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionTypeDto>(false, Resource.Notification.DuplicateValue, dto);
                    }
                    _context.CompanionTypes.Attach(item);
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
    }
}
