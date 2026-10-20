using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی آرایشگاه‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerServiceController : ControllerBase
    {
        private readonly IGroomerCatalogService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerServiceController(IGroomerCatalogService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.ListManageAsync(groomerId, UserId));

        /// <summary>افزودن خدمت (تا تأیید ادمین قابل رزرو نیست)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerServiceVDto>), 200)]
        public async Task<IActionResult> Post(GroomerServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, UserId));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(GroomerServiceActiveRequestDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, UserId));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(GroomerServiceUsersRequestDto dto) => Ok(await _service.SetServiceUsersAsync(dto.GroomerServiceId, dto.UserIds, UserId));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(GroomerServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, UserId));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(GroomerServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, UserId));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(GroomerServiceActiveRequestDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, UserId));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, UserId));
    }

    public class GroomerServiceActiveRequestDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class GroomerServiceUsersRequestDto
    {
        public long GroomerServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
