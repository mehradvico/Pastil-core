using Application.Common.Interface;
using Application.Services.Content.ContactUsSrv.Dto;
using Application.Services.Content.ContactUsSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با ارتباط با ما
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("ContactUs")]
    public class ContactUsController : ControllerBase
    {
        private IContactUsService ContactUsService;
        private CurrentUserDto _currentUser;
        /// <summary>
        /// مرتبط با ارتباط با ما
        /// </summary>
        public ContactUsController(IContactUsService ContactUsService, ICurrentUserHelper currentUserHelper)
        {
            this.ContactUsService = ContactUsService;
            this._currentUser = currentUserHelper.CurrentUser;
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Application.Common.Dto.Result.BaseResultDto), 200)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> Post(ContactUsDto dto)
        {
            dto.UserId = _currentUser?.UserId;
            var contactUs = await ContactUsService.InsertAsyncDto(dto);
            return Ok(contactUs);
        }
    }
}
