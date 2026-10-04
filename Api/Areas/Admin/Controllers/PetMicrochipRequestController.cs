using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PetMicrochipSrv.Dto;
using Application.Services.Accounting.PetMicrochipSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// درخواست‌های جستجوی میکروچیپ و پیگیری توسط پاستیل (ادمین مشخصات مالک پت را می‌بیند)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetMicrochipRequestController : ControllerBase
    {
        private readonly IPetMicrochipService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public PetMicrochipRequestController(IPetMicrochipService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>فهرست؛ پیش‌فرض فقط درخواست‌های پیگیری</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PetMicrochipRequestSearchVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] PetMicrochipRequestInputDto dto) => Ok(await _service.AdminSearchAsync(dto));

        /// <summary>جزئیات: پروفایل پت + مالک + درخواست‌دهنده</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PetMicrochipRequestDetailVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.AdminGetAsync(id));

        /// <summary>تغییر وضعیت و یادداشت ادمین</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long id, PetMicrochipRequestUpdateDto dto)
            => Ok(await _service.AdminUpdateAsync(id, dto, _currentUserHelper.CurrentUser?.UserId ?? 0));
    }
}
