using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاهش دستی امتیاز پاستیل کلاب
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointDecreaseController : ControllerBase
    {
        private readonly IClubPointService _clubPointService;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubPointDecreaseController(IClubPointService clubPointService, ICurrentUserHelper currentUser)
        {
            _clubPointService = clubPointService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// کاهش دستی امتیاز کاربر
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointTransactionVDto>), 200)]
        public async Task<IActionResult> Post(ClubManualPointDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointService.DecreaseManualAsync(
                dto,
                _currentUser.CurrentUser.UserId,
                cancellationToken));
        }
    }
}
