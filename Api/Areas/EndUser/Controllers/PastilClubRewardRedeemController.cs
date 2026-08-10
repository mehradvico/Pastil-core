using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardRedeemController : ControllerBase
    {
        private readonly IClubRewardRedemptionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardRedeemController(IClubRewardRedemptionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardRedemptionVDto>), 200)]
        public async Task<IActionResult> Post(ClubRewardRedeemDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.RedeemAsync(_currentUser.CurrentUser.UserId, dto.RewardOfferId, cancellationToken));
    }
}
