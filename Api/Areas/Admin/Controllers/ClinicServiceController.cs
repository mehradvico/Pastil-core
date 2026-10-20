using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی کلینیک
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicServiceController : ControllerBase
    {
        private readonly IClinicCatalogService _service;
        public ClinicServiceController(IClinicCatalogService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.ListManageAsync(clinicId, ownerUserId: null));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicServiceVDto>), 200)]
        public async Task<IActionResult> Post(ClinicServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, ownerUserId: null));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(ClinicServiceActiveDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        /// <summary>تأیید/رد خدمت و تعیین درصد کمیسیون</summary>
        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(ClinicServiceApproveDto dto) => Ok(await _service.ApproveServiceAsync(dto));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(ClinicServiceUsersDto dto) => Ok(await _service.SetServiceUsersAsync(dto.ClinicServiceId, dto.UserIds, ownerUserId: null));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(ClinicServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, ownerUserId: null));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(ClinicServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, ownerUserId: null));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(ClinicServiceActiveDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, ownerUserId: null));
    }

    public class ClinicServiceActiveDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class ClinicServiceUsersDto
    {
        public long ClinicServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
