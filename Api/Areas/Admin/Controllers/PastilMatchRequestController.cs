using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت درخواست‌های پاستیل مچ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchRequestController : ControllerBase
    {
        private readonly IPastilMatchRequestService _pastilMatchRequestService;

        public PastilMatchRequestController(IPastilMatchRequestService pastilMatchRequestService)
        {
            _pastilMatchRequestService = pastilMatchRequestService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchRequestVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchRequestService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchRequestSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchRequestInputDto dto)
        {
            var result = _pastilMatchRequestService.Search(dto);
            return Ok(result);
        }
    }
}