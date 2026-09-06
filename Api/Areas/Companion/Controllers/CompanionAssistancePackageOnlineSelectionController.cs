using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت انتخاب گزینه‌های خدمات آنلاین برای پکیج‌های نماینده
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageOnlineSelectionController : ControllerBase
    {
        private readonly ICompanionAssistancePackageOnlineSelectionService _companionAssistancePackageOnlineSelectionService;
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionAssistancePackageOnlineSelectionController(
            ICompanionAssistancePackageOnlineSelectionService companionAssistancePackageOnlineSelectionService,
            ICompanionAssistancePackageService companionAssistancePackageService,
            ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistancePackageOnlineSelectionService = companionAssistancePackageOnlineSelectionService;
            this._companionAssistancePackageService = companionAssistancePackageService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// جستجو
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
        /// اطلاعات آیتم
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
            var companionAssistancePackage = await _companionAssistancePackageService.FindAsyncVDto(dto.CompanionAssistancePackageId);
            if (!companionAssistancePackage.IsSuccess || companionAssistancePackage.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistancePackageOnlineSelectionDto>(false, Resource.Notification.AccessDenied, dto));

            dto.Active = false;
            var result = await _companionAssistancePackageOnlineSelectionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistancePackageOnlineSelectionDto dto)
        {
            var existing = await _companionAssistancePackageOnlineSelectionService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistancePackage?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            dto.Active = false;
            dto.CompanionAssistancePackageId = existing.Data!.CompanionAssistancePackageId;
            var agency = _companionAssistancePackageOnlineSelectionService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _companionAssistancePackageOnlineSelectionService.FindAsyncVDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistancePackage?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _companionAssistancePackageOnlineSelectionService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
