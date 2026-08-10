using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنظیمات امتیاز پاستیل کلاب
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilClubPointRuleController : ControllerBase
    {
        private readonly IClubPointRuleService _clubPointRuleService;

        public PastilClubPointRuleController(IClubPointRuleService clubPointRuleService)
        {
            _clubPointRuleService = clubPointRuleService;
        }

        /// <summary>
        /// جزئیات قانون امتیاز
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointRuleDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointRuleService.FindAsync(id, cancellationToken));
        }

        /// <summary>
        /// جستجوی قوانین امتیاز
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClubPointRuleSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ClubPointRuleInputDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointRuleService.SearchAsync(dto, cancellationToken));
        }

        /// <summary>
        /// افزودن قانون امتیاز
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointRuleDto>), 200)]
        public async Task<IActionResult> Post(ClubPointRuleDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointRuleService.InsertAsync(dto, cancellationToken));
        }

        /// <summary>
        /// ویرایش قانون امتیاز
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ClubPointRuleDto>), 200)]
        public async Task<IActionResult> Put(ClubPointRuleDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _clubPointRuleService.UpdateAsync(dto, cancellationToken));
        }
    }
}
