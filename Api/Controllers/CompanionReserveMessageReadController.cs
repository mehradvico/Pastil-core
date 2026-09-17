using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// ثبت خوانده شدن پیام‌های چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveMessageReadController : ControllerBase
    {
        private readonly ICompanionReserveMessageService _companionReserveMessageService;

        public CompanionReserveMessageReadController(ICompanionReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] CompanionReserveMessageReadDto dto)
        {
            var result = await _companionReserveMessageService.UpdateReadDto(dto);
            return Ok(result);
        }
    }
}
