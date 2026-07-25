using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت کاربران نماینده
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUserController : ControllerBase
    {
        private readonly ICompanionUserService CompanionUserService;
        private readonly CurrentUserDto CurrentUserDto;
        /// <summary>
        /// مدیریت کاربران نماینده
        /// </summary>

        public CompanionUserController(ICompanionUserService CompanionUserService, ICurrentUserHelper currentUserHelper)
        {
            this.CompanionUserService = CompanionUserService;
            CurrentUserDto = currentUserHelper.CurrentUser;
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
        [HttpGet]
        [ProducesResponseType(typeof(CompanionUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionUserInputDto dto)
        {
            dto.CompanionId = CurrentUserDto.CompanionId;
            var CompanionUser = CompanionUserService.SearchDto(dto);
            return Ok(CompanionUser);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public async Task<IActionResult> Post(CompanionUserDto CompanionUserDto)
        {
            if (!CurrentUserDto.CompanionId.HasValue)
                return Forbid();
            CompanionUserDto.Active = false;
            CompanionUserDto.UserAccept = null;
            CompanionUserDto.CompanionId = CurrentUserDto.CompanionId.Value;
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
            if (!CurrentUserDto.CompanionId.HasValue)
                return Forbid();
            CompanionUserDto.CompanionId = CurrentUserDto.CompanionId.Value;
            var result = CompanionUserService.Active(CompanionUserDto);
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
