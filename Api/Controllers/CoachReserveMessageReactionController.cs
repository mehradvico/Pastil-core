using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت ری‌اکشن‌های پیام چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveMessageReactionController : ControllerBase
    {
        private readonly ICoachReserveMessageReactionService _companionReserveMessageReactionService;

        public CoachReserveMessageReactionController(ICoachReserveMessageReactionService companionReserveMessageReactionService)
        {
            _companionReserveMessageReactionService = companionReserveMessageReactionService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveMessageReactionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageReactionService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CoachReserveMessageReactionSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] CoachReserveMessageReactionInputDto dto)
        {
            var result = _companionReserveMessageReactionService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveMessageReactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] CoachReserveMessageReactionDto dto)
        {
            var result = await _companionReserveMessageReactionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _companionReserveMessageReactionService.DeleteDto(id);
            return Ok(result);
        }
    }
}
