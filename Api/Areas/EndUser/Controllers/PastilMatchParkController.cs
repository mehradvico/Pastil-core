using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Iface;
using Application.Services.PastilMatchSrvs.PastilMatchParkSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پارک‌های قابل انتخاب برای هدف قرار در پارک در پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchParkController : ControllerBase
    {
        private readonly IParkService _parkService;

        public PastilMatchParkController(IParkService parkService)
        {
            _parkService = parkService;
        }

        /// <summary>
        /// لیست پارک‌ها همراه عکس اصلی و گالری
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<BaseSearchDto<ParkVDto>>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchParkInputDto dto)
        {
            if (dto.PastilMatchGoalId != (long)PastilMatchGoalEnum.PastilMatchGoal_ParkMeetup)
            {
                return Ok(new BaseResultDto<BaseSearchDto<ParkVDto>>(
                    false,
                    Resource.Notification.InvalidPastilMatchGoal,
                    null!));
            }

            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            var result = _parkService.Search(dto);
            return Ok(new BaseResultDto<BaseSearchDto<ParkVDto>>(true, result));
        }
    }
}
