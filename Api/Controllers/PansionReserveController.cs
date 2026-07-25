using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت  رزرو پانسیون
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class PansionReserveController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public PansionReserveController(IPansionReserveService PansionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._PansionReserveService = PansionReserveService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// تعداد رزروهای آیتم
        /// </summary>
        /// <param name="PansionId">شناسه نماینده</param>
        /// <returns>تعداد رزروها</returns>
        [HttpGet("{PansionId}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> GetCount(long PansionId)
        {
            var count = await _PansionReserveService.ReserveCountAsync(PansionId);
            return Ok(count);
        }

    }
}
