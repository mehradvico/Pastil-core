using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با سایت مپ پانسیون‌ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PansionSiteMapController : ControllerBase
    {
        private IPansionService pansionService;
        /// <summary>
        /// مرتبط با سایت مپ پانسیون‌ها
        /// </summary>
        public PansionSiteMapController(IPansionService pansionService)
        {
            this.pansionService = pansionService;
        }
        /// <summary>
        /// سایت مپ پانسیون‌ها
        /// </summary>
        /// <returns></returns>
        ///
        [HttpGet()]
        [CustomOutputCache(CacheTypeEnum.PansionSiteMap)]
        [ProducesResponseType(typeof(BaseResultDto<PansionSiteMapDto>), 200)]
        public IActionResult Get()
        {
            var list = pansionService.GetSiteMap();
            return Ok(list);
        }

    }
}
