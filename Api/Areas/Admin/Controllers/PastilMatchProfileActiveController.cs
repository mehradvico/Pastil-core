using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{

    /// <summary>
    /// پروفایل های پاستیل مچ
    /// </summary>
    /// <returns></returns>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileActiveController : ControllerBase
    {
        private readonly IPastilMatchProfileService _pastilMatchProfileService;

        public PastilMatchProfileActiveController(IPastilMatchProfileService pastilMatchProfileService)
        {
            this._pastilMatchProfileService = pastilMatchProfileService;
        }
        /// <summary>
        /// فعالسازی آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PastilMatchProfileActiveDto dto)
        {
            var result = _pastilMatchProfileService.UpdateActiveDto(dto);
            return Ok(result);
        }
    }
}
