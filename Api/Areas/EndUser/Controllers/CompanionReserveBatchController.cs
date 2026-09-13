using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت سبد رزرو (چند خدمت از یک نمایندگی با یک پرداخت مشترک)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveBatchController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        private readonly CurrentUserDto _currentUserHelper;
        public CompanionReserveBatchController(ICompanionReserveService companionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._companionReserveService = companionReserveService;
            this._currentUserHelper = currentUserHelper.CurrentUser;
        }

        /// <summary>
        ///  اطلاعات سبد رزرو
        /// </summary>
        /// <param name="id">شناسه سبد رزرو</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveBatchVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var batch = await _companionReserveService.FindBatchAsyncVDto(id, _currentUserHelper.UserId);
            return Ok(batch);
        }

        /// <summary>
        /// نهایی کردن سبد رزرو - ثبت همزمان چند رزرو (هر کدام برای یک خدمت متفاوت اما همگی از یک نمایندگی)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveBatchVDto>), 200)]
        public async Task<IActionResult> Post(CompanionReserveBatchInsertDto dto)
        {
            if (dto?.Items != null)
            {
                foreach (var item in dto.Items)
                {
                    item.BookerId = _currentUserHelper.UserId;
                }
            }
            var result = await _companionReserveService.InsertBatchAsyncDto(dto);
            return Ok(result);
        }
    }
}
