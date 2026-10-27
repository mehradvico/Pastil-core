using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت مدرسه‌ی خودِ نماینده (مربی)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly ISchoolOwnershipService _ownership;
        public SchoolController(ISchoolService schoolService, ICurrentUserHelper currentUser, ISchoolOwnershipService ownership)
        {
            this._ownership = ownership;
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            var search = _schoolService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var school = await _schoolService.FindAsyncVDto(id);
            if (!school.IsSuccess || await _ownership.SchoolOfSchoolAsync(id, _currentUser.CurrentUser.UserId, approvedOnly: false) == null)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(school);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDto>), 200)]
        public async Task<IActionResult> Post(SchoolDto dto)
        {
            // مالک مدرسه‌ی جدید = ارائه‌دهنده‌ی کاربر (نقش School)؛ نمایندگی اختیاری/قدیمی. خطا با پیام روشن، نه ۴۰۳ خالی.
            var owner = await _ownership.ResolveOwnerForNewSchoolAsync(_currentUser.CurrentUser.UserId, dto.CompanionId);
            if (!owner.Ok)
                return Ok(new BaseResultDto<SchoolDto>(false, owner.Error, dto));
            dto.ProviderId = owner.ProviderId;
            dto.CompanionId = owner.CompanionId;
            var result = await _schoolService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolDto dto)
        {
            var ownedSchoolId = await _ownership.SchoolOfSchoolAsync(dto.Id, _currentUser.CurrentUser.UserId);
            var existing = await _schoolService.FindAsyncVDto(dto.Id);
            if (!ownedSchoolId.HasValue || !existing.IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));

            // مالکیت (ارائه‌دهنده/نمایندگی) فقط با ادمین عوض می‌شود
            dto.CompanionId = existing.Data.CompanionId;
            dto.ProviderId = existing.Data.ProviderId;

            // UpdateDto عمومی همه‌ی فیلدهای DTO را روی موجودیت می‌نویسد؛ فیلدهای تأیید/نمایش/امتیاز فقط با
            // ادمین یا سیستم عوض می‌شوند (Approve/ApprovalValue با SchoolApprove، Active با SchoolActive،
            // ShowToSite/Suggested با ادمین). همیشه مقدار ذخیره‌شده برگردانده می‌شود.
            dto.Approve = existing.Data.Approve;
            dto.ApprovalValue = existing.Data.ApprovalValue;
            dto.Active = existing.Data.Active;
            dto.ShowToSite = existing.Data.ShowToSite;
            dto.Suggested = existing.Data.Suggested;
            dto.CommentCount = existing.Data.CommentCount;
            dto.RateAvg = existing.Data.RateAvg;
            dto.RateCount = existing.Data.RateCount;

            var result = _schoolService.UpdateDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف نرم مدرسه‌ی من (فقط مالک کلینیک). دوره‌ها، رزروها و گزارش مالی می‌مانند؛ با ثبت‌نام باز
        /// پرداخت‌شده حذف نمی‌شود. بازگردانی فقط توسط ادمین.
        /// </summary>
        /// <param name="id">شناسه مدرسه</param>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var ownedSchoolId = await _ownership.SchoolOfSchoolAsync(id, _currentUser.CurrentUser.UserId);
            if (!ownedSchoolId.HasValue)
                return Forbid();
            var result = await _schoolService.SoftDeleteAsync(id, ownedSchoolId.Value, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
