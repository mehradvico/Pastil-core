using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نمایش نمایندگان در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteCompanionController : ControllerBase
    {
        private readonly ICompanionService _service;
        public SiteCompanionController(ICompanionService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] CompanionInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(CompanionDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public async Task<IActionResult> Put(CompanionDto dto) => Ok(await _service.UpdateAsyncDto(dto));

        [HttpPatch("{id:long}/visibility")]
        public async Task<IActionResult> PatchVisibility(long id, SiteVisibilityDto dto)
        {
            var current = await _service.FindAsyncDto(id);
            if (!current.IsSuccess || current.Data == null)
                return Ok(current);

            current.Data.ShowToSite = dto.ShowToSite;
            return Ok(await _service.UpdateAsyncDto(current.Data));
        }

        [HttpDelete]
        public IActionResult Delete(long id) => Ok(_service.DeleteDto(id));
    }
}
