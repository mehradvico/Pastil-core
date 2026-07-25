using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت گزارشات تخلف نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReportController : ControllerBase
    {
        private readonly ICompanionReportService _CompanionReportService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionReportController(ICompanionReportService CompanionReportService, ICurrentUserHelper currentUser)
        {
            this._CompanionReportService = CompanionReportService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReportDto>), 200)]
        public async Task<IActionResult> Post(CompanionReportDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var result = await _CompanionReportService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
