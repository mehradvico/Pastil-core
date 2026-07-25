using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripOptionSrv.Dto;
using Application.Services.TripSrv.TripOptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزینه های سفر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripOptionController : ControllerBase
    {
        private readonly ITripOptionService _assistanceService;
        public TripOptionController(ITripOptionService assistanceService)
        {
            this._assistanceService = assistanceService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(TripOptionSearchDto), 200)]
        public IActionResult Get([FromQuery] TripOptionInputDto dto)
        {
            var search = _assistanceService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripOptionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _assistanceService.FindAsyncDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripOptionDto>), 200)]
        public async Task<IActionResult> Post(TripOptionDto dto)
        {
            var result = await _assistanceService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(TripOptionDto dto)
        {
            var agency = _assistanceService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _assistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
