using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Accounting.UserPetPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت تصویر پت ها
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetPictureController : ControllerBase
    {
        private readonly IUserPetPictureService UserPetPictureService;
        private readonly ICurrentUserHelper currentUserHelper;

        public UserPetPictureController(IUserPetPictureService UserPetPictureService, ICurrentUserHelper currentUserHelper)
        {
            this.UserPetPictureService = UserPetPictureService;
            this.currentUserHelper = currentUserHelper; 
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var UserPetPicture = await UserPetPictureService.FindAsyncVDto(id, currentUserHelper.CurrentUser.UserId);
            return Ok(UserPetPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserPetPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] UserPetPictureInputDto dto)
        {
            dto.UserId = currentUserHelper.CurrentUser.UserId;
            var UserPetPicture = UserPetPictureService.Search(dto);
            return Ok(UserPetPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserPetPictureDto>), 200)]
        public async Task<IActionResult> UserPet(UserPetPictureDto UserPetPictureDto)
        {
            var owned = await UserPetPictureService.IsUserPetOwnedByAsync(UserPetPictureDto.UserPetId, currentUserHelper.CurrentUser.UserId);
            if (!owned)
                return Ok(new BaseResultDto<UserPetPictureDto>(false, Resource.Notification.AccessDenied, default!));
            var result = await UserPetPictureService.InsertAsyncDto(UserPetPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<UserPetPictureDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await UserPetPictureService.FindAsyncVDto(id, currentUserHelper.CurrentUser.UserId);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var result = UserPetPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
