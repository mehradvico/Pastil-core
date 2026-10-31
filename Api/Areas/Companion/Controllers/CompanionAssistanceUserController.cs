using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت کاربران خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceUserController : ControllerBase
    {
        private readonly ICompanionAssistanceUserService _companionAssistanceUserService;
        private readonly ICompanionAssistanceService _companionAssistanceService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionAssistanceUserController(ICompanionAssistanceUserService companionAssistanceUserService, ICompanionAssistanceService companionAssistanceService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistanceUserService = companionAssistanceUserService;
            this._companionAssistanceService = companionAssistanceService;
            this._currentUserHelper = currentUserHelper;
        }


        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceUserInputDto dto)
        {
            // بدون این، جستجو نیروهای همه‌ی مراکز را (با موبایل/ایمیل) برمی‌گرداند
            if (!_currentUserHelper.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUserHelper.CurrentUser.CompanionId.Value;
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
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceUserDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _companionAssistanceUserService.FindAsyncVDto(id);
            if (agency.IsSuccess && agency.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistanceUserVDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceUserDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistanceUserDto dto)
        {
            // خدمت باید مال مرکز همین نماینده باشد (سرویس فقط عضویت کاربرِ هدف در مرکزِ خدمت را چک می‌کند)
            var assistance = await _companionAssistanceService.FindAsyncDto(dto.CompanionAssistanceId);
            if (!assistance.IsSuccess || assistance.Data == null || !_currentUserHelper.CurrentUser.CompanionId.HasValue ||
                assistance.Data.CompanionId != _currentUserHelper.CurrentUser.CompanionId.Value)
                return Ok(new BaseResultDto<CompanionAssistanceUserDto>(false, Resource.Notification.AccessDenied, dto));
            dto.Active = false;
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
        public async Task<IActionResult> Put(CompanionAssistanceUserDto dto)
        {
            var existing = await _companionAssistanceUserService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            dto.Active = false;
            dto.CompanionAssistanceId = existing.Data!.CompanionAssistanceId;
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
            var existing = await _companionAssistanceUserService.FindAsyncVDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _companionAssistanceUserService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
