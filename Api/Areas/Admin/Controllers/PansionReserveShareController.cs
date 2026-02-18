//using Application.Common.Dto.Result;
//using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
//using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Api.Areas.Admin.Controllers
//{
//    /// <summary>
//    /// مدیریت محاسبه درصد رزرو پانسیون
//    /// </summary>
//    /// 
//    [Area("Admin")]
//    [Route("api/[area]/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class PansionReserveShareController : ControllerBase
//    {
//        private readonly IPansionReserveService _PansionReserveService;
//        public PansionReserveShareController(IPansionReserveService PansionReserveService)
//        {
//            this._PansionReserveService = PansionReserveService;
//        }

//        /// <summary>
//        ///  ویرایش آیتم 
//        /// </summary>
//        /// <returns>
//        /// </returns>
//        [HttpPut]
//        [ProducesResponseType(typeof(BaseResultDto), 200)]
//        public async Task<IActionResult> Put(PansionReserveShareDto dto)
//        {
//            var Pansion = await _PansionReserveService.UpdateShareDto(dto);
//            return Ok(Pansion);
//        }
//    }
//}
