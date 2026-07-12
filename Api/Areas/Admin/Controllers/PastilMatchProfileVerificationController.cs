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
    /// <returns></returns>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileVerificationController : ControllerBase
    {
        private readonly IPastilMatchProfileService _pastilMatchProfileService;

        public PastilMatchProfileVerificationController(IPastilMatchProfileService pastilMatchProfileService)
        {
            this._pastilMatchProfileService = pastilMatchProfileService;
        }
        /// <summary>
        /// تایید آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PastilMatchProfileVerificationDto dto)
        {
            var result = _pastilMatchProfileService.UpdateVerificationDto(dto);
            return Ok(result);
        }
    }
}
