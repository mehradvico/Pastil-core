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
    /// تاریخچه‌ی تسویه‌ی پانسیون‌های من. پانسیون استخر جدا ندارد و در استخر «نمایندگی‌ها» (Kind=1) همراه رزرو خدمت، مشاوره و مدرسه تسویه می‌شود؛
    /// این مسیر همان تسویه‌های ارائه‌دهنده‌ی کاربر را می‌دهد و ریز آن را فقط به ردیف‌های پانسیون (Type=Pansion) محدود می‌کند.
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

        /// <summary>تسویه‌های ارائه‌دهنده‌ی من در استخر نمایندگی‌ها (شامل پانسیون)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(SettlementSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] SettlementInputDto dto) =>
            Ok(await _service.SearchForProviderUserAsync(dto, SettlementKindEnum.Companion, _currentUser.CurrentUser.UserId));

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<SettlementVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            if (!await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Companion, _currentUser.CurrentUser.UserId))
                return Ok(new BaseResultDto<SettlementVDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(await _service.FindAsyncVDto(id));
        }

        /// <summary>ریز پانسیونیِ یک تسویه‌ی من (فقط ردیف‌های Type=Pansion)</summary>
        [HttpGet("{id:long}/Items")]
        [ProducesResponseType(typeof(BaseResultDto<List<SettlementItemVDto>>), 200)]
        public async Task<IActionResult> Items(long id)
        {
            if (!await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Companion, _currentUser.CurrentUser.UserId))
                return Ok(new BaseResultDto<List<SettlementItemVDto>>(false, Resource.Notification.AccessDenied, null));
            var all = await _service.GetItemsAsync(id);
            if (!all.IsSuccess || all.Data == null)
                return Ok(all);
            return Ok(new BaseResultDto<List<SettlementItemVDto>>(true, all.Data.Where(i => i.Type == "Pansion").ToList()));
        }
    }
}
