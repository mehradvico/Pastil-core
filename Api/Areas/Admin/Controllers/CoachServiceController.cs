using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی مربی
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachServiceController : ControllerBase
    {
        private readonly ICoachCatalogService _service;
        public CoachServiceController(ICoachCatalogService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.ListManageAsync(coachId, ownerUserId: null));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachServiceVDto>), 200)]
        public async Task<IActionResult> Post(CoachServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, ownerUserId: null));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(CoachServiceActiveDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        /// <summary>تأیید/رد خدمت و تعیین درصد کمیسیون</summary>
        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(CoachServiceApproveDto dto) => Ok(await _service.ApproveServiceAsync(dto));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(CoachServiceUsersDto dto) => Ok(await _service.SetServiceUsersAsync(dto.CoachServiceId, dto.UserIds, ownerUserId: null));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<CoachServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(CoachServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, ownerUserId: null));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(CoachServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, ownerUserId: null));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(CoachServiceActiveDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, ownerUserId: null));
    }

    public class CoachServiceActiveDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class CoachServiceUsersDto
    {
        public long CoachServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
