using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت فعال بودن دلایل گزارش پاستیل مچ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchReportReasonActiveController : ControllerBase
    {
        private readonly IPastilMatchReportReasonService _pastilMatchReportReasonService;

        public PastilMatchReportReasonActiveController(IPastilMatchReportReasonService pastilMatchReportReasonService)
        {
            _pastilMatchReportReasonService = pastilMatchReportReasonService;
        }

        /// <summary>
        /// فعال یا غیرفعال کردن آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Put([FromBody] PastilMatchReportReasonActiveDto dto)
        {
            var result = _pastilMatchReportReasonService.UpdateActiveDto(dto);
            return Ok(result);
        }
    }
}