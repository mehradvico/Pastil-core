using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface;
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
    public class ClinicReserveMessageReactionController : ControllerBase
    {
        private readonly IClinicReserveMessageReactionService _companionReserveMessageReactionService;

        public ClinicReserveMessageReactionController(IClinicReserveMessageReactionService companionReserveMessageReactionService)
        {
            _companionReserveMessageReactionService = companionReserveMessageReactionService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageReactionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageReactionService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReserveMessageReactionSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] ClinicReserveMessageReactionInputDto dto)
        {
            var result = _companionReserveMessageReactionService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageReactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] ClinicReserveMessageReactionDto dto)
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
