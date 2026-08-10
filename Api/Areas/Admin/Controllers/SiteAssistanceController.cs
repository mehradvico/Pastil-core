using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نمایش خدمات در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteAssistanceController : ControllerBase
    {
        private readonly IAssistanceService _service;
        public SiteAssistanceController(IAssistanceService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] AssistanceInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(AssistanceDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(AssistanceDto dto) => Ok(_service.UpdateDto(dto));

        [HttpPatch("{id:long}/visibility")]
        public async Task<IActionResult> PatchVisibility(long id, SiteVisibilityDto dto)
        {
            var current = await _service.FindAsyncDto(id);
            if (!current.IsSuccess || current.Data == null)
                return Ok(current);

            current.Data.ShowToSite = dto.ShowToSite;
            return Ok(_service.UpdateDto(current.Data));
        }

        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
