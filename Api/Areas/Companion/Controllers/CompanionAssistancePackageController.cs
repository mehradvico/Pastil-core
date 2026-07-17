using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پکیج های خدماتی همکاران
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageController : ControllerBase
    {
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionAssistancePackageController(ICompanionAssistancePackageService companionAssistancePackageService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistancePackageService = companionAssistancePackageService;
            this._currentUserHelper = currentUserHelper;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistancePackageSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackageInputDto dto)
        {

            var search = _companionAssistancePackageService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        /// اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پکیج خدمات همکاران</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistancePackageService.FindAsyncVDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistancePackageDto dto)
        {
            dto.Active = false;
            var result = await _companionAssistancePackageService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistancePackageDto dto)
        {
            dto.Active = false;
            var agency = await _companionAssistancePackageService.UpdateAsyncDto(dto);
            return Ok(agency);
        }

        /// <summary>
        /// حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _companionAssistancePackageService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
