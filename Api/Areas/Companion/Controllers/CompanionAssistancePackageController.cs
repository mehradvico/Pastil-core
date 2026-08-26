using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پکیج های خدماتی نمایندگان
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistancePackageController : ControllerBase
    {
        private readonly ICompanionAssistancePackageService _companionAssistancePackageService;
        private readonly ICompanionAssistanceService _companionAssistanceService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionAssistancePackageController(ICompanionAssistancePackageService companionAssistancePackageService, ICompanionAssistanceService companionAssistanceService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistancePackageService = companionAssistancePackageService;
            this._companionAssistanceService = companionAssistanceService;
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
        /// <param name="id">شناسه پکیج خدمات نمایندگان</param>
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
            var companionAssistance = await _companionAssistanceService.FindAsyncDto(dto.CompanionAssistanceId);
            if (!companionAssistance.IsSuccess || companionAssistance.Data?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistancePackageDto>(false, Resource.Notification.AccessDenied, dto));

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
            var existing = await _companionAssistancePackageService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            dto.Active = false;
            dto.CompanionAssistanceId = existing.Data.CompanionAssistanceId;
            var agency = await _companionAssistancePackageService.UpdateAsyncDto(dto);
            return Ok(agency);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _companionAssistancePackageService.FindAsyncVDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _companionAssistancePackageService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
