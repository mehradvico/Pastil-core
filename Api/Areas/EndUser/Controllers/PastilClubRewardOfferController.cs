using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardOfferController : ControllerBase
    {
        private readonly IClubRewardOfferService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardOfferController(IClubRewardOfferService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardOfferVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _service.FindUserAsync(id, _currentUser.CurrentUser.UserId, cancellationToken));

        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardOfferSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubRewardOfferInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.SearchUserAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
