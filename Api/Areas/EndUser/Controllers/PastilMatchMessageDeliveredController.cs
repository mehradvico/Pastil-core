using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت تحویل پیام پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchMessageDeliveredController : ControllerBase
    {
        private readonly IPastilMatchMessageService _pastilMatchMessageService;

        public PastilMatchMessageDeliveredController(IPastilMatchMessageService pastilMatchMessageService)
        {
            _pastilMatchMessageService = pastilMatchMessageService;
        }

        /// <summary>
        /// ثبت تحویل پیام
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] PastilMatchMessageDeliveredDto dto)
        {
            var result = await _pastilMatchMessageService.UpdateDeliveredDto(dto);
            return Ok(result);
        }
    }
}