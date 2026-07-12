using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت پروفایل های پاستیل مچ
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileController : ControllerBase
    {
        private readonly IPastilMatchProfileService _pastilMatchProfileService;

        public PastilMatchProfileController(IPastilMatchProfileService pastilMatchProfileService)
        {
            this._pastilMatchProfileService = pastilMatchProfileService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchProfileVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchProfileService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchProfileSearchDto), 200)]
        public IActionResult Get([FromQuery] PastilMatchProfileInputDto dto)
        {
            var result = _pastilMatchProfileService.Search(dto);
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
            var result = _pastilMatchProfileService.DeleteDto(id);
            return Ok(result);
        }
    }
}
