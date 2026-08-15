using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت درخواست‌های پاستیل مچ
    /// </summary>
    [Area("EndUser")]
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

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PastilMatchRequestDto dto)
        {
            var result = await _pastilMatchRequestService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// لغو درخواست
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _pastilMatchRequestService.DeleteAsyncDto(id);
            return Ok(result);
        }
    }
}
