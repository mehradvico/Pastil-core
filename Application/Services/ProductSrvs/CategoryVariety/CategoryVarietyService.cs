using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.CategoryVariety.Dto;
using Application.Services.ProductSrvs.CategoryVariety.Iface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.CategoryVariety
{
    public class CategoryVarietyService : ICategoryVarietyService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CategoryVarietyService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto> FindAsyncDto(long id)
        {
            var categoryVariety = await _context.Categories.Include(s => s.Varieties).FirstOrDefaultAsync(s => s.Id.Equals(id));
            if (categoryVariety != null)
            {
                return new BaseResultDto<CategoryVarietyDto>(true, mapper.Map<CategoryVarietyDto>(categoryVariety));
            }
            return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);
        }

        public async Task<BaseResultDto> InsertAndUpdateAsyncDto(CategoryVarietyDto dto)
        {
            try
            {
                if (dto != null)
                {
                    var category = await _context.Categories.AsTracking().Include(s => s.Varieties).FirstOrDefaultAsync(s => s.Id.Equals(dto.CategoryDto.Id));
                    if (category != null)
                    {
                        foreach (var variety in category.Varieties)
                        {
                            if (!dto.VarietyDto.Any(s => s.Id == variety.Id))
                            {
                                category.Varieties.Remove(variety);
                            }
                        }
                        foreach (var varietyDto in dto.VarietyDto)
                        {
                            if (!category.Varieties.Any(s => s.Id == varietyDto.Id))
                            {
                                var varietyEntity = _context.Varieties.FirstOrDefault(s => s.Id == varietyDto.Id);
                                if (varietyEntity != null)
                                    category.Varieties.Add(varietyEntity);
                            }
                        }
                        await _context.SaveChangesAsync();
                        return new BaseResultDto(true);
                    }
                }
                return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);

            }
            catch (Exception e)
            {
                return new BaseResultDto(false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(e));
            }
        }
    }
}
