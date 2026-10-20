using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی آرایشگاه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerServiceController : ControllerBase
    {
        private readonly IGroomerCatalogService _service;
        public GroomerServiceController(IGroomerCatalogService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.ListManageAsync(groomerId, ownerUserId: null));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerServiceVDto>), 200)]
        public async Task<IActionResult> Post(GroomerServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, ownerUserId: null));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(GroomerServiceActiveDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        /// <summary>تأیید/رد خدمت و تعیین درصد کمیسیون</summary>
        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(GroomerServiceApproveDto dto) => Ok(await _service.ApproveServiceAsync(dto));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(GroomerServiceUsersDto dto) => Ok(await _service.SetServiceUsersAsync(dto.GroomerServiceId, dto.UserIds, ownerUserId: null));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(GroomerServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, ownerUserId: null));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(GroomerServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, ownerUserId: null));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(GroomerServiceActiveDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, ownerUserId: null));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, ownerUserId: null));
    }

    public class GroomerServiceActiveDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class GroomerServiceUsersDto
    {
        public long GroomerServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
