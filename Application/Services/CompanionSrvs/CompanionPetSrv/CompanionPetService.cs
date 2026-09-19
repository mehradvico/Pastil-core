using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionPetSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;


namespace Application.Services.CompanionSrvs.CompanionPetSrv
{
    public class CompanionPetService : CommonSrv<CompanionPet, CompanionPetDto>, ICompanionPetService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionPetService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<CompanionPetDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionPets.Include(s => s.Companion).Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionPetDto>(true, mapper.Map<CompanionPetDto>(item));
            return new BaseResultDto<CompanionPetDto>(false, mapper.Map<CompanionPetDto>(item));
        }

        public async Task<BaseResultDto<CompanionPetVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionPets.Include(s => s.Companion).Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionPetVDto>(true, mapper.Map<CompanionPetVDto>(item));
            return new BaseResultDto<CompanionPetVDto>(false, mapper.Map<CompanionPetVDto>(item));
        }

        public CompanionPetSearchDto Search(CompanionPetInputDto baseSearchDto)
        {
            var model = _context.CompanionPets.Include(s => s.Pet).Include(s => s.Companion).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.PetId == baseSearchDto.PetId.Value);
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
            return new CompanionPetSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionPetDto>> InsertAsyncDto(CompanionPetDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionPetDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionPet>(dto);
                    bool existed = await _context.CompanionPets.AnyAsync(x => x.CompanionId == dto.CompanionId && x.PetId == dto.PetId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionPetDto>(false, Resource.Notification.DuplicateValue, dto);
                    }
                    await _context.CompanionPets.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<CompanionPetDto>(true, mapper.Map<CompanionPetDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionPetDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }

        public override BaseResultDto UpdateDto(CompanionPetDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionPetDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionPet>(dto);
                    bool existed = _context.CompanionPets.Any(x => x.CompanionId == dto.CompanionId && x.PetId == dto.PetId && !x.Deleted);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionPetDto>(false, Resource.Notification.DuplicateValue, dto);
                    }
                    _context.CompanionPets.Attach(item);
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }
    }
}
