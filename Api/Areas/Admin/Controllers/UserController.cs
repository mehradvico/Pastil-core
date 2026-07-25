using Application.Common.Dto.Result;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private IUserService userService;
        public UserController(IUserService user)
        {
            this.userService = user;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await userService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(UserSearchDto), 200)]

        public IActionResult Get([FromQuery] UserInputDto search)
        {
            var list = userService.Search(search);
            return Ok(list);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserDto>), 200)]

        public async Task<IActionResult> Post(UserDto userDto)
        {
            var dto = await userService.InsertAsyncDto(userDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم 
        /// </summary>  
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(UserDto userDto)
        {
            var dto = userService.UpdateDto(userDto);
            return Ok(dto);
        }
        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = userService.DeleteDto(id);
            return Ok(dto);
        }
    }

}
