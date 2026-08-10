using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت رزرو نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public PansionReserveController(IPansionReserveService PansionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._PansionReserveService = PansionReserveService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionReserveInputDto dto)
        {
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var search = _PansionReserveService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه رزرو نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Pansion = await _PansionReserveService.FindAsyncVDto(
                id,
                _currentUserHelper.CurrentUser.UserId);
            return Ok(Pansion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionReserveDto>), 200)]
        public async Task<IActionResult> Post(PansionReserveDto dto)
        {
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var result = await _PansionReserveService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
