using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Iface;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Application.Common.Dto.Result;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت جستجو
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("Search")]
    public class SearchController : ControllerBase
    {
        private ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            this._searchService = searchService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SearchDto>), 200)]
        public async Task<IActionResult> Post(SearchRequestDto dto, CancellationToken cancellationToken)
        {
            var post = await _searchService.SearchAsync(dto, cancellationToken);
            return Ok(post);
        }

        /// <summary>
        /// پیشنهاد عبارت جستجوی فروشگاه؛ فقط عبارت‌هایی که حتماً در لیست محصولات نتیجه دارند
        /// </summary>
        [HttpGet("Suggest")]
        [ProducesResponseType(typeof(BaseResultDto<System.Collections.Generic.List<string>>), 200)]
        public async Task<IActionResult> Suggest(
            [FromQuery] string q,
            [FromQuery] int take = 8,
            CancellationToken cancellationToken = default)
        {
            var result = await _searchService.SuggestAsync(q, take, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// پرتکرارترین جستجوهای کاربران (مثلاً channel=Shop برای جستجوی فروشگاه)
        /// </summary>
        [HttpGet("Popular")]
        [ProducesResponseType(typeof(BaseResultDto<System.Collections.Generic.List<string>>), 200)]
        public async Task<IActionResult> Popular(
            [FromQuery] string channel = "Shop",
            [FromQuery] int days = 30,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _searchService.GetPopularAsync(channel, days, take, cancellationToken);
            return Ok(result);
        }

    }
}
