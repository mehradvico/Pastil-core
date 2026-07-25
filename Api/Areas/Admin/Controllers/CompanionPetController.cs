using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پت های نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionPetController : ControllerBase
    {
        private readonly ICompanionPetService CompanionPetService;
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

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Post(CompanionPetDto CompanionPetDto)
        {
            var result = await CompanionPetService.InsertAsyncDto(CompanionPetDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public IActionResult Put(CompanionPetDto CompanionPetDto)
        {
            var result = CompanionPetService.UpdateDto(CompanionPetDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionPetService.DeleteDto(id);
            return Ok(result);
        }
    }
}
