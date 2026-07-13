using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// قبول یا رد درخواست پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchRequestResponseController : ControllerBase
    {
        private readonly IPastilMatchRequestService _pastilMatchRequestService;

        public PastilMatchRequestResponseController(IPastilMatchRequestService pastilMatchRequestService)
        {
            _pastilMatchRequestService = pastilMatchRequestService;
        }

        /// <summary>
        /// قبول یا رد درخواست
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] PastilMatchRequestResponseDto dto)
        {
            var result = await _pastilMatchRequestService.UpdateResponseDto(dto);
            return Ok(result);
        }
    }
}