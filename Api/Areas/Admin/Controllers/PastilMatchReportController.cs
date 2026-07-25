using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارش‌های پاستیل مچ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchReportController : ControllerBase
    {
        private readonly IPastilMatchReportService _pastilMatchReportService;

        public PastilMatchReportController(IPastilMatchReportService pastilMatchReportService)
        {
            _pastilMatchReportService = pastilMatchReportService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchReportVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchReportService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchReportSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchReportInputDto dto)
        {
            var result = _pastilMatchReportService.Search(dto);
            return Ok(result);
        }
    }
}