using Application.Common.Dto.Result;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Accounting.UserPetPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصاویر پت های کاربر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetPictureController : ControllerBase
    {
        private readonly IUserPetPictureService UserPetPictureService;

        public UserPetPictureController(IUserPetPictureService UserPetPictureService)
        {
            this.UserPetPictureService = UserPetPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var UserPetPicture = await UserPetPictureService.FindAsyncVDto(id);
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
            var UserPetPicture = UserPetPictureService.Search(dto);
            return Ok(UserPetPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserPetPictureDto>), 200)]
        public async Task<IActionResult> Post(UserPetPictureDto UserPetPictureDto)
        {
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
        public IActionResult Delete(long id)
        {
            var result = UserPetPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
