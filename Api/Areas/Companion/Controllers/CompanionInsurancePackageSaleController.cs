using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت فروش پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageSaleController : ControllerBase
    {
        private readonly ICompanionInsurancePackageSaleService _companionInsurancePackageSaleService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionInsurancePackageSaleController(ICompanionInsurancePackageSaleService companionInsurancePackageSaleService, ICurrentUserHelper currentUser)
        {
            this._companionInsurancePackageSaleService = companionInsurancePackageSaleService;
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
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var search = _companionInsurancePackageSaleService.Search(dto);
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
            var agency = await _companionInsurancePackageSaleService.FindAsyncDto(id);
            if (agency.IsSuccess && agency.Data?.CompanionInsurancePackage?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionInsurancePackageSaleDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(agency);
        }
    }
}
