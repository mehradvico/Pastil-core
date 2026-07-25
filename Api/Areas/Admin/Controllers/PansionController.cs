using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پانسیون ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        public PansionController(IPansionService PansionService)
        {
            this._PansionService = PansionService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionInputDto dto)
        {
            var search = _PansionService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پانسیون</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Pansion = await _PansionService.FindAsyncVDto(id);
            return Ok(Pansion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Post(PansionDto dto)
        {
            var result = await _PansionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PansionDto dto)
        {
            var Pansion = _PansionService.UpdateDto(dto);
            return Ok(Pansion);
        }
    }
}
