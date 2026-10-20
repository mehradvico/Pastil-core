using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Iface;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// آرایشگاه‌ها (عمومی): فقط آرایشگاه‌های تأییدشده و فعال
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class GroomerController : ControllerBase
    {
        private readonly IGroomerCoreService _service;
        private readonly IGroomerCatalogService _catalog;
        private readonly IGroomerProfileService _profile;

        public GroomerController(IGroomerCoreService service, IGroomerCatalogService catalog, IGroomerProfileService profile)
        {
            _service = service;
            _catalog = catalog;
            _profile = profile;
        }

        /// <summary>جستجو (شهر، محله، حیوان، خدمت، خدمات در محل، ...)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerSearchDto), 200)]
        public IActionResult Get([FromQuery] GroomerInputDto dto) => Ok(_service.SearchPublic(dto));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindPublicAsync(id));

        /// <summary>خدمات و بسته‌های قیمتی قابل رزرو؛ petSize (Small/Medium/Large) بسته‌ها را فیلتر می‌کند</summary>
        [HttpGet("{id:long}/Services")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerServiceVDto>>), 200)]
        public async Task<IActionResult> Services(long id, string petSize = null) => Ok(await _catalog.ListPublicAsync(id, petSize));

        [HttpGet("{id:long}/Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetTimesAsync(id));
        }

        [HttpGet("{id:long}/Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetZonesAsync(id));
        }

        [HttpGet("{id:long}/Pictures")]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerPictureVDto>>), 200)]
        public async Task<IActionResult> Pictures(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetPicturesAsync(id));
        }
    }
}
