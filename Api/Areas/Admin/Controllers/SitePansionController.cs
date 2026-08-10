using Application.Common.Dto.Input;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نمایش پانسیون‌ها در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SitePansionController : ControllerBase
    {
        private readonly IPansionService _service;
        public SitePansionController(IPansionService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] PansionInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(PansionDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(PansionDto dto) => Ok(_service.UpdateDto(dto));

        [HttpPatch("{id:long}/visibility")]
        public async Task<IActionResult> PatchVisibility(long id, SiteVisibilityDto dto)
        {
            var current = await _service.FindAsyncDto(id);
            if (!current.IsSuccess || current.Data == null)
                return Ok(current);

            current.Data.ShowToSite = dto.ShowToSite;
            return Ok(_service.UpdateDto(current.Data));
        }
    }
}
