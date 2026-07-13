using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    using Application.Common.Dto.Result;
    using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto;
    using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace Api.Areas.Admin.Controllers
    {
        /// <summary>
        /// مدیریت کاربران بلاک‌شده در پاستیل مچ
        /// </summary>
        [Area("Admin")]
        [Route("api/[area]/[controller]")]
        [ApiController]
        [Authorize]
        public class PastilMatchBlockController : ControllerBase
        {
            private readonly IPastilMatchBlockService _pastilMatchBlockService;

            public PastilMatchBlockController(IPastilMatchBlockService pastilMatchBlockService)
            {
                _pastilMatchBlockService = pastilMatchBlockService;
            }

            /// <summary>
            /// اطلاعات آیتم
            /// </summary>
            [HttpGet("{id}")]
            [ProducesResponseType(typeof(BaseResultDto<PastilMatchBlockVDto>), StatusCodes.Status200OK)]
            public async Task<IActionResult> Get(long id)
            {
                var result = await _pastilMatchBlockService.FindAsyncVDto(id);
                return Ok(result);
            }

            /// <summary>
            /// جستجو
            /// </summary>
            [HttpGet]
            [ProducesResponseType(typeof(PastilMatchBlockSearchDto), StatusCodes.Status200OK)]
            public IActionResult Get([FromQuery] PastilMatchBlockInputDto dto)
            {
                var result = _pastilMatchBlockService.Search(dto);
                return Ok(result);
            }

            /// <summary>
            /// حذف بلاک
            /// </summary>
            [HttpDelete]
            [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
            public IActionResult Delete(long id)
            {
                var result = _pastilMatchBlockService.DeleteDto(id);
                return Ok(result);
            }
        }
    }
}
