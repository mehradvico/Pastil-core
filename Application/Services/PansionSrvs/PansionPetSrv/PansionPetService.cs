using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Iface;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPetSrv
{
    public class PansionPetService : CommonSrv<PansionPet, PansionPetDto>, IPansionPetService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PansionPetService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<PansionPetVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PansionPets.Include(s => s.Pet).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<PansionPetVDto>(true, mapper.Map<PansionPetVDto>(item));
            }
            return new BaseResultDto<PansionPetVDto>(false, mapper.Map<PansionPetVDto>(item));
        }

        public PansionPetSearchDto Search(PansionPetInputDto baseSearchDto)
        {
            var model = _context.PansionPets.Include(s => s.Pet).Include(s => s.Pansion).ThenInclude(s => s.Companion).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.Pansion.CompanionId == baseSearchDto.CompanionId.Value);
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
            return new PansionPetSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PansionPetDto>> InsertAsyncDto(PansionPetDto dto)
        {
            try
            {
                var item = mapper.Map<PansionPet>(dto);
                bool existed = await _context.PansionPets.AnyAsync(x => x.PansionId == dto.PansionId && x.PetId == dto.PetId && !x.Deleted);
                if (existed)
                {
                    return new BaseResultDto<PansionPetDto>(false, Resource.Notification.DuplicateValue, dto);
                }
                await _context.PansionPets.AddAsync(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto<PansionPetDto>(true, mapper.Map<PansionPetDto>(item));


            }
            catch (Exception ex)
            {
                return new BaseResultDto<PansionPetDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
    }
}
