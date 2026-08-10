using Application.Services.Content.GallerySrv.Dto;
using Application.Services.Content.GallerySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت گالری‌های سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteGalleryController : ControllerBase
    {
        private readonly IGalleryService _service;
        public SiteGalleryController(IGalleryService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] GalleryInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(GalleryDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(GalleryDto dto) => Ok(_service.UpdateDto(dto));

        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
