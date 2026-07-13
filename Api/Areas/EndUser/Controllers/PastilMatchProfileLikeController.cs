using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت لایک پروفایل‌های پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileLikeController : ControllerBase
    {
        private readonly IPastilMatchProfileLikeService _pastilMatchProfileLikeService;

        public PastilMatchProfileLikeController(IPastilMatchProfileLikeService pastilMatchProfileLikeService)
        {
            _pastilMatchProfileLikeService = pastilMatchProfileLikeService;
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchProfileLikeDto>),StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PastilMatchProfileLikeDto dto)
        {
            var result = await _pastilMatchProfileLikeService.InsertAsyncDto(dto);

            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto),StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchProfileLikeService.DeleteDto(id);

            return Ok(result);
        }
    }
}
