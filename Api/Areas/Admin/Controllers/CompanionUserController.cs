using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران نماینده
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUserController : ControllerBase
    {
        private readonly ICompanionUserService CompanionUserService;
        public CompanionUserController(ICompanionUserService CompanionUserService)
        {
            this.CompanionUserService = CompanionUserService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionUser = await CompanionUserService.FindAsyncDto(id);
            return Ok(CompanionUser);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionUserInputDto dto)
        {
            var search = CompanionUserService.SearchDto(dto);
            return Ok(search);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public async Task<IActionResult> Post(CompanionUserDto CompanionUserDto)
        {
            var result = await CompanionUserService.InsertAsyncDto(CompanionUserDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public IActionResult Put(CompanionUserDto CompanionUserDto)
        {
            var result = CompanionUserService.UpdateDto(CompanionUserDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionUserService.DeleteDto(id);
            return Ok(result);
        }
    }
}
