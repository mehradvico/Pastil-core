using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت فروشگاه ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductItemController : ControllerBase
    {
        private readonly IProductItemService _productItemService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت فروشگاه ها
        /// </summary>
        ///
        public ProductItemController(IProductItemService productItemService, ICurrentUserHelper currentUser)
        {
            this._productItemService = productItemService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductItemSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductItemInputDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            dto.Available = true;
            var ProductItem = _productItemService.SearchDto(dto);
            return Ok(ProductItem);
        }
        /// <summary>
        /// همه تنوع ها با شناسه محصول
        /// </summary>
        [HttpGet("id")]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemDto>), 200)]
        public async Task<IActionResult> Get(long id, [FromQuery] string varietyItemIds = null, [FromQuery] string variety2ItemIds = null)
        {
            var dto = new ProductItemListRequestDto();
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            dto.ProductId = id;
            // اختیاری (لیست جداشده با کاما): فقط سطرهای مقدارهای انتخاب‌شده + آیتم‌های موجود همین فروشگاه؛ نیاید = رفتار قدیمی
            dto.VarietyItemIds = ParseIds(varietyItemIds);
            dto.Variety2ItemIds = ParseIds(variety2ItemIds);
            var ProductItem = await _productItemService.GetInsertOrUpdateListAsync(dto);
            return Ok(ProductItem);
        }
        private static List<long> ParseIds(string csv) =>
            string.IsNullOrWhiteSpace(csv)
                ? null
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(part => long.TryParse(part, out var value) ? value : 0)
                    .Where(value => value > 0)
                    .Distinct()
                    .ToList();

        /// <summary>
        /// ویرایش و اضافه تنوع
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductItemDto>), 200)]
        public async Task<IActionResult> Post(ProductItemListUpdateDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            var ProductItem = await _productItemService.InsertOrUpdateAsync(dto);
            return Ok(ProductItem);
        }
    }
}
