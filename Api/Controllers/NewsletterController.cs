using Application.Common.Dto.Result;
using Application.Services.Content.NewsletterSrv.Dto;
using Application.Services.Content.NewsletterSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت خبرنامه
    /// </summary>
    ///
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class NewsletterController : ControllerBase
    {
        private readonly INewsletterService NewsletterService;
        /// <summary>
        /// مدیریت خبرنامه
        /// </summary>
        ///  
        public NewsletterController(INewsletterService NewsletterService)
        {
            this.NewsletterService = NewsletterService;
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("ContactUs")]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public async Task<IActionResult> Post(NewsletterDto NewsletterDto)
        {
            var insertDto = await NewsletterService.InsertAsyncDto(NewsletterDto);
            return Ok(insertDto);
        }

    }
}
