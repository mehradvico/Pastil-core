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
    public class PastilClubRewardBulkApproveController : ControllerBase
    {
        private readonly IClubRewardOfferService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardBulkApproveController(IClubRewardOfferService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ClubRewardOfferBulkDecisionDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.BulkApproveAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
