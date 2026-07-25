using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت خدمات نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionAssistanceController : ControllerBase
    {
        private readonly ICompanionAssistanceService _companionAssistanceService;
        public CompanionAssistanceController(ICompanionAssistanceService companionAssistanceService)
        {
            this._companionAssistanceService = companionAssistanceService;
        }


        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionAssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionAssistanceInputDto dto)
        {
            dto.Available = true;
            var search = _companionAssistanceService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات نمایندگان</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionAssistanceService.FindAsyncVDto(id);
            return Ok(companion);
        }
    }
}
