using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.LocationFields.CountrySrv.Dto;
using Application.Services.LocationFields.CountrySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کشور ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CountryController : ControllerBase
    {
        private ICountryService countryService;
        public CountryController(ICountryService countryService)
        {
            this.countryService = countryService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CountryDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await countryService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CountryVDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = countryService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CountryDto>), 200)]
        public async Task<IActionResult> Post(CountryDto CountryDto)
        {

            var dto = await countryService.InsertAsyncDto(CountryDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CountryDto CountryDto)
        {
            var dto = countryService.UpdateDto(CountryDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = countryService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
