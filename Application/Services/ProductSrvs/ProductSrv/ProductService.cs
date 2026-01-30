using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CategorySrv.Iface;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductCategorySrv.Iface;
using Application.Services.ProductSrvs.ProductPictureSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
namespace Application.Services.ProductSrvs.ProductSrv
{
    public class ProductService : IProductService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICategoryService _categoryService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IProductPictureService _productPictureService;
        private readonly string connectionString;

        public ProductService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICategoryService categoryService, IProductCategoryService productCategoryService, IProductPictureService productPictureService)
        {
            this._context = _context;
            this.mapper = mapper;
            this._categoryService = categoryService;
            this._productCategoryService = productCategoryService;
            this._productPictureService = productPictureService;
            this.connectionString = config.GetValue<string>(
            "connection");
        }

        private IQueryable<Product> BaseSaerch(ProductInputDto searchDto)
        {
            var query = _context.Products.Include(s => s.Category).Include(s => s.Brand).Include(s => s.Status).Include(s => s.Type).Include(s => s.Picture).Where(s => !s.Deleted).AsQueryable();
            if (searchDto.IsAdmin)
            {
                query = query.Include(s => s.Stores);
            }
            DateTime now = DateTime.Now;

            if (searchDto.Available == true)
            {
                query = query.Where(s => s.Active && s.StatusId != (long)ProductStatusEnum.ProductStatus_Draft);
            }
            else
            {
                if (searchDto.Active.HasValue)
                {
                    query = query.Where(s => s.Active == searchDto.Active.Value);
                }
            }


            if (searchDto.InStock == true)
            {
                query = query.Where(s => s.Status.Label == ProductStatusEnum.ProductStatus_Available.ToString());
            }
            else if (searchDto.InStock == false)
            {
                query = query.Where(s => s.Status.Label != ProductStatusEnum.ProductStatus_Available.ToString());
            }
            if (searchDto.CreateStoreId.HasValue)
            {
                query = query.Where(s => s.StoreId == searchDto.StoreId);
            }
            if (searchDto.Status.HasValue)
            {
                query = query.Where(s => s.Status.Label == searchDto.Status.ToString());
            }
            if (searchDto.Type.HasValue)
            {
                query = query.Where(s => s.Type.Label == searchDto.Type.ToString());
            }
            if (searchDto.Distance != null && searchDto.Distance.DistanceMeter > 0)
            {
                var point = mapper.Map<Point>(searchDto.Distance);
                query = query.Where(s => s.Location != null && s.Location.Distance(point) < searchDto.Distance.DistanceMeter);
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(searchDto.Q) || s.SecondName.Contains(searchDto.Q));
            }

            if (searchDto.PriceFrom.HasValue)
            {
                query = query.Where(s => s.Price >= searchDto.PriceFrom);
            }
            if (searchDto.PriceTo.HasValue)
            {
                query = query.Where(s => s.Price <= searchDto.PriceTo);
            }
            if (searchDto.NotId.HasValue)
            {
                query = query.Where(s => s.Id != searchDto.NotId);
            }
            if (searchDto.BrandIds != null && searchDto.BrandIds.Any())
            {
                query = query.Where(s => s.BrandId.HasValue && searchDto.BrandIds.Contains(s.BrandId.Value));
            }
            if (searchDto.HasDiscount.HasValue)
            {
                query = query.Where(s => s.DiscountGroupId.HasValue == searchDto.HasDiscount.Value);
            }
            if (!string.IsNullOrEmpty(searchDto.DiscountGroupLabel))
            {
                query = query.Where(s => s.DiscountGroupId.HasValue && (s.DiscountGroup.Label == searchDto.DiscountGroupLabel));
            }
            if (searchDto.CategoryIds != null && searchDto.CategoryIds.Any())
            {
                if (searchDto.IsAndCategories)
                    foreach (var categoryId in searchDto.CategoryIds)
                    {
                        query = query.Where(s => s.Categories.Any(a => a.Id == categoryId));
                    }
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryIds.Contains(x.Id)));

            }
            else if (searchDto.CategoryLabels != null && searchDto.CategoryLabels.Any())
            {
                if (searchDto.IsAndCategories)
                    query = query.Where(s => searchDto.CategoryLabels.All(a => s.Categories.Select(c => c.Label).Contains(a)));
                else
                    query = query.Where(s => s.Categories.Any(x => searchDto.CategoryLabels.Contains(x.Label)));

            }
            if (searchDto.FeatureItemIds != null && searchDto.FeatureItemIds.Any())
            {
                query = query.Where(s => s.ProductFeatureValues.Any(x => x.FeatureItemId.HasValue && searchDto.FeatureItemIds.Any(a => a.Equals(x.FeatureItemId.Value))));

            }
            if (searchDto.StoreId.HasValue)
            {
                query = query.Where(s => s.ProductItems.Any(x => x.StoreId == searchDto.StoreId));

            }
            switch (searchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Name:
                    {
                        query = query.OrderByDescending(s => s.Name);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreVisit:
                    {
                        query = query.OrderByDescending(s => s.VisitCount);

                        break;
                    }
                case Common.Enumerable.SortEnum.LessVisit:
                    {
                        query = query.OrderBy(s => s.VisitCount);
                        break;
                    }
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        query = query.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.MoreSell:
                    {
                        query = query.OrderByDescending(s => s.SellCount);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessSell:
                    {
                        query = query.OrderBy(s => s.SellCount);
                        break;
                    }
                default:
                    break;
            }
            return query;
        }
        public ProductSearchDto Search(ProductInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new ProductSearchDto(baseSearchDto, model, mapper);
        }
        public async Task<BaseResultDto<ProductDto>> FindAsyncDto(long id, long? storeId = null)
        {
            var item = await _context.Products.Include(s => s.ProductPictures).ThenInclude(s => s.Picture).Include(s => s.Categories).Include(s => s.Picture).SingleOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                if (storeId != null)
                {
                    if (item.StoreId != storeId || item.StatusId != (long)ProductStatusEnum.ProductStatus_Draft)
                    {
                        return new BaseResultDto<ProductDto>(false, val: Resource.Notification.AccessDenied, null);

                    }
                }
                return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));

            }
            return new BaseResultDto<ProductDto>(false, null);
        }
        public async Task<BaseResultDto<ProductDto>> InsertAsyncDto(ProductDto dto)
        {
            using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var modelCheker = ModelHelper<ProductDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Variety2Id != null && dto.VarietyId == null)
                    {
                        return new BaseResultDto<ProductDto>(false, Resource.Notification.PleaseSpecifyFirstVariationValue, dto);
                    }
                    if (dto.VarietyId.HasValue && (dto.VarietyId == dto.Variety2Id))
                    {
                        return new BaseResultDto<ProductDto>(false, Resource.Notification.VarietiesShouldBeDifferent, dto);
                    }
                    var item = mapper.Map<Product>(dto);
                    item.VarietyId = dto.VarietyId;
                    item.Variety2Id = dto.Variety2Id;
                    item.CreateDate = DateTime.Now;
                    item.UpdateDate = DateTime.Now;
                    await _context.Products.AddAsync(item);
                    await _context.SaveChangesAsync();

                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = await _categoryService.GetAllParents(dto.CategoryId.Value);
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);
                        }
                    }
                    if (dto.CategoryIds.Any())
                    {
                        dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                        await _productCategoryService.InsertOrUpdateAsync(item, dto.CategoryIds);
                    }

                    //if (dto.ProductPictures.Any())
                    //{
                    //    _productPictureService.InsertOrUpdate(item, dto.ProductPictures);
                    //}
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, item.Id.ToString());

                    return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new BaseResultDto<ProductDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateDtoAsync(ProductDto dto, long? storeId = null)
        {
            using var transaction = await _context.BeginTransactionAsync();

            try
            {
                var modelCheker = ModelHelper<ProductDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = await _context.Products.Include(s => s.Categories).Include(s => s.ProductPictures).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
                    if (storeId != null)
                    {
                        if (item.StoreId != storeId || item.StatusId != (long)ProductStatusEnum.ProductStatus_Draft)
                        {
                            return new BaseResultDto<ProductDto>(false, val: Resource.Notification.AccessDenied, null);

                        }
                    }
                    mapper.Map(dto, item);
                    item.UpdateDate = DateTime.Now;
                    _context.Products.Update(item);
                    await _context.SaveChangesAsync();
                    if (dto.CategoryIds == null)
                    {
                        dto.CategoryIds = new List<long>();
                    }
                    if (dto.CategoryId.HasValue)
                    {
                        var categoryParents = await _categoryService.GetAllParents(dto.CategoryId.Value);
                        foreach (var categoryParent in categoryParents)
                        {
                            dto.CategoryIds.Add(categoryParent.Id);
                        }
                    }
                    dto.CategoryIds = dto.CategoryIds.Distinct().ToList();
                    await _productCategoryService.InsertOrUpdateAsync(item, dto.CategoryIds);



                    _productPictureService.InsertOrUpdate(item, dto.ProductPictures);

                    await _context.SaveChangesAsync();
                    await _context.CommitTransactionAsync();
                    await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, dto.Id.ToString());

                    return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(item));
                }
            }
            catch (Exception ex)
            {
                await _context.RollbackTransactionAsync();
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public BaseResultDto DeleteDto(long id)
        {
            try
            {
                var item = _context.Products.Find(id);
                item.Deleted = true;
                _context.Products.Update(item);
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public async Task<BaseResultDto<ProductVDto>> FindAsyncVDto(long id, bool visit = true)
        {
            var item = await _context.Products.Include(s => s.Category).Include(s => s.Status).Include(s => s.Brand).Include(s => s.Picture).Include(s => s.ProductFiles).Include(s => s.Category).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).ThenInclude(s => s.Parent).SingleOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted != true);
            if (item != null)
            {
                if (visit)
                {
                    item.VisitCount++;
                    _context.Products.Update(item);
                    await _context.SaveChangesAsync();
                }
                return new BaseResultDto<ProductVDto>(true, mapper.Map<ProductVDto>(item));
            }
            return new BaseResultDto<ProductVDto>(false, mapper.Map<ProductVDto>(item));
        }
        public async Task<Product> GetByIdAsync(long id)
        {
            return await _context.Products.AsTracking().Include(s => s.Variety).Include(s => s.Type).Include(s => s.Status).FirstOrDefaultAsync(s => s.Id == id && s.Deleted == false);
        }
        public async Task IncreaseSellCountAsync(ProductOrder order)
        {
            try
            {
                var productIds = new List<long>();
                foreach (var itemStore in order.ProductOrderStores)
                {
                    foreach (var item in itemStore.ProductOrderItems)
                    {
                        item.ProductItem.Quantity -= item.Count;
                        if (item.ProductItem.Quantity < 0)
                        {
                            item.ProductItem.Quantity = 0;
                        }
                        _context.ProductItems.Update(item.ProductItem);
                        productIds.Add(item.ProductItem.ProductId);
                        var product = item.ProductItem.Product;
                        product.SellCount += item.Count;
                        _context.Products.Update(product);
                    }
                }
                await _context.SaveChangesAsync();
                string productIdsString = string.Join(",", productIds.Distinct());
                await UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, productIdsString);
            }
            catch { }

        }
        public BaseResultDto GetSiteMap()
        {
            string sqlQuery = $"SELECT p.Id, p.Name,p.UpdateDate, c.Label As CategoryName FROM Products p LEFT JOIN Categories c ON p.CategoryId = c.Id WHERE p.Active = 1 and p.Deleted=0";
            //var list = _context.Posts.Include(s => s.Category).Where(s => s.Deleted == false && s.Active && s.AdminConfirm == true).Select(s => new PostSiteMapDto() { Id = s.Id, Name = s.Name, CategoryName = s.Category.Label,UpdateDate=s.PublishDate }).ToList();
            var connection = new SqlConnection(connectionString);
            var posts = connection.Query<ProductSiteMapDto>(sqlQuery).ToList();
            return new BaseResultDto<List<ProductSiteMapDto>>(true, posts);
        }
        public async Task UpdateProductPriceAsync(ProductUpdateTypeEnum productUpdateType, string Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateProductItemDiscount", new { FilterType = productUpdateType.ToString(), FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
        public async Task<List<SearchProductDto>> SearchMinAsync(SearchRequestDto request)
        {
            var connection = new SqlConnection(connectionString);
            if (!string.IsNullOrEmpty(request.Q))
            {
                request.Q = request.Q.Replace("%", "").Replace("ي", "ی").Replace("ك", "ک");
            }
            var parameters = new { ProductCount = request.ProductCount, ProductNotId = request.ProductNotId };
            var query = $@"
DECLARE @Keywords TABLE (Keyword NVARCHAR(255));


INSERT INTO @Keywords (Keyword)
SELECT value
FROM STRING_SPLIT(N'{request.Q}', ' ');

SELECT TOP(@ProductCount)
    p.Id, 
    p.Name,
p.BasePrice,
p.Price,
p.DiscountPercent,
    picture.Id AS PictureId,
    picture.Url + '/' + picture.Name AS Url,
    picture.Url AS BaseUrl,
    picture.Name,
    picture.GuidName,
    picture.Extension,
    p.CategoryId,
    p.BrandId,
    c.Label AS CategoryName,
    ISNULL(br.Name, '') AS BrandName,
    (
        SELECT COUNT(*)
        FROM @Keywords k
        WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
           OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
           OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
           OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
    ) AS MatchScore
FROM products p
LEFT JOIN pictures picture ON p.PictureId = picture.Id
LEFT JOIN categories c ON p.CategoryId = c.Id
LEFT JOIN Brands br ON p.BrandId = br.Id
WHERE 
    p.active = 1 
    AND p.deleted = 0 
    AND (p.Id != @ProductNotId OR @ProductNotId IS NULL)
    AND (
        p.Name COLLATE Persian_100_CI_AS LIKE '%' + N'{request.Q}' + '%' 
       OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + N'{request.Q}' + '%' 
       OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + N'{request.Q}' + '%' 
       OR EXISTS (
           SELECT 1
           FROM @Keywords k
           WHERE p.Name COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
              OR ISNULL(p.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
              OR ISNULL(br.Name, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
              OR ISNULL(br.SecondName, '') COLLATE Persian_100_CI_AS LIKE '%' + k.Keyword + '%' 
       )
    )
ORDER BY p.StatusId DESC, MatchScore DESC;";
            var result = await connection.QueryAsync<SearchProductDto, PictureVDto, string, string, SearchProductDto>(
                query,
             (product, picture, CategoryName, BrandName) =>
             {

                 product.Picture = picture;
                 product.CategoryName = CategoryName;
                 product.BrandName = BrandName;

                 return product;
             },
            parameters,
            commandType: CommandType.Text,
            splitOn: "PictureId,CategoryName,BrandName");
            return result.ToList();
        }
        public async Task<BaseResultDto<ProductDto>> DuplicateAsyncDto(ProductDuplicateDto productDuplicateDto)
        {
            var orgProduct = await _context.Products.Include(s => s.ProductPictures).Include(s => s.Categories).Include(s => s.Picture).Include(s => s.ProductFeatureValues).SingleOrDefaultAsync(s => s.Id == productDuplicateDto.ProductId);

            if (orgProduct == null)
            {
                return new BaseResultDto<ProductDto>(false, mapper.Map<ProductDto>(productDuplicateDto));
            }

            using var transaction = await _context.BeginTransactionAsync();
            var newProduct = new Product()
            {
                StoreId = productDuplicateDto.StoreId,
                Name = orgProduct.Name,
                ProductLabel = orgProduct.ProductLabel,
                SecondName = orgProduct.SecondName,
                CategoryId = orgProduct.CategoryId,
                BrandId = orgProduct.BrandId,
                Summary = orgProduct.Summary,
                Description = orgProduct.Description,
                Price = orgProduct.Price,
                BasePrice = orgProduct.BasePrice,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                StatusId = orgProduct.StatusId,
                TypeId = orgProduct.TypeId,
                VarietyId = orgProduct.VarietyId,
                Variety2Id = orgProduct.Variety2Id,
                SeoNoIndex = orgProduct.SeoNoIndex,
                SeoNoFollow = orgProduct.SeoNoFollow,
                Active = orgProduct.Active,
                SeoH1 = orgProduct.SeoH1,
                SeoMinDescription = orgProduct.SeoMinDescription,
                SeoDescription = orgProduct.SeoDescription,
                SeoTitle = orgProduct.SeoTitle,
                SeoPictureAlt = orgProduct.SeoPictureAlt,
                SeoUrlText = orgProduct.SeoUrlText,
                SeoCanonical = orgProduct.SeoCanonical,
                SellLimitCount = orgProduct.SellLimitCount,
                DiscountGroup = orgProduct.DiscountGroup,
                DiscountEndDate = orgProduct.DiscountEndDate,
                DiscountPercent = orgProduct.DiscountPercent,
                CodeValue = productDuplicateDto.CodeValue,
            };
            if (productDuplicateDto.DuplicatePicture)
            {
                newProduct.PictureId = orgProduct.PictureId;
            }
            if (productDuplicateDto.DuplicateProductPictures)
            {
                newProduct.ProductPictures = new List<ProductPicture>();

                foreach (var pp in orgProduct.ProductPictures)
                {
                    var newProductPicure = new ProductPicture();

                    newProductPicure.PictureId = pp.PictureId;
                    newProductPicure.Label = pp.Label;
                    newProduct.ProductPictures.Add(newProductPicure);

                }
            }
            if (productDuplicateDto.DuplicateProductFeatureValues)
            {
                newProduct.ProductFeatureValues = new List<ProductFeatureValue>();
                foreach (var pp in orgProduct.ProductFeatureValues)
                {
                    var newfeatureValue = new ProductFeatureValue();

                    newfeatureValue.FeatureId = pp.FeatureId;
                    newfeatureValue.FeatureItemId = pp?.FeatureItemId;
                    newfeatureValue.Name = pp?.Name;
                    newProduct.ProductFeatureValues.Add(newfeatureValue);
                }
            }
            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            var categoryIds = orgProduct.Categories.Select(c => c.Id).ToList();
            if (categoryIds.Any())
            {
                await _productCategoryService.InsertOrUpdateAsync(newProduct, categoryIds);
            }

            await transaction.CommitAsync();

            return new BaseResultDto<ProductDto>(true, mapper.Map<ProductDto>(newProduct));
        }
        public async Task<BaseResultDto> ChangeProductVarietiesAsync(ProductDto product)
        {
            if (product.Variety2Id != null && product.VarietyId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSpecifyFirstVariationValue);
            }
            if (product.VarietyId.HasValue && (product.VarietyId == product.Variety2Id))
            {
                return new BaseResultDto(false, Resource.Notification.VarietiesShouldBeDifferent);
            }
            var productEntity = await _context.Products.FindAsync(product.Id);
            if (productEntity != null)
            {
                if (product.VarietyId != productEntity.VarietyId || product.Variety2Id != productEntity.Variety2Id)
                {
                    productEntity.VarietyId = product.VarietyId;
                    productEntity.Variety2Id = product.Variety2Id;
                    _context.Products.Update(productEntity);
                    await _context.SaveChangesAsync();
                    await _context.ProductItems.Where(s => s.ProductId == product.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Deleted, true).SetProperty(a => a.Active, false).SetProperty(a => a.SystemActive, false));
                    return new BaseResultDto(true, Resource.Notification.SuccessfullyCompletedPreviousVariationsWereRemoved);
                }
            }
            return new BaseResultDto(false, Resource.Notification.Unsuccess);

        }
    }
}
