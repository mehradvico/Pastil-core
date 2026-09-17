using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// ثبت تحویل پیام چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveMessageDeliveredController : ControllerBase
    {
        private readonly ICompanionReserveMessageService _companionReserveMessageService;

        public CompanionReserveMessageDeliveredController(ICompanionReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] CompanionReserveMessageDeliveredDto dto)
        {
            var result = await _companionReserveMessageService.UpdateDeliveredDto(dto);
            return Ok(result);
        }
    }
}
