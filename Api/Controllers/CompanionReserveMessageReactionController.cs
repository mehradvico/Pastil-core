using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface;
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
    public class CompanionReserveMessageReactionController : ControllerBase
    {
        private readonly ICompanionReserveMessageReactionService _companionReserveMessageReactionService;

        public CompanionReserveMessageReactionController(ICompanionReserveMessageReactionService companionReserveMessageReactionService)
        {
            _companionReserveMessageReactionService = companionReserveMessageReactionService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageReactionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageReactionService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CompanionReserveMessageReactionSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] CompanionReserveMessageReactionInputDto dto)
        {
            var result = _companionReserveMessageReactionService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageReactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] CompanionReserveMessageReactionDto dto)
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
