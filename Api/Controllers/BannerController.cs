using Application.Common.Enumerable;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با بنر ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class BannerController : ControllerBase
    {
        private IBannerService bannerService;
        /// <summary>
        /// مرتبط با بنر ها
        /// </summary>
        public BannerController(IBannerService bannerService)
        {
            this.bannerService = bannerService;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [CustomOutputCache(CacheTypeEnum.BannerSearch)]
        [ProducesResponseType(typeof(BannerSearchDto), 200)]
        public IActionResult Get([FromQuery] BannerInputDto dto)
        {
            dto.Available = true;
            dto.ShowToApp = true;
            var post = bannerService.Search(dto);
            return Ok(post);
        }
    }
}
