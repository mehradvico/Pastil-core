using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardRedemptionController : ControllerBase
    {
        private readonly IClubRewardRedemptionService _service;

        public PastilClubRewardRedemptionController(IClubRewardRedemptionService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardRedemptionVDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _service.FindAdminAsync(id, cancellationToken));

        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardRedemptionSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubRewardRedemptionInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.SearchAdminAsync(dto, cancellationToken));
    }
}
