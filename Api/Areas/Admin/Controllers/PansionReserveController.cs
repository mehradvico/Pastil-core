using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رزروهای پانسیون
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        public PansionReserveController(IPansionReserveService PansionReserveService)
        {
            this._PansionReserveService = PansionReserveService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionReserveInputDto dto)
        {
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
            var Pansion = await _PansionReserveService.FindAsyncVDto(id);
            return Ok(Pansion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionReserveDto>), 200)]
        public async Task<IActionResult> Post(PansionReserveDto dto)
        {
            var result = await _PansionReserveService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PansionReserveDto dto)
        {
            var Pansion = _PansionReserveService.UpdateDto(dto);
            return Ok(Pansion);
        }
    }
}
