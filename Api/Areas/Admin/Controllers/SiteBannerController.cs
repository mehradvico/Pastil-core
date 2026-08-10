using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت بنرهای سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteBannerController : ControllerBase
    {
        private readonly IBannerService _service;
        public SiteBannerController(IBannerService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] BannerInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(BannerDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(BannerDto dto) => Ok(_service.UpdateDto(dto));

        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
