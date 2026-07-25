using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportReasonSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// دلایل گزارش پاستیل مچ
    /// </summary>
    [Area("Admin")]
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

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchReportReasonDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PastilMatchReportReasonDto dto)
        {
            var result = await _pastilMatchReportReasonService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Put([FromBody] PastilMatchReportReasonDto dto)
        {
            var result = _pastilMatchReportReasonService.UpdateDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchReportReasonService.DeleteDto(id);
            return Ok(result);
        }
    }
}