using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.BrandCategorySrv.Dto;
using Application.Services.ProductSrvs.BrandCategorySrv.Iface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.BrandCategorySrv
{
    public class BrandCategoryService : IBrandCategoryService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public BrandCategoryService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto> FindAsyncDto(long id)
        {
            var brandCategory = await _context.Brands.Include(s => s.Categories).FirstOrDefaultAsync(s => s.Id.Equals(id));
            if (brandCategory != null)
            {
                return new BaseResultDto<BrandCategoryDto>(true, mapper.Map<BrandCategoryDto>(brandCategory));
            }
            return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);
        }

        public async Task<BaseResultDto> InsertAndUpdateAsyncDto(BrandCategoryDto dto)
        {
            try
            {
                if (dto != null)
                {
                    var brand = await _context.Brands.AsTracking().Include(s => s.Categories).FirstOrDefaultAsync(s => s.Id.Equals(dto.BrandDto.Id));
                    if (brand != null)
                    {
                        foreach (var category in brand.Categories)
                        {
                            if (!dto.CategoryDto.Any(s => s.Id == category.Id))
                            {
                                brand.Categories.Remove(category);
                            }
                        }
                        foreach (var categoryDto in dto.CategoryDto)
                        {
                            if (!brand.Categories.Any(s => s.Id == categoryDto.Id))
                            {
                                var categoryEntity = _context.Categories.FirstOrDefault(s => s.Id == categoryDto.Id);
                                if (categoryEntity != null)
                                    brand.Categories.Add(categoryEntity);
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
