//using Application.Common.Dto.Result;
//using Application.Services.CompanionSrvs.CompanionSrv.Dto;
//using Application.Services.CompanionSrvs.CompanionSrv.Iface;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Api.Areas.Admin.Controllers
//{
//    /// <summary>
//    /// درصد سهم همکاران
//    /// </summary>
//    /// 
//    [Area("Admin")]
//    [Route("api/[area]/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class CompanionShareController : ControllerBase
//    {
//        private readonly ICompanionService _companionService;
//        public CompanionShareController(ICompanionService companionService)
//        {
//            this._companionService = companionService;
//        }

//        /// <summary>
//        ///  فعال سازی آیتم 
//        /// </summary>
//        /// <returns>
//        /// </returns>
//        [HttpPut]
//        [ProducesResponseType(typeof(BaseResultDto), 200)]
//        public IActionResult Put(CompanionShareDto dto)
//        {
//            var companion = _companionService.CompanionShareDto(dto);
//            return Ok(companion);
//        }
//    }
//}
