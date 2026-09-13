using Application.Common.Dto.Input;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت نمایش مدارس در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteSchoolController : ControllerBase
    {
        private readonly ISchoolService _service;
        public SiteSchoolController(ISchoolService service) => _service = service;

        [HttpGet]
        public IActionResult Get([FromQuery] SchoolInputDto dto) => Ok(_service.Search(dto));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id));

        [HttpPost]
        public async Task<IActionResult> Post(SchoolDto dto) => Ok(await _service.InsertAsyncDto(dto));

        [HttpPut]
        public IActionResult Put(SchoolDto dto) => Ok(_service.UpdateDto(dto));

        [HttpPatch("{id:long}/visibility")]
        public async Task<IActionResult> Put(long id, SiteVisibilityDto dto) =>
            Ok(await _service.UpdateSiteVisibilityAsync(id, dto.ShowToSite));
    }
}
