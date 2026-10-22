using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Pansion.Controllers
{
    /// <summary>
    /// مدیریت کنسلی رزرو نماینده
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Api.Filters.RequireCompanionScope]
    public class PansionReserveCancelController : ControllerBase
    {
        private readonly IPansionReserveApprovalService _approvalService;
        private readonly Application.Common.Interface.ICurrentUserHelper _currentUser;
        public PansionReserveCancelController(IPansionReserveApprovalService approvalService, Application.Common.Interface.ICurrentUserHelper currentUser)
        {
            this._approvalService = approvalService;
            this._currentUser = currentUser;
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveCancelDto dto)
        {
            // مرکز فقط می‌تواند لغو کند (برگرداندن لغو با ادمین است)؛ کل مبلغ به کیف پول مشتری برمی‌گردد و سفر متصل لغو می‌شود.
            if (dto == null || !dto.IsCancel)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _approvalService.CancelByOwnerAsync(dto.Id, dto.CancelDetail, _currentUser.CurrentUser.CompanionId, HttpContext.RequestAborted));
        }
    }
}
