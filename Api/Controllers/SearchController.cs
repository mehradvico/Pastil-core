using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Iface;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت جستجو
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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
        [ProducesResponseType(typeof(BrandSearchDto), 200)]
        public async Task<IActionResult> Post(SearchRequestDto dto)
        {
            var post = await _searchService.SearchAsync(dto);
            return Ok(post);
        }

    }
}
