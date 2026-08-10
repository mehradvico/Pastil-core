using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رد گروهی جوایز
    /// </summary>
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

        /// <summary>
        /// رد گروهی پیشنهادهای جایزه
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post(ClubRewardOfferBulkDecisionDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.BulkRejectAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
