using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت انواع نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionTypeController : ControllerBase
    {
        private readonly ICompanionTypeService CompanionTypeService;
        /// <summary>
        /// مدیریت انواع نمایندگان
        /// </summary>

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
    }
}
