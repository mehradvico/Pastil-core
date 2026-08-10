using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پیشنهادهای جایزه
    /// </summary>
    [Area("Admin")]
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

        /// <summary>
        /// جزئیات پیشنهاد جایزه
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardOfferVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _service.FindAdminAsync(id, cancellationToken));

        /// <summary>
        /// فهرست پیشنهادهای جایزه
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardOfferSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubRewardOfferInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.SearchAdminAsync(dto, cancellationToken));

        /// <summary>
        /// ثبت دستی پیشنهاد جایزه
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardOfferVDto>), 200)]
        public async Task<IActionResult> Post(ClubRewardOfferCreateDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.CreateManualAsync(dto, _currentUser.CurrentUser.UserId, cancellationToken));
    }
}
