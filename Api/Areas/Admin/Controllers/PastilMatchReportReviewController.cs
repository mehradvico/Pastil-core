using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// بررسی گزارش‌های پاستیل مچ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchReportReviewController : ControllerBase
    {
        private readonly IPastilMatchReportService _pastilMatchReportService;

        public PastilMatchReportReviewController(IPastilMatchReportService pastilMatchReportService)
        {
            _pastilMatchReportService = pastilMatchReportService;
        }

        /// <summary>
        /// ثبت نتیجه بررسی گزارش
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] PastilMatchReportReviewDto dto)
        {
            var result = await _pastilMatchReportService.UpdateReviewDto(dto);
            return Ok(result);
        }
    }
}