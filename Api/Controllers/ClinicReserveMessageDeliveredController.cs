using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;
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
    public class ClinicReserveMessageDeliveredController : ControllerBase
    {
        private readonly IClinicReserveMessageService _companionReserveMessageService;

        public ClinicReserveMessageDeliveredController(IClinicReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] ClinicReserveMessageDeliveredDto dto)
        {
            var result = await _companionReserveMessageService.UpdateDeliveredDto(dto);
            return Ok(result);
        }
    }
}
