using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کاربران
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private IUserService userService;
        private ICurrentUserHelper _currentUserHelper;
        /// <summary>
        /// مدیریت کاربران
        /// </summary>
        ///
        public UserController(IUserService user, ICurrentUserHelper currentUserHelper)
        {
            this.userService = user;
            this._currentUserHelper = currentUserHelper;
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
            var item = await userService.FindAsyncDto(_currentUserHelper.CurrentUser.UserId);
            return Ok(item);
        }

        /// <summary>
        /// ویرایش آیتم 
        /// </summary>  
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(UserDto userDto)
        {
            userDto.Id = _currentUserHelper.CurrentUser.UserId;
            var dto = userService.UpdateDto(userDto);
            return Ok(dto);
        }
    }
}
