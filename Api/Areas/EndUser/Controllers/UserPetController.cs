using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کاربری پت ها
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetController : ControllerBase
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private IUserPetService _userPetService;
        /// <summary>
        /// مدیریت کاربری پت ها
        /// </summary>
        ///
        public UserPetController(IUserPetService userPetService, ICurrentUserHelper currentUserHelper)
        {
            _userPetService = userPetService;
            _currentUserHelper = currentUserHelper;
        }
        /// <summary>
        ///  اطلاعات پت 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _userPetService.FindAsyncVDto(id);
            if (role.IsSuccess && role.Data?.UserId != _currentUserHelper.CurrentUser.UserId)
                return Ok(new BaseResultDto<UserPetVDto>(false, Resource.Notification.AccessDenied, default));
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
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            dto.Available = true;
            var searchDto = _userPetService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserPetDto>), 200)]
        public async Task<IActionResult> Post(UserPetDto UserPetDto)
        {
            UserPetDto.UserId = _currentUserHelper.CurrentUser.UserId;
            var dto = await _userPetService.InsertAsyncDto(UserPetDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(UserPetDto), 200)]
        public async Task<IActionResult> Put(UserPetDto UserPetDto)
        {
            UserPetDto.UserId = _currentUserHelper.CurrentUser.UserId;
            var dto = await _userPetService.UpdateAsyncDto(UserPetDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _userPetService.FindAsyncVDto(id);
            if (!existing.IsSuccess || existing.Data?.UserId != _currentUserHelper.CurrentUser.UserId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var dto = _userPetService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
