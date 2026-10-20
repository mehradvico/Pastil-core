using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی کلینیک‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicServiceController : ControllerBase
    {
        private readonly IClinicCatalogService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicServiceController(IClinicCatalogService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.ListManageAsync(clinicId, UserId));

        /// <summary>افزودن خدمت (تا تأیید ادمین قابل رزرو نیست)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicServiceVDto>), 200)]
        public async Task<IActionResult> Post(ClinicServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, UserId));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(ClinicServiceActiveRequestDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, UserId));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(ClinicServiceUsersRequestDto dto) => Ok(await _service.SetServiceUsersAsync(dto.ClinicServiceId, dto.UserIds, UserId));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(ClinicServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, UserId));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(ClinicServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, UserId));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(ClinicServiceActiveRequestDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, UserId));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, UserId));
    }

    public class ClinicServiceActiveRequestDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class ClinicServiceUsersRequestDto
    {
        public long ClinicServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
