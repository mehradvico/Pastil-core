using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// تخصیص رزرو خدمت به کاربر نمایندگی
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveAssignController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;

        public CompanionReserveAssignController(ICompanionReserveService companionReserveService)
        {
            _companionReserveService = companionReserveService;
        }

        /// <summary>
        /// تخصیص یا تغییر مسئول انجام رزرو
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveAdminVDto>), 200)]
        public async Task<IActionResult> Put(CompanionReserveAssignDto dto)
        {
            return Ok(await _companionReserveService.AssignCompanionReserveAsync(dto));
        }
    }
}
