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

    }
}
