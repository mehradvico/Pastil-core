using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// امتیاز پاستیل کلاب کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointController : ControllerBase
    {
        private readonly IClubPointService _clubPointService;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubPointController(IClubPointService clubPointService, ICurrentUserHelper currentUser)
        {
            _clubPointService = clubPointService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// موجودی و بدهی امتیاز کاربر
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointBalanceVDto>), 200)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _clubPointService.GetBalanceAsync(
                _currentUser.CurrentUser.UserId,
                cancellationToken));
        }
    }
}
