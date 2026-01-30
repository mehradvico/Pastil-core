using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CategorySrv.Dto;
using Application.Services.CategorySrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CategorySrv
{
    public class CategoryService : CommonSrv<Category, CategoryDto>, ICategoryService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IQueryable<Category> _baseQuery;
        private readonly string connectionString;

        public CategoryService(IDataBaseContext _context, IMapper mapper, IConfiguration config) : base(_context: _context, mapper: mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._baseQuery = _context.Categories.Where(s => !s.Deleted);
            this.connectionString = config.GetValue<string>(
                "connection");
        }
        public override async Task<BaseResultDto<CategoryDto>> InsertAsyncDto(CategoryDto dto)
        {
            var modelCheker = ModelHelper<CategoryDto>.ModelErrors(dto);
            if (!modelCheker.IsSuccess)
            {
                return modelCheker;
            }
            else
            {
                var errors = new List<Tuple<string, string>>();
                dto.Label = dto.Label.ToStandardUrl();
                if (!LabelIsUnique(dto.Label))
                {
                    errors.Add(new Tuple<string, string>(Resource.Notification.TheLabelIsDuplicate, nameof(dto.Label)));
                }
                if (errors.Any())
                {
                    return new BaseResultDto<CategoryDto>(isSuccess: false, messages: errors, dto);
                }

                return await base.InsertAsyncDto(dto);
            }
        }
        public override BaseResultDto UpdateDto(CategoryDto dto)
        {
            var modelCheker = ModelHelper<CategoryDto>.ModelErrors(dto);
            if (!modelCheker.IsSuccess)
            {
                return modelCheker;
            }
            else
            {
                var errors = new List<Tuple<string, string>>();
                dto.Label = dto.Label.ToStandardUrl();
                if (!LabelIsUnique(dto.Label, dto.Id))
                {
                    errors.Add(new Tuple<string, string>(Resource.Notification.TheLabelIsDuplicate, nameof(dto.Label)));
                }
                if (errors.Any())
                {
                    return new BaseResultDto<CategoryDto>(isSuccess: false, messages: errors, dto);
                }
                return base.UpdateDto(dto);
            }
        }
        public CategorySearchDto Search(CategoryInputDto baseInputDto)
        {
            var model = _baseQuery.Include(s => s.Picture).Include(s => s.Icon).Include(s => s.Children.Where(s => s.Active && s.Deleted == false)).AsQueryable();
            //if (baseInputDto.ParentId.HasValue)
            //{
            //    var ids = GetAllChildrenIds(baseInputDto.ParentId.Value);
            //    model = model.Where(s =>ids.Contains(s.Id));

            //}
            model = model.Where(s => s.ParentId == baseInputDto.ParentId);

            if (baseInputDto.Available == true)
            {
                model = model.Where(s => s.Deleted == false && s.Active);
            }
            else
            {
                if (baseInputDto.Active != null)
                {
                    model = model.Where(s => s.Active == baseInputDto.Active);
                }
            }
            if (!string.IsNullOrEmpty(baseInputDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseInputDto.Q));
            }
            model = Sorting(model, baseInputDto.SortBy);
            return new CategorySearchDto(baseInputDto, model, mapper);
        }
        public async Task<BaseResultDto<CategoryVDto>> FindVDtoAsync(long id, bool? active = null)
        {
            var item = await _baseQuery
              .Include(s => s.Picture)
                  .Include(s => s.Icon).FirstOrDefaultAsync(s => s.Id == id && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<CategoryVDto>(true, mapper.Map<CategoryVDto>(item));
            }
            return new BaseResultDto<CategoryVDto>(false, mapper.Map<CategoryVDto>(item));
        }
        public async Task<BaseResultDto<CategoryParentVDto>> FindParentVDtoAsync(long id, bool? active = null)
        {
            var item = await _baseQuery
            .Include(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent)
           .Include(s => s.Picture)
               .Include(s => s.Icon).FirstOrDefaultAsync(s => s.Id == id && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<CategoryParentVDto>(true, mapper.Map<CategoryParentVDto>(item));
            }
            return new BaseResultDto<CategoryParentVDto>(false, mapper.Map<CategoryParentVDto>(item));
        }
        public async Task<BaseResultDto<CategoryChildrenVDto>> FindChildrenVDtoAsync(long id, bool? active = null)
        {
            var item = await _baseQuery.Include(s => s.Picture)
                           .Include(s => s.Icon)
            .Include(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children)
                       .FirstOrDefaultAsync(s => s.Id == id && s.Active == true && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<CategoryChildrenVDto>(true, mapper.Map<CategoryChildrenVDto>(item));
            }
            return new BaseResultDto<CategoryChildrenVDto>(false, mapper.Map<CategoryChildrenVDto>(item));
        }
        public async Task<BaseResultDto<CategoryChildrenVDto>> FindChildrenVDtoAsync(string label, bool? active = null)
        {
            var item = await _baseQuery.Include(s => s.Picture)
                           .Include(s => s.Icon)
            .Include(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children)
                       .FirstOrDefaultAsync(s => s.Label == label && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<CategoryChildrenVDto>(true, mapper.Map<CategoryChildrenVDto>(item));
            }
            return new BaseResultDto<CategoryChildrenVDto>(false, mapper.Map<CategoryChildrenVDto>(item));
        }
        public async Task<BaseResultDto<CategoryCompleteVDto>> FindCompleteVDtoAsync(long id, bool? active = null)
        {
            var item = await _baseQuery
            .Include(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent)
            .Include(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children)
            .Include(s => s.Picture)
                .Include(s => s.Icon).FirstOrDefaultAsync(s => s.Id == id && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<CategoryCompleteVDto>(true, mapper.Map<CategoryCompleteVDto>(item));
            }
            return new BaseResultDto<CategoryCompleteVDto>(false, mapper.Map<CategoryCompleteVDto>(item));
        }
        bool LabelIsUnique(string label, long? categoryId = null)
        {
            var item = GetCategoryByLabel(label);
            if (item != null && item.Id == categoryId)
                return true;
            else if (item == null)
                return true;
            else
                return false;
        }
        Category GetCategoryByLabel(string label, bool? active = null)
        {
            return _baseQuery.FirstOrDefault(s => s.Label == label && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
        }
        internal virtual IQueryable<Category> Sorting(IQueryable<Category> query, SortEnum sortBy)
        {
            switch (sortBy)
            {
                case SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }
                case SortEnum.Name:
                    {
                        query = query.OrderBy(s => s.Name);
                        break;
                    }
                case SortEnum.MorePriority:
                    {
                        query = query.OrderByDescending(s => s.Priority);
                        break;
                    }
                case SortEnum.LessPriority:
                    {
                        query = query.OrderBy(s => s.Priority);
                        break;
                    }

                default:
                    break;
            }
            return query;
        }

        public Category Find(long id, bool? active = null)
        {
            return _baseQuery.FirstOrDefault(s => s.Id == id && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
        }
        public List<long> GetAllParentIds(long id)
        {
            string sqlQuery = $"WITH ParentCategories AS(SELECT Id, ParentId, Name FROM Categories WHERE Id = {id} UNION ALL SELECT c.Id, c.ParentId, c.Name FROM Categories c INNER JOIN ParentCategories pc ON c.Id = pc.ParentId) SELECT Id FROM ParentCategories";
            var connection = new SqlConnection(connectionString);
            var categoryIds = connection.Query<long>(sqlQuery).ToList();
            return categoryIds;
        }
        public List<long> GetAllParentIds(string label)
        {
            string sqlQuery = $"WITH ParentCategories AS(SELECT Id, ParentId, Name FROM Categories WHERE Label = {label} UNION ALL SELECT c.Id, c.ParentId, c.Name FROM Categories c INNER JOIN ParentCategories pc ON c.Id = pc.ParentId) SELECT Id FROM ParentCategories";
            var connection = new SqlConnection(connectionString);
            var categoryIds = connection.Query<long>(sqlQuery).ToList();
            return categoryIds;
        }
        public async Task<List<CategoryVDto>> GetAllParents(long id, bool? active = null)
        {

            var category = await _baseQuery.Include(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).FirstOrDefaultAsync(s => s.Id == id && (active.HasValue ? s.Active == active : true) && s.Deleted == false);
            var result = new List<CategoryVDto>();
            result.Add(mapper.Map<CategoryVDto>(category));
            while (category.Parent != null)
            {
                result.Add(mapper.Map<CategoryVDto>(category.Parent));
                category = category.Parent;
            }
            result.Reverse();
            return result;


        }
        public List<long> GetAllChildrenIds(string label)
        {
            string sqlQuery = $"WITH Recursives AS(SELECT id, name, parentID ,Active,Deleted FROM Categories WHERE label = '{label}' And Active=1 And Deleted=0  UNION ALL SELECT t.id, t.name, t.parentID,t.Active,t.Deleted FROM Categories t INNER JOIN Recursives r ON t.parentID = r.id)SELECT id FROM Recursives where Active=1 And Deleted=0";
            var connection = new SqlConnection(connectionString);
            var categoryIds = connection.Query<long>(sqlQuery).ToList();
            return categoryIds;
        }
        public List<long> GetAllChildrenIds(long id)
        {
            string sqlQuery = $"WITH Recursives AS(SELECT id, name, parentID ,Active,Deleted FROM Categories WHERE id = '{id}' And Active=1 And Deleted=0  UNION ALL SELECT t.id, t.name, t.parentID,t.Active,t.Deleted FROM Categories t INNER JOIN Recursives r ON t.parentID = r.id)SELECT id FROM Recursives where Active=1 And Deleted=0";
            var connection = new SqlConnection(connectionString);
            var categoryIds = connection.Query<long>(sqlQuery).ToList();
            return categoryIds;
        }
        public BaseResultDto GetSiteMap(string label)
        {
            var list = GetCat(label);
            return new BaseResultDto<List<CategorySiteMapDto>>(true, list);
        }
        private List<CategorySiteMapDto> GetCat(string categoryLabel)
        {
            var categoryids = string.Join(',', GetAllChildrenIds(categoryLabel));
            //var categories = _context.Categories.Where(s => categoryids.Contains(s.Id)).Select(s=>new CategorySiteMapDto() { Id=s.Id,Label=s.Label,Name=s.Name,UpdateDate=s.UpdateDate}).ToList();
            string sqlQuery = $"SELECT TOP (1000) [Id],[Label],[Name],[UpdateDate] FROM [Categories] where Id in ({categoryids})";
            var connection = new SqlConnection(connectionString);
            var categories = connection.Query<CategorySiteMapDto>(sqlQuery).ToList();
            return categories;
        }

        public async Task<BaseResultDto<CategoryChildrenMinVDto>> GetTreeAsync(long parentId, string lang = null)
        {
            var categoryTree = new CategoryChildrenMinVDto();
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
WITH CategoryTree AS (
    -- بخش اولیه: انتخاب دسته‌بندی اولیه
    SELECT 
        ca.Id, 
        ca.Name,
        ca.Label, 
        ca.ParentId, 
        ca.Priority, 
        ca.IconId
    FROM Categories ca 
    WHERE ca.Id = @parentId 
      AND ca.Active = 1 AND ca.Deleted = 0

    UNION ALL

    -- بخش بازگشتی: انتخاب دسته‌بندی‌های فرزند
    SELECT 
        c.Id, 
        c.Name,
        c.Label, 
        c.ParentId, 
        c.Priority, 
        c.IconId
    FROM Categories c
    INNER JOIN CategoryTree ct ON c.ParentId = ct.Id  -- استفاده از INNER JOIN
    WHERE c.Active = 1 AND c.Deleted = 0
)
-- پس از انتخاب تمام دسته‌بندی‌ها، join کردن با CategorySeoFieldLang و Pictures
SELECT 
    ct.Id, 
    COALESCE(sfl.Name, ct.Name) AS Name,  -- اگر نام در SeoFieldLangs موجود نبود، از نام خود Category استفاده کن
    ct.Label, 
    ct.ParentId, 
    ct.Priority, 
    COALESCE(CONCAT(p.Url, '/', p.Name), NULL) AS IconUrl, 
    ct.IconId
FROM CategoryTree ct
LEFT JOIN Pictures p ON p.Id = ct.IconId
LEFT JOIN CategorySeoFieldLang csfl ON csfl.CategoriesId = ct.Id
LEFT JOIN SeoFieldLangs sfl ON sfl.Id = csfl.SeoFieldLangsId AND sfl.LanguageId = (SELECT Id FROM Language WHERE Label = @lang)  -- فقط رکوردهای زبان مشخص
WHERE ct.Id IS NOT NULL;  -- اطمینان از وجود رکوردها

        ";

                var categories = (await connection.QueryAsync<CategoryChildrenMinVDto>(query, new { parentId, lang })).ToList();
                categoryTree = BuildTree(categories, parentId);
            }
            return new BaseResultDto<CategoryChildrenMinVDto>(true, categoryTree);
        }

        private CategoryChildrenMinVDto BuildTree(List<CategoryChildrenMinVDto> categories, long parentId)
        {
            var parentCategory = categories.SingleOrDefault(c => c.Id == parentId);
            if (parentCategory != null)
            {
                AddChildrenToParent(parentCategory, categories);
            }
            return parentCategory;
        }

        private void AddChildrenToParent(CategoryChildrenMinVDto parentCategory, List<CategoryChildrenMinVDto> categories)
        {
            var children = categories.Where(c => c.ParentId == parentCategory.Id).ToList();
            foreach (var child in children)
            {
                parentCategory.Children.Add(child);
                AddChildrenToParent(child, categories);
            }
        }
        public async Task<List<SearchCategoryDto>> SearchMinAsync(SearchRequestDto request)
        {
            var model = _context.Categories.Include(s => s.Icon).Where(s => s.Deleted == false && s.Active && (s.Name.Contains(request.Q))).Take(request.CategoryCount).Select(s => new SearchCategoryDto { Id = s.Id, Name = s.Name, Label = s.Label, Icon = mapper.Map<PictureVDto>(s.Icon) });
            return await model.ToListAsync();
        }
        public async Task<BaseResultDto<List<SearchCategoryDto>>> GetAllActiveParents(long categoryId)
        {
            var item = await _context.Categories.Include(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).FirstOrDefaultAsync(s => s.Id == categoryId && s.Active && s.Deleted == false);
            var result = new List<SearchCategoryDto>();
            if (item != null)
            {
                result.Add(mapper.Map<SearchCategoryDto>(item));
                do
                {
                    item = item.Parent;
                    if (item != null)
                    {
                        result.Add(mapper.Map<SearchCategoryDto>(item));
                    }
                    else
                    {
                        break;
                    }
                } while (true);
            }
            result.Reverse();
            return new BaseResultDto<List<SearchCategoryDto>>(true, result);

        }

        public async Task<BaseResultDto<List<long>>> CategoryStore(long storeId)
        {
            string sqlQuery = $"Select Distinct(categoriesid) From CategoryProduct cp  left join products p on cp.ProductsId=p.Id left join ProductItems pii on pii.ProductId=p.Id  where pii.StoreId={storeId}";
            var connection = new SqlConnection(connectionString);
            var result = await connection.QueryAsync<long>(sqlQuery);
            return new BaseResultDto<List<long>>(true, result.ToList());
        }
    }
}
