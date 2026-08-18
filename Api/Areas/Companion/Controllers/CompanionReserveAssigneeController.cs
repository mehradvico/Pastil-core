using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// کاربران قابل تخصیص به رزرو خدمت
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveAssigneeController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;

        public CompanionReserveAssigneeController(ICompanionReserveService companionReserveService)
        {
            _companionReserveService = companionReserveService;
        }

        /// <summary>
        /// دریافت کاربران فعال و تأییدشده همان خدمت برای تخصیص رزرو
        /// </summary>
        [HttpGet("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<CompanionReserveAssigneeVDto>>), 200)]
        public async Task<IActionResult> Get(long reserveId)
        {
            return Ok(await _companionReserveService.GetCompanionReserveAssigneesAsync(reserveId));
        }
    }
}
