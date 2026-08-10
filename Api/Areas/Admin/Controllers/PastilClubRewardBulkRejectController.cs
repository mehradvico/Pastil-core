using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardBulkRejectController : ControllerBase
    {
        private readonly IClubRewardOfferService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardBulkRejectController(IClubRewardOfferService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ClubRewardOfferBulkDecisionDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.BulkRejectAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
