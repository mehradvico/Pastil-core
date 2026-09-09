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

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var reserve = await _schoolReserveService.FindAsyncVDto(id);
            return Ok(reserve);
        }
    }
}
