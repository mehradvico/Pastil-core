using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت خوانده شدن پیام‌های پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchMessageReadController : ControllerBase
    {
        private readonly IPastilMatchMessageService _pastilMatchMessageService;

        public PastilMatchMessageReadController(IPastilMatchMessageService pastilMatchMessageService)
        {
            _pastilMatchMessageService = pastilMatchMessageService;
        }

        /// <summary>
        /// خوانده‌شده کردن پیام‌ها تا شناسه مشخص‌شده
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] PastilMatchMessageReadDto dto)
        {
            var result = await _pastilMatchMessageService.UpdateReadDto(dto);
            return Ok(result);
        }
    }
}