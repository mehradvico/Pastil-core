using Application.Common.Dto.Result;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پت های کاربر
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetController : ControllerBase
    {
        private IUserPetService UserPetService;
        public UserPetController(IUserPetService UserPetService)
        {
            this.UserPetService = UserPetService;
        }
        /// <summary>
        ///  اطلاعات پت 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await UserPetService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(UserPetInputDto), 200)]
        public IActionResult Get([FromQuery] UserPetInputDto dto)
        {
            var searchDto = UserPetService.Search(dto);
            return Ok(searchDto);
        }
    }
}
