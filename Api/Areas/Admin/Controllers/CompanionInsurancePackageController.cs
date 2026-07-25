using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageController : ControllerBase
    {
        private readonly ICompanionInsurancePackageService _companionAssistanceUserService;
        public CompanionInsurancePackageController(ICompanionInsurancePackageService companionAssistanceUserService)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
        }


        /// <summary>
        /// جستجو 
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionInsurancePackageSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInsurancePackageInputDto dto)
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
        [ProducesResponseType(typeof(BaseResultDto<CompanionInsurancePackageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceUserService.FindAsyncDto(id);
            return Ok(agency);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionInsurancePackageDto>), 200)]
        public async Task<IActionResult> Post(CompanionInsurancePackageDto dto)
        {
            var result = await _companionAssistanceUserService.InsertAsyncDto(dto);
            return Ok(result);
        }
        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionInsurancePackageDto dto)
        {
            var agency = _companionAssistanceUserService.UpdateDto(dto);
            return Ok(agency);
        }
        /// <summary>
        /// حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _companionAssistanceUserService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
