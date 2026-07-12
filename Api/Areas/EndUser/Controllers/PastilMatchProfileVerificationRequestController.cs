using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پروفایل های پاستیل مچ
    /// </summary>
    /// <returns></returns>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchProfileVerificationRequestController : ControllerBase
    {
        private readonly IPastilMatchProfileService _pastilMatchProfileService;

        public PastilMatchProfileVerificationRequestController(IPastilMatchProfileService pastilMatchProfileService)
        {
            this._pastilMatchProfileService = pastilMatchProfileService;
        }
        /// <summary>
        /// درخواست تایید
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Post(PastilMatchProfileVerificationRequestDto dto)
        {
            var result = _pastilMatchProfileService.RequestVerificationDto(dto);
            return Ok(result);
        }
    }
}
