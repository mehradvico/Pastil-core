using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// انتخاب گزینه‌های خدمات آنلاین برای پکیج نماینده
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageOnlineSelectionController : ControllerBase
    {
        private readonly ICompanionAssistancePackageOnlineSelectionService _companionAssistancePackageOnlineSelectionService;
        public CompanionAssistancePackageOnlineSelectionController(ICompanionAssistancePackageOnlineSelectionService companionAssistancePackageOnlineSelectionService)
        {
            this._companionAssistancePackageOnlineSelectionService = companionAssistancePackageOnlineSelectionService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistancePackageOnlineSelectionSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackageOnlineSelectionInputDto dto)
        {
            var search = _companionAssistancePackageOnlineSelectionService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه انتخاب خدمت آنلاین</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistancePackageOnlineSelectionService.FindAsyncVDto(id);
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistancePackageOnlineSelectionDto dto)
        {
            var result = await _companionAssistancePackageOnlineSelectionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionAssistancePackageOnlineSelectionDto dto)
        {
            var agency = _companionAssistancePackageOnlineSelectionService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _companionAssistancePackageOnlineSelectionService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
