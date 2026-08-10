using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubRewardTemplateController : ControllerBase
    {
        private readonly IClubRewardTemplateService _service;

        public PastilClubRewardTemplateController(IClubRewardTemplateService service)
        {
            _service = service;
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardTemplateDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _service.FindAsync(id, cancellationToken));

        [HttpGet]
        [ProducesResponseType(typeof(ClubRewardTemplateSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubRewardTemplateInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.SearchAsync(dto, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardTemplateDto>), 200)]
        public async Task<IActionResult> Post(ClubRewardTemplateDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.InsertAsync(dto, cancellationToken));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ClubRewardTemplateDto>), 200)]
        public async Task<IActionResult> Put(ClubRewardTemplateDto dto, CancellationToken cancellationToken) =>
            Ok(await _service.UpdateAsync(dto, cancellationToken));
    }
}
