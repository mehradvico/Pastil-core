using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت‌نام کاربر در دوره‌های مدرسه
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolReserveController : ControllerBase
    {
        private readonly ISchoolReserveService _schoolReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public SchoolReserveController(ISchoolReserveService schoolReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._schoolReserveService = schoolReserveService;
            this._currentUserHelper = currentUserHelper;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolReserveInputDto dto)
        {
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var search = _schoolReserveService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var reserve = await _schoolReserveService.FindAsyncVDto(id);
            if (!reserve.IsSuccess || reserve.Data?.BookerId != _currentUserHelper.CurrentUser.UserId)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(reserve);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveDto>), 200)]
        public async Task<IActionResult> Post(SchoolReserveDto dto)
        {
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var result = await _schoolReserveService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
