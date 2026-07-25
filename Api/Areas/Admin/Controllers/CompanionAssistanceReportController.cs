using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارشات تخلف خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceReportController : ControllerBase
    {
        private readonly ICompanionAssistanceReportService _CompanionAssistanceReportService;
        public CompanionAssistanceReportController(ICompanionAssistanceReportService CompanionAssistanceReportService)
        {
            this._CompanionAssistanceReportService = CompanionAssistanceReportService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(CompanionAssistanceReportSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceReportInputDto dto)
        {
            var search = _CompanionAssistanceReportService.Search(dto);
            return Ok(search);
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمت نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceReportDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionAssistanceReport = await _CompanionAssistanceReportService.FindAsyncVDto(id);
            return Ok(CompanionAssistanceReport);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistanceReportDto dto)
        {
            var result = await _CompanionAssistanceReportService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
