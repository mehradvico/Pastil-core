using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachCatalogSrv.Iface;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مربی‌ها (عمومی): فقط مربی‌های تأییدشده و فعال
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CoachController : ControllerBase
    {
        private readonly ICoachCoreService _service;
        private readonly ICoachCatalogService _catalog;
        private readonly ICoachProfileService _profile;

        public CoachController(ICoachCoreService service, ICoachCatalogService catalog, ICoachProfileService profile)
        {
            _service = service;
            _catalog = catalog;
            _profile = profile;
        }

        /// <summary>جستجو (شهر، محله، حیوان، خدمت، خدمات در محل، ...)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CoachSearchDto), 200)]
        public IActionResult Get([FromQuery] CoachInputDto dto) => Ok(_service.SearchPublic(dto));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindPublicAsync(id));

        /// <summary>خدمات و بسته‌های قیمتی قابل رزرو؛ petSize (Small/Medium/Large) بسته‌ها را فیلتر می‌کند</summary>
        [HttpGet("{id:long}/Services")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachServiceVDto>>), 200)]
        public async Task<IActionResult> Services(long id, string petSize = null) => Ok(await _catalog.ListPublicAsync(id, petSize));

        [HttpGet("{id:long}/Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetTimesAsync(id));
        }

        [HttpGet("{id:long}/Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetZonesAsync(id));
        }

        [HttpGet("{id:long}/Pictures")]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachPictureVDto>>), 200)]
        public async Task<IActionResult> Pictures(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetPicturesAsync(id));
        }
    }
}
