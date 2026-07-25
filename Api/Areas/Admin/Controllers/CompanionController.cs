using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionController : ControllerBase
    {
        private readonly ICompanionService _companionService;
        public CompanionController(ICompanionService companionService)
        {
            this._companionService = companionService;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionInputDto dto)
        {
            var search = _companionService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionService.FindAsyncVDto(id);
            return Ok(companion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionDto>), 200)]
        public async Task<IActionResult> Post(CompanionDto dto)
        {
            var result = await _companionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionDto dto)
        {
            var companion = await _companionService.UpdateAsyncDto(dto);
            return Ok(companion);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _companionService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
