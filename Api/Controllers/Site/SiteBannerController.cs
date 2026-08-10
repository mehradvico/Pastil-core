using Application.Common.Dto.Result;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// بنرهای قابل نمایش در سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Banner")]
    [ApiController]
    [AllowAnonymous]
    public class SiteBannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public SiteBannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] BannerInputDto dto)
        {
            dto.Available = true;
            dto.ShowToSite = true;
            return Ok(_bannerService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _bannerService.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data == null || !result.Data.Active || !result.Data.ShowToSite)
                return Ok(new BaseResultDto<BannerVDto>(false, Resource.Notification.NothingFound, null!));

            return Ok(result);
        }
    }
}
