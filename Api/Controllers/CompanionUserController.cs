using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت تصویر محصول ها
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionUserController : ControllerBase
    {
        private readonly ICompanionUserService CompanionUserService;
        /// <summary>
        /// مدیریت تصویر محصول ها
        /// </summary>

        public CompanionUserController(ICompanionUserService CompanionUserService)
        {
            this.CompanionUserService = CompanionUserService;
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CompanionUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionUserInputDto dto)
        {
            var CompanionUser = CompanionUserService.SearchPublic(dto);
            return Ok(CompanionUser);
        }
    }
}
