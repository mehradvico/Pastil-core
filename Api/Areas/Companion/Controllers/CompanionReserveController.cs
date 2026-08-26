using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public CompanionReserveController(ICompanionReserveService companionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._companionReserveService = companionReserveService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReserveInputDto dto)
        {
            dto.CompanionId = _currentUserHelper.CurrentUser.CompanionId;
            var search = _companionReserveService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveAdminVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionReserveService.FindAsyncAdminVDto(id);
            if (companion.IsSuccess && companion.Data?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionReserveAdminVDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(companion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveDto>), 200)]
        public async Task<IActionResult> Post(CompanionReserveDto dto)
        {
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var result = await _companionReserveService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
