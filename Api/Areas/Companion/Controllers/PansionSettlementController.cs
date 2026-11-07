using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// تاریخچه‌ی تسویه‌ی پانسیون‌های من. از ۲۰۲۶-۱۱-۰۳ پانسیونِ دارای ارائه‌دهنده استخر جدای «پانسیون» (Kind=7) دارد؛
    /// تسویه‌های قدیمی‌تر (و پانسیون‌های فقط-نمایندگی) در استخر «نمایندگی‌ها» (Kind=1) همراه رزرو خدمت، مشاوره و مدرسه‌اند.
    /// فهرست پیش‌فرض Kind=7 است و با kind=1 تاریخچه‌ی قدیمی داده می‌شود؛ ریز هر تسویه فقط ردیف‌های پانسیون (Type=Pansion) است.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionSettlementController : ControllerBase
    {
        private readonly ISettlementService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PansionSettlementController(ISettlementService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>تسویه‌های ارائه‌دهنده‌ی من: پیش‌فرض استخر «پانسیون» (۷)؛ kind=1 = تسویه‌های قدیمی استخر نمایندگی‌ها (شامل پانسیون)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(SettlementSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] SettlementInputDto dto)
        {
            var kind = dto?.Kind == SettlementKindEnum.Companion ? SettlementKindEnum.Companion : SettlementKindEnum.Pansion;
            return Ok(await _service.SearchForProviderUserAsync(dto, kind, _currentUser.CurrentUser.UserId));
        }

        // تسویه‌ی من در یکی از دو استخری که ردیف پانسیون دارند
        private async Task<bool> BelongsToMeAsync(long id) =>
            await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Pansion, _currentUser.CurrentUser.UserId)
            || await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Companion, _currentUser.CurrentUser.UserId);

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<SettlementVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            if (!await BelongsToMeAsync(id))
                return Ok(new BaseResultDto<SettlementVDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(await _service.FindAsyncVDto(id));
        }

        /// <summary>ریز پانسیونیِ یک تسویه‌ی من (فقط ردیف‌های Type=Pansion)</summary>
        [HttpGet("{id:long}/Items")]
        [ProducesResponseType(typeof(BaseResultDto<List<SettlementItemVDto>>), 200)]
        public async Task<IActionResult> Items(long id)
        {
            if (!await BelongsToMeAsync(id))
                return Ok(new BaseResultDto<List<SettlementItemVDto>>(false, Resource.Notification.AccessDenied, null));
            var all = await _service.GetItemsAsync(id);
            if (!all.IsSuccess || all.Data == null)
                return Ok(all);
            return Ok(new BaseResultDto<List<SettlementItemVDto>>(true, all.Data.Where(i => i.Type == "Pansion").ToList()));
        }
    }
}
