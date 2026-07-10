using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.LocationFields.CountrySrv.Dto;
using Application.Services.LocationFields.CountrySrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیریت کشورها
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private ICountryService CountryService;
        /// <summary>
        /// مدیریت کشورها
        /// </summary>
        ///
        public CountryController(ICountryService CountryService)
        {
            this.CountryService = CountryService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CountryVDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            dto.PageSize = 50;
            var searchDto = CountryService.Search(dto);
            return Ok(searchDto);
        }
    }
}
