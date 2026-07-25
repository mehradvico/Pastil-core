using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پت های پانسیون
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionPetController : ControllerBase
    {
        private readonly IPansionPetService _PansionPetService;
        public PansionPetController(IPansionPetService PansionPetService)
        {
            this._PansionPetService = PansionPetService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionPetSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionPetInputDto dto)
        {
            var search = _PansionPetService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _PansionPetService.FindAsyncDto(id);
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionPetDto>), 200)]
        public async Task<IActionResult> Post(PansionPetDto dto)
        {
            var result = await _PansionPetService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _PansionPetService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
