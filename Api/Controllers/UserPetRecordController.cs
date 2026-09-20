using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت تاریخچه پت ها
    /// </summary>
    ///
    // قبلاً بدون [Authorize] و بدون بررسی مالکیت بود: سوابق پزشکی هر پت (به‌همراه مشخصات پت و موبایل/ایمیل صاحب و اپراتور)
    // برای هر ناشناسی قابل خواندن بود. حالا: فقط صاحب پت، اپراتوری که رکورد را ثبت کرده، و ادمین.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetRecordController : ControllerBase
    {
        private readonly IUserPetRecordService _petService;
        private readonly ICurrentUserHelper _currentUser;

        public UserPetRecordController(IUserPetRecordService petService, ICurrentUserHelper currentUser)
        {
            _petService = petService;
            _currentUser = currentUser;
        }

        private bool IsAdmin => _currentUser.CurrentUser?.RoleId == (long)RoleEnum.Admin;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetRecordDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _petService.FindAsyncDto(id);
            if (result.IsSuccess && !IsAdmin)
            {
                var me = _currentUser.CurrentUser.UserId;
                if (result.Data?.UserPet?.UserId != me && result.Data?.OperatorId != me)
                    return Ok(new BaseResultDto<UserPetRecordDto>(false, Resource.Notification.AccessDenied, default!));
            }
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserPetRecordInputDto), 200)]
        public IActionResult Get([FromQuery] UserPetRecordInputDto dto)
        {
            if (!IsAdmin)
            {
                var me = _currentUser.CurrentUser.UserId;
                // اپراتور می‌تواند رکوردهای ثبت‌کرده‌ی خودش را ببیند؛ در غیر این‌صورت فقط سوابق پت‌های خودش
                if (dto.OperatorId != me)
                    dto.UserId = me;
            }
            return Ok(_petService.Search(dto));
        }
    }
}
