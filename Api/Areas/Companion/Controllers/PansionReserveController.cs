using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Pansion.Controllers
{
    /// <summary>
    /// مدیریت رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [Route("api/Comapnion/[controller]")] // مسیر قدیمی (غلط املایی) موقتاً برای کلاینت‌های فعلی
    [ApiController]
    [Authorize]
    [Api.Filters.RequireCompanionScope]
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
            dto.CompanionId = _currentUserHelper.CurrentUser.CompanionId;
            var search = _PansionReserveService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Pansion = await _PansionReserveService.FindAsyncVDto(id, companionId: _currentUserHelper.CurrentUser.CompanionId);
            return Ok(Pansion);
        }
    }
}
