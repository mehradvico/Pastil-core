using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveExcelSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveExcelSrv.Iface;
using Application.Services.ProductSrvs.ProductExelSrv.Dto;
using Application.Services.ProductSrvs.ProductExelSrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{

    /// <summary>
    /// خروجی اکسل رزروهای نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveExcelController : ControllerBase
    {
        private readonly ICompanionReserveExcelService _reseveExcelService;
        public CompanionReserveExcelController(ICompanionReserveExcelService reseveExcelService)
        {
            _reseveExcelService = reseveExcelService;
        }

        /// <summary>
        ///  دریافت اکسل
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public IActionResult Get([FromQuery] SearchCompanionReserveExcelDto search)
        {
            var result = _reseveExcelService.GetReserveExcel(search);
            var content = result.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "CompanionReserves.xlsx";
            return File(content, contentType, fileName);
        }
    }
}
