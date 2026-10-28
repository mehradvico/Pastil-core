using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ثبت‌نام‌های دوره‌های مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly ISchoolOwnershipService _ownership;
        private readonly Application.Services.SchoolSrvs.SchoolTeachingSrv.ISchoolTeachingService _teaching;
        public SchoolReserveController(ISchoolReserveService schoolReserveService, ICurrentUserHelper currentUser, ISchoolOwnershipService ownership,
            Application.Services.SchoolSrvs.SchoolTeachingSrv.ISchoolTeachingService teaching)
        {
            this._teaching = teaching;
            this._ownership = ownership;
            this._schoolReserveService = schoolReserveService;
            this._currentUser = currentUser;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolReserveInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            var search = _schoolReserveService.Search(dto);
            return Ok(search);
        }

        /// <summary>ثبت «کامل‌شده» برای ثبت‌نام پرداخت‌شده‌ی دوره‌ی مدرسه‌ی خودِ نماینده</summary>
        [HttpPut("{id}/Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id)
        {
            var ownedSchoolId = await _ownership.SchoolOfReserveAsync(id, _currentUser.CurrentUser.UserId);
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolReserveService.CompleteByCompanionAsync(id, ownedSchoolId.Value);
            // دوره‌ی دارای گواهی: گواهی پایان دوره خودکار صادر و به مالک پت پوش می‌شود
            if (result.IsSuccess)
                await _teaching.IssueCertificateIfEnabledAsync(id, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var reserve = await _schoolReserveService.FindAsyncVDto(id);
            if (!reserve.IsSuccess || await _ownership.SchoolOfReserveAsync(id, _currentUser.CurrentUser.UserId, approvedOnly: false) == null)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(reserve);
        }
    }
}
