using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.CategorySrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Services.ProductSrvs.BrandSrv
{
    public class BrandService : CommonSrv<Brand, BrandDto>, IBrandService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICategoryService _categoryService;
        private readonly string connectionString;

        public BrandService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICategoryService categoryService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._categoryService = categoryService;
            this.connectionString = config.GetValue<string>(
        "connection");
        }

        public BrandSearchDto Search(BrandInputDto baseSearchDto)

        {
            var model = _context.Brands.Include(s => s.Picture).AsQueryable().Where(s => s.Deleted == false);
            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.CategoryId.HasValue)
            {
                model = model.Where(s => s.Categories.Any(a => a.Id == baseSearchDto.CategoryId.Value));
            }
            if (baseSearchDto.StoreId.HasValue)
            {
                model = model.Where(s => s.Products.Any(a => a.ProductItems.Any(pi => pi.StoreId == baseSearchDto.StoreId)));
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderBy(o => o.Priority);
            }
            switch (baseSearchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                    {
                        model = model.OrderBy(s => s.Priority);
                        break;
                    }
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
                case Common.Enumerable.SortEnum.Name:
                    {
                        model = model.OrderByDescending(s => s.Name);
                        break;
                    }
                default:
                    break;
            }

            return new BrandSearchDto(baseSearchDto, model, mapper);
        }
        public async Task<BaseResultDto<BrandVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Brands.Include(s => s.Picture).Include(s => s.Icon).FirstOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted != true);
            if (item != null)
            {
                return new BaseResultDto<BrandVDto>(true, mapper.Map<BrandVDto>(item));
            }
            return new BaseResultDto<BrandVDto>(false, mapper.Map<BrandVDto>(item));
        }

        public BaseResultDto GetForCategory(string categoryLabel)
        {
            var categoryIds = _categoryService.GetAllParentIds(categoryLabel);
            var model = _context.Brands.Where(s => s.Deleted == false && s.Active && s.Categories.Any(a => categoryIds.Contains(a.Id))).OrderBy(s => s.Priority);
            return new BaseResultDto<List<BrandVDto>>(true, mapper.Map<List<BrandVDto>>(model));
        }
        public async Task<List<SearchBrandDto>> SearchMinAsync(SearchRequestDto request)
        {
            var model = _context.Brands.Include(s => s.Icon).Where(s => s.Deleted == false && s.Active && (s.Name.Contains(request.Q) || s.SecondName.Contains(request.Q))).Take(request.BrandCount).Select(s => new SearchBrandDto { Id = s.Id, Name = s.Name, SecondName = s.SecondName, Icon = mapper.Map<PictureVDto>(s.Icon) });
            return await model.ToListAsync();
        }
    }
}
