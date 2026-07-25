using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// اهداف پروفایل‌های پاستیل مچ
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileGoalController : ControllerBase
    {
        private readonly IPastilMatchProfileGoalService _pastilMatchProfileGoalService;

        public PastilMatchProfileGoalController(IPastilMatchProfileGoalService pastilMatchProfileGoalService)
        {
            this._pastilMatchProfileGoalService = pastilMatchProfileGoalService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchProfileGoalVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchProfileGoalService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchProfileGoalSearchDto), 200)]
        public IActionResult Get([FromQuery] PastilMatchProfileGoalInputDto dto)
        {
            var result = _pastilMatchProfileGoalService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchProfileGoalService.DeleteDto(id);
            return Ok(result);
        }
    }
}
