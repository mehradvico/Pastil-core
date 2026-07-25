using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت گزارشات تخلف خدمات نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceReportController : ControllerBase
    {
        private readonly ICompanionAssistanceReportService _CompanionAssistanceReportService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionAssistanceReportController(ICompanionAssistanceReportService CompanionAssistanceReportService, ICurrentUserHelper currentUser)
        {
            this._CompanionAssistanceReportService = CompanionAssistanceReportService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceReportDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistanceReportDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var result = await _CompanionAssistanceReportService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
