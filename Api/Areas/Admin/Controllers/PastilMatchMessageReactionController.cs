using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت ری‌اکشن‌های پیام پاستیل مچ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchMessageReactionController : ControllerBase
    {
        private readonly IPastilMatchMessageReactionService _pastilMatchMessageReactionService;

        public PastilMatchMessageReactionController(IPastilMatchMessageReactionService pastilMatchMessageReactionService)
        {
            _pastilMatchMessageReactionService = pastilMatchMessageReactionService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchMessageReactionVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchMessageReactionService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchMessageReactionSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchMessageReactionInputDto dto)
        {
            var result = _pastilMatchMessageReactionService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchMessageReactionService.DeleteDto(id);
            return Ok(result);
        }
    }
}