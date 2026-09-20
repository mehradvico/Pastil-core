using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Iface;
using Application.Services.Accounting.UserPetSrv;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Accounting.UserPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پت های کاربر
    /// </summary>
    ///
    // قبلاً بدون [Authorize] و بدون هیچ بررسی مالکیتی بود: هر ناشناسی با id ترتیبی یا فیلتر UserId می‌توانست همه‌ی پت‌ها را
    // همراه موبایل/ایمیل صاحب، آدرس، بیماری/دارو، میکروچیپ و سوابق پزشکی بخواند. حالا: لاگین لازم است؛ صاحب پت و ادمین کامل
    // می‌بینند، دیگران فقط نمای نمایشی (UserPetPrivacy) و جستجو برای غیرادمین فقط پت‌های خودش است.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserPetController : ControllerBase
    {
        private readonly IUserPetService _petService;
        private readonly ICurrentUserHelper _currentUser;

        public UserPetController(IUserPetService petService, ICurrentUserHelper currentUser)
        {
            _petService = petService;
            _currentUser = currentUser;
        }

        private bool IsAdmin => _currentUser.CurrentUser?.RoleId == (long)RoleEnum.Admin;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _petService.FindAsyncVDto(id);
            if (result.IsSuccess && result.Data != null && result.Data.UserId != _currentUser.CurrentUser.UserId && !IsAdmin)
                UserPetPrivacy.ToPublicView(result.Data);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserPetInputDto), 200)]
        public IActionResult Get([FromQuery] UserPetInputDto dto)
        {
            if (!IsAdmin)
            {
                dto.UserId = _currentUser.CurrentUser.UserId;
                dto.Available = true;
            }
            return Ok(_petService.Search(dto));
        }
    }
}
