using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خدمات
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistanceController : ControllerBase
    {
        private readonly IAssistanceService _assistanceService;
        public AssistanceController(IAssistanceService assistanceService)
        {
            this._assistanceService = assistanceService;
        }

        /// <summary>
        ///  جستجو خدمت
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(AssistanceSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceInputDto dto)
        {
            var search = _assistanceService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات خدمت
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _assistanceService.FindAsyncVDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// خدمت جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceDto>), 200)]
        public async Task<IActionResult> Post(AssistanceDto dto)
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
        public IActionResult Put(AssistanceDto dto)
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