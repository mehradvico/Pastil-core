using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت حسابداری کلینیک ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceCompanionController : ControllerBase
    {
        private readonly IFinanceCompanionService _financeCompanionService;
        private readonly ICurrentUserHelper _currentUser;

        public FinanceCompanionController(IFinanceCompanionService financeCompanionService, ICurrentUserHelper currentUser)
        {
            _financeCompanionService = financeCompanionService;
            _currentUser = currentUser;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceCompanionVDto>), 200)]
        public IActionResult Get([FromQuery] FinanceCompanionInputDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var searchDto = _financeCompanionService.Search(dto);
            return Ok(searchDto);
        }
    }
}
