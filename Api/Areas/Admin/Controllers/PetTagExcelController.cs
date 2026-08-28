using Application.Services.Accounting.PetTagSrv.Dto;
using Application.Services.Accounting.PetTagSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خروجی اکسل کدهای قلاده
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetTagExcelController : ControllerBase
    {
        private readonly IPetTagService _service;
        public PetTagExcelController(IPetTagService service)
        {
            _service = service;
        }

        /// <summary>
        /// دریافت اکسل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PetTagExportFilterDto filter)
        {
            var result = await _service.GetExcelAsync(filter);
            var content = result.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "PetTags.xlsx";
            return File(content, contentType, fileName);
        }
    }
}
