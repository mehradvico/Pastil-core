using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductExelSrv.Dto;
using Application.Services.ProductSrvs.ProductExelSrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// اکسل محصولات
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsExcelController : ControllerBase
    {
        private readonly IProductExcelService _excelService;
        public ProductsExcelController(IProductExcelService excelService)
        {
            _excelService = excelService;
        }

        /// <summary>
        /// دریافت آیتم
        /// </summary>
        /// 
        [HttpGet]
        public IActionResult GetTemplate()
        {
            var result = _excelService.GetProductExcelTemplate();
            var content = result.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "ProductsTemplate.xlsx";
            return File(content, contentType, fileName);
        }

        /// <summary>
        /// بازگذاری آیتم
        /// </summary>
        /// 
        [HttpPost]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            var stream = await _excelService.ImportProductsAsync(file);
            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "ProductsResult.xlsx";
            return File(content, contentType, fileName);
        }
    }
}
