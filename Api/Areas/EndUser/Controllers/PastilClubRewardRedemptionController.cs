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
    public class PastilClubRewardRedemptionController : ControllerBase
    {
        private readonly IClubRewardRedemptionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardRedemptionController(
            IClubRewardRedemptionService service,
            ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardRedemptionVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _service.FindUserAsync(id, _currentUser.CurrentUser.UserId, cancellationToken));

        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardRedemptionSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubRewardRedemptionInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.SearchUserAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
