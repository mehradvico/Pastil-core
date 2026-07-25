using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Operator.Controllers
{
    /// <summary>
    /// مدیریت  رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Operator")]
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
            dto.CompanionAssistanceUserId = _currentUserHelper.CurrentUser.UserId;
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
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionReserveService.FindAsyncVDto(id);
            return Ok(companion);
        }
    }
}
