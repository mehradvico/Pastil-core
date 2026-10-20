using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// چت خدمات آنلاین (رزروکننده و نماینده) — بدون Area چون مجاز بودن رابطه‌محور است، نه نقش‌محور
    /// (دقیقاً مثل Api/Hubs/CallHub.cs که هر دو طرف را از یک نقطه‌ی ورود سرویس می‌دهد).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReserveMessageController : ControllerBase
    {
        private readonly IClinicReserveMessageService _companionReserveMessageService;

        public ClinicReserveMessageController(IClinicReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReserveMessageSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] ClinicReserveMessageInputDto dto)
        {
            var result = _companionReserveMessageService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] ClinicReserveMessageDto dto)
        {
            var result = await _companionReserveMessageService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _companionReserveMessageService.DeleteDto(id);
            return Ok(result);
        }
    }
}
