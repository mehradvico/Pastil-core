using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// انواع نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionTypeController : ControllerBase
    {
        private readonly ICompanionTypeService CompanionTypeService;
        public CompanionTypeController(ICompanionTypeService CompanionTypeService)
        {
            this.CompanionTypeService = CompanionTypeService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionType = await CompanionTypeService.FindAsyncDto(id);
            return Ok(CompanionType);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionTypeSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionTypeInputDto dto)
        {
            var search = CompanionTypeService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public async Task<IActionResult> Post(CompanionTypeDto CompanionTypeDto)
        {
            var result = await CompanionTypeService.InsertAsyncDto(CompanionTypeDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public IActionResult Put(CompanionTypeDto CompanionTypeDto)
        {
            var result = CompanionTypeService.UpdateDto(CompanionTypeDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionTypeService.DeleteDto(id);
            return Ok(result);
        }
    }
}
