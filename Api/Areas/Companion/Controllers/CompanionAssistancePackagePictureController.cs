using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackagePictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت تصویر پکیج ها
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackagePictureController : ControllerBase
    {
        private readonly ICompanionAssistancePackagePictureService companionassistancepackagePictureService;
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت تصویر محصول ها
        /// </summary>

        public CompanionAssistancePackagePictureController(ICompanionAssistancePackagePictureService companionassistancepackagePictureService, ICompanionAssistancePackageService companionAssistancePackageService, ICurrentUserHelper currentUser)
        {
            this.companionassistancepackagePictureService = companionassistancepackagePictureService;
            this._companionAssistancePackageService = companionAssistancePackageService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companionassistancepackagePicture = await companionassistancepackagePictureService.FindAsyncDto(id);
            return Ok(companionassistancepackagePicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CompanionAssistancePackagePictureSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistancePackagePictureInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var companionassistancepackagePicture = companionassistancepackagePictureService.SearchDto(dto);
            return Ok(companionassistancepackagePicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistancePackagePictureDto companionassistancepackagePictureDto)
        {
            var package = await _companionAssistancePackageService.FindAsyncVDto(companionassistancepackagePictureDto.CompanionAssistancePackageId);
            if (!package.IsSuccess || package.Data?.CompanionAssistance?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistancePackagePictureDto>(false, Resource.Notification.AccessDenied, companionassistancepackagePictureDto));

            var result = await companionassistancepackagePictureService.InsertAsyncDto(companionassistancepackagePictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Put(CompanionAssistancePackagePictureDto companionassistancepackagePictureDto)
        {
            var existing = await companionassistancepackagePictureService.FindAsyncDto(companionassistancepackagePictureDto.Id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<CompanionAssistancePackagePictureDto>(false, Resource.Notification.AccessDenied, companionassistancepackagePictureDto));
            var package = await _companionAssistancePackageService.FindAsyncVDto(existing.Data.CompanionAssistancePackageId);
            if (!package.IsSuccess || package.Data?.CompanionAssistance?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistancePackagePictureDto>(false, Resource.Notification.AccessDenied, companionassistancepackagePictureDto));

            companionassistancepackagePictureDto.CompanionAssistancePackageId = existing.Data.CompanionAssistancePackageId;
            var result = companionassistancepackagePictureService.UpdateDto(companionassistancepackagePictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistancePackagePictureDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await companionassistancepackagePictureService.FindAsyncDto(id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<CompanionAssistancePackagePictureDto>(false, Resource.Notification.AccessDenied, default));
            var package = await _companionAssistancePackageService.FindAsyncVDto(existing.Data.CompanionAssistancePackageId);
            if (!package.IsSuccess || package.Data?.CompanionAssistance?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistancePackagePictureDto>(false, Resource.Notification.AccessDenied, default));

            var result = companionassistancepackagePictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
