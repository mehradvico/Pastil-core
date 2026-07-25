using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فروش پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageSaleController : ControllerBase
    {
        private readonly ICompanionInsurancePackageSaleService _companionAssistanceUserService;
        public CompanionInsurancePackageSaleController(ICompanionInsurancePackageSaleService companionAssistanceUserService)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
        }


        /// <summary>
        /// جستجو 
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionInsurancePackageSaleSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInsurancePackageSaleInputDto dto)
        {
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
            var agency = await _companionAssistanceUserService.FindAsyncDto(id);
            return Ok(agency);
        }
    }
}
