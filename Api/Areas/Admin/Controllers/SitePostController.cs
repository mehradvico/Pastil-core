using Application.Services.Content.PostSrv.Dto;
using Application.Services.Content.PostSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نوشته‌های سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SitePostController : ControllerBase
    {
        private readonly IPostService _service;
        public SitePostController(IPostService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] PostInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncDto(id));

        [HttpGet("CheckLabel")]
        public async Task<IActionResult> CheckLabel([FromQuery] string label, [FromQuery] long? excludeId) => Ok(await _service.CheckLabelAvailableAsync(label, excludeId));

        [HttpPost]
        public async Task<IActionResult> Post(PostDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(PostDto dto) => Ok(_service.UpdateDto(dto));

        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
