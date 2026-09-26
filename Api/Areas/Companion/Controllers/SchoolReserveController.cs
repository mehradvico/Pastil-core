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
        public SchoolReserveController(ISchoolReserveService schoolReserveService, ICurrentUserHelper currentUser)
        {
            this._schoolReserveService = schoolReserveService;
            this._currentUser = currentUser;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolReserveInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var search = _schoolReserveService.Search(dto);
            return Ok(search);
        }

        /// <summary>ثبت «کامل‌شده» برای ثبت‌نام پرداخت‌شده‌ی دوره‌ی مدرسه‌ی خودِ نماینده</summary>
        [HttpPut("{id}/Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id)
        {
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _schoolReserveService.CompleteByCompanionAsync(id, companionId.Value));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var reserve = await _schoolReserveService.FindAsyncVDto(id);
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!reserve.IsSuccess || !companionId.HasValue ||
                reserve.Data?.SchoolCourse?.School?.CompanionId != companionId.Value)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(reserve);
        }
    }
}
