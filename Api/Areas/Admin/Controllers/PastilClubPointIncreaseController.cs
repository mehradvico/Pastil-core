using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// افزایش دستی امتیاز
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointIncreaseController : ControllerBase
    {
        private readonly IClubPointService _clubPointService;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubPointIncreaseController(IClubPointService clubPointService, ICurrentUserHelper currentUser)
        {
            _clubPointService = clubPointService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// افزایش دستی امتیاز کاربر
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointTransactionVDto>), 200)]
        public async Task<IActionResult> Post(ClubManualPointDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointService.IncreaseManualAsync(
                dto,
                _currentUser.CurrentUser.UserId,
                cancellationToken));
        }
    }
}
