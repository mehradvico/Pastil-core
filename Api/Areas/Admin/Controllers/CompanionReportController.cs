using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارشات تخلف نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReportController : ControllerBase
    {
        private readonly ICompanionReportService _CompanionReportService;
        public CompanionReportController(ICompanionReportService CompanionReportService)
        {
            this._CompanionReportService = CompanionReportService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(CompanionReportSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReportInputDto dto)
        {
            var search = _CompanionReportService.Search(dto);
            return Ok(search);
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReportDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionReport = await _CompanionReportService.FindAsyncVDto(id);
            return Ok(CompanionReport);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionReportDto dto)
        {
            var result = await _CompanionReportService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
