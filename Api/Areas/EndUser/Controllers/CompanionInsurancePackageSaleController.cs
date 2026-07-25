using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReportSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت فروش پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageSaleController : ControllerBase
    {
        private readonly ICompanionInsurancePackageSaleService _companionAssistanceUserService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionInsurancePackageSaleController(ICompanionInsurancePackageSaleService companionAssistanceUserService, ICurrentUserHelper currentUser)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
            this._currentUser = currentUser;
        }


        /// <summary>
        /// جستجو 
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionInsurancePackageSaleSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInsurancePackageSaleInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var search = _companionAssistanceUserService.Search(dto);
            return Ok(search);
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه کاربر خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionInsurancePackageSaleDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceUserService.FindAsyncVDto(id);
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionInsurancePackageSaleDto>), 200)]
        public async Task<IActionResult> Post(CompanionInsurancePackageSaleDto dto)
        {
            var result = await _companionAssistanceUserService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
