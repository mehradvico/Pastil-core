using Application.Services.Content.PostSrv.Dto;
using Application.Services.Content.PostSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// نوشته‌های منتشرشده سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Post")]
    [ApiController]
    [AllowAnonymous]
    public class SitePostController : ControllerBase
    {
        private readonly IPostService _postService;

        public SitePostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] PostInputDto dto)
        {
            dto.Available = true;
            return Ok(_postService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _postService.FindAsyncVDto(id, visit: true));
        }
    }
}
