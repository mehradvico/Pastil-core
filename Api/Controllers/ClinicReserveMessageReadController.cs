using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;
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
    public class ClinicReserveMessageReadController : ControllerBase
    {
        private readonly IClinicReserveMessageService _companionReserveMessageService;

        public ClinicReserveMessageReadController(IClinicReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] ClinicReserveMessageReadDto dto)
        {
            var result = await _companionReserveMessageService.UpdateReadDto(dto);
            return Ok(result);
        }
    }
}
