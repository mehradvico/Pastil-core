using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت  خدمات
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class AssistanceController : ControllerBase
    {
        private readonly IAssistanceService _assistanceService;
        public AssistanceController(IAssistanceService assistanceService)
        {
            this._assistanceService = assistanceService;
        }


        /// <summary>
        ///  جستجو خدمات
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(AssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceInputDto dto)
        {
            dto.Available = true;
            dto.IsGrooming ??= false;
            var search = _assistanceService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات خدمت 
        /// </summary>
        /// <param name="id">شناسه خدمت</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var assistance = await _assistanceService.FindAsyncVDto(id);
            return Ok(assistance);
        }
    }
}
