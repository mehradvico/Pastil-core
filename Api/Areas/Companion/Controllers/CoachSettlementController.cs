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
    /// تاریخچه‌ی تسویه‌ی مربی‌های من (تسویه‌ی ارائه‌دهنده در استخر مربی)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachSettlementController : ControllerBase
    {
        private readonly ISettlementService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachSettlementController(ISettlementService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SettlementSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] SettlementInputDto dto) =>
            Ok(await _service.SearchForProviderUserAsync(dto, SettlementKindEnum.Coach, _currentUser.CurrentUser.UserId));

        /// <summary>ریز تسویه‌ی خودم</summary>
        [HttpGet("{id:long}/Items")]
        [ProducesResponseType(typeof(BaseResultDto<List<SettlementItemVDto>>), 200)]
        public async Task<IActionResult> Items(long id)
        {
            if (!await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Coach, _currentUser.CurrentUser.UserId))
                return Ok(new BaseResultDto<List<SettlementItemVDto>>(false, Resource.Notification.AccessDenied, null));
            return Ok(await _service.GetItemsAsync(id));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<SettlementVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            if (!await _service.BelongsToProviderUserAsync(id, SettlementKindEnum.Coach, _currentUser.CurrentUser.UserId))
                return Ok(new BaseResultDto<SettlementVDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(await _service.FindAsyncVDto(id));
        }
    }
}
