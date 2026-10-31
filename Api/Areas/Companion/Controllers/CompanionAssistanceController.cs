using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت خدمات نمایندگان 
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionAssistanceController : ControllerBase
    {
        private readonly ICompanionAssistanceService _companionAssistanceService;
        private readonly CurrentUserDto _currentUserDto;
        public CompanionAssistanceController(ICompanionAssistanceService companionAssistanceService, ICurrentUserHelper currentUserHelper)
        {
            this._companionAssistanceService = companionAssistanceService;
            _currentUserDto = currentUserHelper.CurrentUser;
        }


        /// <summary>
        /// جستجو 
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceInputDto dto)
        {

            // CompanionId خالی یعنی سرویس فیلتر مرکز را نمی‌گذارد و رکورد همه‌ی مراکز برمی‌گردد
            if (!_currentUserDto.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUserDto.CompanionId;
            var search = _companionAssistanceService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {

            var companion = await _companionAssistanceService.FindAsyncDto(id);
            if (companion.IsSuccess && companion.Data?.CompanionId != _currentUserDto.CompanionId)
                return Ok(new BaseResultDto<CompanionAssistanceDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(companion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistanceDto dto)
        {
            if (!_currentUserDto.CompanionId.HasValue)
                return Forbid();
            dto.Active = false;
            dto.CompanionId = _currentUserDto.CompanionId.Value;
            // تأیید خدمت فقط با ادمین (سرویس Approved را از DTO می‌گیرد)
            dto.Approved = false;
            var result = await _companionAssistanceService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistanceDto dto)
        {
            dto.Active = false;
            // CompanionId خالی (مثلاً درخواست نمایندگیِ تأییدنشده) یعنی سرویس چک مالکیت را رد می‌کند؛ پس اینجا حتماً لازم است
            if (!_currentUserDto.CompanionId.HasValue)
                return Forbid();
            var companion = await _companionAssistanceService.UpdateAsyncDto(dto, _currentUserDto.CompanionId.Value);
            return Ok(companion);
        }

        /// <summary>
        ///  حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _companionAssistanceService.FindAsyncDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _currentUserDto.CompanionId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var dto = _companionAssistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
