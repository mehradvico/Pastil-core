using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خدمات نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
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
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionAssistanceService.FindAsyncDto(id);
            return Ok(companion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionAssistanceDto>), 200)]
        public async Task<IActionResult> Post(CompanionAssistanceDto dto)
        {
            dto.Active = true;
            dto.Approved = true;
            var result = await _companionAssistanceService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionAssistanceDto dto)
        {
            var companion = await _companionAssistanceService.UpdateAsyncDto(dto);
            return Ok(companion);
        }

        /// <summary>
        /// حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _companionAssistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
