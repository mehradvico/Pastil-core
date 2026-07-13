using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// دریافت دلایل گزارش پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchReportReasonController : ControllerBase
    {
        private readonly IPastilMatchReportReasonService _pastilMatchReportReasonService;

        public PastilMatchReportReasonController(IPastilMatchReportReasonService pastilMatchReportReasonService)
        {
            _pastilMatchReportReasonService = pastilMatchReportReasonService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchReportReasonVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchReportReasonService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchReportReasonSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchReportReasonInputDto dto)
        {
            var result = _pastilMatchReportReasonService.Search(dto);
            return Ok(result);
        }
    }
}