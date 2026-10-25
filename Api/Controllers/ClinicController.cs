using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Iface;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Application.Services.VenueMemberSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// کلینیک‌ها (عمومی): فقط کلینیک‌های تأییدشده و فعال
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ClinicController : ControllerBase
    {
        private readonly IClinicCoreService _service;
        private readonly IClinicCatalogService _catalog;
        private readonly IClinicProfileService _profile;
        private readonly IVenueMemberService _members;

        public ClinicController(IClinicCoreService service, IClinicCatalogService catalog, IClinicProfileService profile, IVenueMemberService members)
        {
            _members = members;
            _service = service;
            _catalog = catalog;
            _profile = profile;
        }

        /// <summary>جستجو (شهر، محله، حیوان، خدمت، خدمات در محل، ...)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClinicSearchDto), 200)]
        public IActionResult Get([FromQuery] ClinicInputDto dto) => Ok(_service.SearchPublic(dto));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindPublicAsync(id));

        /// <summary>خدمات و بسته‌های قیمتی قابل رزرو؛ petSize (Small/Medium/Large) بسته‌ها را فیلتر می‌کند</summary>
        [HttpGet("{id:long}/Services")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicServiceVDto>>), 200)]
        public async Task<IActionResult> Services(long id, string petSize = null) => Ok(await _catalog.ListPublicAsync(id, petSize));

        [HttpGet("{id:long}/Times")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicTimeVDto>>), 200)]
        public async Task<IActionResult> Times(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetTimesAsync(id));
        }

        [HttpGet("{id:long}/Zones")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicZoneVDto>>), 200)]
        public async Task<IActionResult> Zones(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetZonesAsync(id));
        }

        [HttpGet("{id:long}/Pictures")]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicPictureVDto>>), 200)]
        public async Task<IActionResult> Pictures(long id)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _profile.GetPicturesAsync(id));
        }

        /// <summary>اعضای مرکز (کارت «اعضای مرکز»): مالک + پرسنل پذیرفته‌شده و فعال؛ فقط نام، عکس و عنوان نقش</summary>
        /// <remarks>pageSize پیش‌فرض و حداکثر ۵۰؛ مرکزِ بدون عضو = data خالی. پاسخ ۶۰ ثانیه cache می‌شود.</remarks>
        [HttpGet("{id:long}/Members")]
        [ProducesResponseType(typeof(BaseResultDto<List<VenueMemberVDto>>), 200)]
        public async Task<IActionResult> Members(long id, int? page = null, int? pageSize = null)
        {
            if (!(await _service.FindPublicAsync(id)).IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _members.ClinicMembersAsync(id, page, pageSize));
        }
    }
}
