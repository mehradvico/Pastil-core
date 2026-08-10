using Application.Services.Content.GallerySrv.Dto;
using Application.Services.Content.GallerySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// گالری‌های فعال سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Gallery")]
    [ApiController]
    [AllowAnonymous]
    public class SiteGalleryController : ControllerBase
    {
        private readonly IGalleryService _galleryService;

        public SiteGalleryController(IGalleryService galleryService)
        {
            _galleryService = galleryService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] GalleryInputDto dto)
        {
            dto.Available = true;
            return Ok(_galleryService.Search(dto));
        }

        [HttpGet("label/{label}")]
        public async Task<IActionResult> Get(string label)
        {
            return Ok(await _galleryService.FindVDtoAsync(label));
        }
    }
}
