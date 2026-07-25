using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.ProductReportSrv.Dto;
using Application.Services.ProductSrvs.ProductReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارشات تخلف محصول
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductReportController : ControllerBase
    {
        private readonly IProductReportService _ProductReportService;
        public ProductReportController(IProductReportService ProductReportService)
        {
            this._ProductReportService = ProductReportService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(ProductReportSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductReportInputDto dto)
        {
            var search = _ProductReportService.Search(dto);
            return Ok(search);
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductReportDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ProductReport = await _ProductReportService.FindAsyncVDto(id);
            return Ok(ProductReport);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ProductReportDto dto)
        {
            var result = await _ProductReportService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
