using Application.Common.Geography.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// جستجو در نقشه
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class MapSearchController : ControllerBase
    {
        private readonly IGeographyService _geographyService;
        public MapSearchController(IGeographyService geographyService)
        {
            _geographyService = geographyService;
        }

        /// <summary>
        ///  جستجو در نقشه
        /// </summary>
        /// <returns></returns> 
        // هر درخواست یک فراخوانی پولی/سهمیه‌دار به سرویس نقشه است و endpoint ناشناس بود؛ محدودیت per-IP (autocomplete عادی را خفه نمی‌کند)
        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("MapSearch")]
        [ProducesResponseType(typeof(TripSearchDto), 200)]
        public async Task<IActionResult> Post(string q)
        {
            var search = await _geographyService.SearchAsync(q);
            return Ok(search);
        }


    }
}
