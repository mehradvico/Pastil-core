using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پت های نمایندگان
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class CompanionPetController : ControllerBase
    {
        private readonly ICompanionPetService CompanionPetService;
        /// <summary>
        /// مدیریت پت های نمایندگان
        /// </summary>

        public CompanionPetController(ICompanionPetService CompanionPetService)
        {
            this.CompanionPetService = CompanionPetService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionPet = await CompanionPetService.FindAsyncDto(id);
            return Ok(CompanionPet);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionPetSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionPetInputDto dto)
        {
            var search = CompanionPetService.Search(dto);
            return Ok(search);
        }
    }
}
