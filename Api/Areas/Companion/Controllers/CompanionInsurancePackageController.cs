using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پکیج های بیمه نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageController : ControllerBase
    {
        private readonly ICompanionInsurancePackageService _companionAssistanceUserService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionInsurancePackageController(ICompanionInsurancePackageService companionAssistanceUserService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
            this._currentUserHelper = currentUserHelper;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionInsurancePackageSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInsurancePackageInputDto dto)
        {
            dto.CompanionId = _currentUserHelper.CurrentUser.CompanionId;
            var search = _companionAssistanceUserService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        /// اطلاعات آیتم 
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
            if (!_currentUserHelper.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.Active = false;
            dto.CompanionId = _currentUserHelper.CurrentUser.CompanionId.Value;
            var result = await _companionAssistanceUserService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionInsurancePackageDto dto)
        {
            var existing = await _companionAssistanceUserService.FindAsyncDto(dto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            dto.Active = false;
            dto.CompanionId = existing.Data.CompanionId;
            var agency = _companionAssistanceUserService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _companionAssistanceUserService.FindAsyncDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _companionAssistanceUserService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
