using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageSrv.Iface;
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
    public class CompanionReserveMessageController : ControllerBase
    {
        private readonly ICompanionReserveMessageService _companionReserveMessageService;

        public CompanionReserveMessageController(ICompanionReserveMessageService companionReserveMessageService)
        {
            _companionReserveMessageService = companionReserveMessageService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CompanionReserveMessageSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] CompanionReserveMessageInputDto dto)
        {
            var result = _companionReserveMessageService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] CompanionReserveMessageDto dto)
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
