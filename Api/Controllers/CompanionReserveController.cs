using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت  رزرو نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionReserveController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionReserveController(ICompanionReserveService companionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._companionReserveService = companionReserveService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// تعداد رزروهای آیتم
        /// </summary>
        /// <param name="companionId">شناسه نماینده</param>
        /// <returns>تعداد رزروها</returns>
        [HttpGet("{companionId}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> GetCount(long companionId)
        {
            var count = await _companionReserveService.ReserveCountAsync(companionId);
            return Ok(count);
        }

    }
}
