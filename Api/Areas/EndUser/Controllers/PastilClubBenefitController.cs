using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilClubSrvs.BenefitSrv.Dto;
using Application.Services.PastilClubSrvs.BenefitSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubBenefitController : ControllerBase
    {
        private readonly IClubBenefitWalletService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PastilClubBenefitController(IClubBenefitWalletService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ClubBenefitWalletVDto>), 200)]
        public async Task<IActionResult> Get(bool includeConsumed = false, CancellationToken cancellationToken = default) =>
            Ok(await _service.GetAsync(_currentUser.CurrentUser.UserId, includeConsumed, cancellationToken));
    }
}
