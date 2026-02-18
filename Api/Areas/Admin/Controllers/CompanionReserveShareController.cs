//using Application.Common.Dto.Result;
//using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
//using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Api.Areas.Admin.Controllers
//{
//    /// <summary>
//    /// مدیریت تغییر وضعیت رزرو همکار
//    /// </summary>
//    /// 
//    [Area("Admin")]
//    [Route("api/[area]/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class CompanionReserveShareController : ControllerBase
//    {
//        private readonly ICompanionReserveService _companionReserveService;
//        public CompanionReserveShareController(ICompanionReserveService companionReserveService)
//        {
//            this._companionReserveService = companionReserveService;
//        }

//        /// <summary>
//        ///  ویرایش آیتم 
//        /// </summary>
//        /// <returns>
//        /// </returns>
//        [HttpPut]
//        [ProducesResponseType(typeof(BaseResultDto), 200)]
//        public async Task<IActionResult> Put(CompanionReserveShareDto dto)
//        {
//            var companion = await _companionReserveService.UpdateShareDto(dto);
//            return Ok(companion);
//        }
//    }
//}
