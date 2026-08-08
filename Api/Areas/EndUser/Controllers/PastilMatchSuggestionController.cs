using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پیشنهاد یک‌به‌یک بهترین پت‌فرند
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchSuggestionController : ControllerBase
    {
        private readonly IPastilMatchSuggestionService _suggestionService;

        public PastilMatchSuggestionController(
            IPastilMatchSuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        /// <summary>
        /// دریافت بهترین پیشنهاد بعدی بر اساس فیلترها و درصد تطابق
        /// </summary>
        [HttpPost]
        [ProducesResponseType(
            typeof(BaseResultDto<PastilMatchSuggestionVDto>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(
            [FromBody] PastilMatchSuggestionInputDto dto)
        {
            return Ok(await _suggestionService.FindNextAsync(dto));
        }
    }
}
