using Application.Common.Dto.Result;
using Application.Common.Helpers;
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
            var model = _context.PansionPets.Include(s => s.Pet).Include(s => s.Pansion).ThenInclude(s => s.Companion).AsQueryable().Where(s => !s.Deleted && !s.Pansion.Deleted);

            if (baseSearchDto.OwnerUserId.HasValue)
            {
                var owned = Application.Services.PansionSrvs.PansionOwnershipSrv.PansionOwnershipService.OwnedPansionIds(_context, baseSearchDto.OwnerUserId.Value, false);
                model = model.Where(s => owned.Contains(s.PansionId));
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.Pansion.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.PansionId.HasValue)
            {
                model = model.Where(s => s.PansionId == baseSearchDto.PansionId.Value);
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
                var error = await ValidateRateAsync(dto);
                if (error != null)
                {
                    return new BaseResultDto<PansionPetDto>(false, error, dto);
                }
                var item = mapper.Map<PansionPet>(dto);
                await _context.PansionPets.AddAsync(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto<PansionPetDto>(true, mapper.Map<PansionPetDto>(item));


            }
            catch (Exception ex)
            {
                return new BaseResultDto<PansionPetDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }

        public async Task<BaseResultDto> UpdateRateAsync(PansionPetDto dto)
        {
            var item = await _context.PansionPets.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }
            dto.PansionId = item.PansionId;
            dto.PetId = item.PetId;
            var error = await ValidateRateAsync(dto);
            if (error != null)
            {
                return new BaseResultDto(false, error);
            }
            item.PetSize = dto.PetSize;
            item.PansionPrice = dto.PansionPrice;
            item.SchoolPrice = dto.SchoolPrice;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // سایز و قیمت را استاندارد می‌کند؛ خروجی متن خطا یا null. هر مرکز برای هر پت/سایز فقط یک ردیف دارد.
        private async Task<string> ValidateRateAsync(PansionPetDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PetSize))
            {
                dto.PetSize = null;
            }
            else
            {
                dto.PetSize = VenuePetSizes.One(dto.PetSize);
                if (dto.PetSize == null)
                {
                    return Resource.Notification.PansionPetSizeInvalid;
                }
            }
            if (dto.PansionPrice < 0 || dto.SchoolPrice < 0)
            {
                return Resource.Notification.PansionPetPriceInvalid;
            }
            dto.PansionPrice = dto.PansionPrice > 0 ? dto.PansionPrice : null;
            dto.SchoolPrice = dto.SchoolPrice > 0 ? dto.SchoolPrice : null;

            var size = dto.PetSize;
            var existed = await _context.PansionPets.AnyAsync(x =>
                x.PansionId == dto.PansionId && x.PetId == dto.PetId && x.PetSize == size && x.Id != dto.Id && !x.Deleted);
            return existed ? Resource.Notification.DuplicateValue : null;
        }
    }
}
