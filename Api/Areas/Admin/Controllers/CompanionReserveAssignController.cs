using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخصیص رزرو خدمت به کاربر نمایندگی توسط مدیر
    /// </summary>
    [Area("Admin")]
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
        /// تخصیص یا تغییر مسئول انجام رزرو توسط مدیر
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveAdminVDto>), 200)]
        public async Task<IActionResult> Put(CompanionReserveAssignDto dto)
        {
            return Ok(await _companionReserveService.AssignCompanionReserveAsync(dto, adminAccess: true));
        }
    }
}
