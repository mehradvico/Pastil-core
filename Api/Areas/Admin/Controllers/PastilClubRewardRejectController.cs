using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رد جایزه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardRejectController : ControllerBase
    {
        private readonly IClubRewardOfferService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubRewardRejectController(IClubRewardOfferService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>
        /// رد پیشنهاد جایزه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post(ClubRewardOfferDecisionDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.RejectAsync(dto.RewardOfferId, dto.Reason, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
