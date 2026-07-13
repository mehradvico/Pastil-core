using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت مچ‌های پاستیل
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchController : ControllerBase
    {
        private readonly IPastilMatchService _pastilMatchService;

        public PastilMatchController(IPastilMatchService pastilMatchService)
        {
            _pastilMatchService = pastilMatchService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchInputDto dto)
        {
            var result = _pastilMatchService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// بستن مچ
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchService.DeleteDto(id);
            return Ok(result);
        }
    }
}