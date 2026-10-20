using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachCatalogSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// خدمات و بسته‌های قیمتی مربی‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachServiceController : ControllerBase
    {
        private readonly ICoachCatalogService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachServiceController(ICoachCatalogService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachServiceVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.ListManageAsync(coachId, UserId));

        /// <summary>افزودن خدمت (تا تأیید ادمین قابل رزرو نیست)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachServiceVDto>), 200)]
        public async Task<IActionResult> Post(CoachServiceInputDto dto) => Ok(await _service.AddServiceAsync(dto, UserId));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachServiceInputDto dto) => Ok(await _service.UpdateServiceAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteServiceAsync(id, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(CoachServiceActiveRequestDto dto) => Ok(await _service.SetServiceActiveAsync(dto.Id, dto.Active, UserId));

        [HttpPut("Users")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Users(CoachServiceUsersRequestDto dto) => Ok(await _service.SetServiceUsersAsync(dto.CoachServiceId, dto.UserIds, UserId));

        [HttpPost("Package")]
        [ProducesResponseType(typeof(BaseResultDto<CoachServicePackageVDto>), 200)]
        public async Task<IActionResult> PostPackage(CoachServicePackageInputDto dto) => Ok(await _service.AddPackageAsync(dto, UserId));

        [HttpPut("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutPackage(CoachServicePackageInputDto dto) => Ok(await _service.UpdatePackageAsync(dto, UserId));

        [HttpPut("Package/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PackageActive(CoachServiceActiveRequestDto dto) => Ok(await _service.SetPackageActiveAsync(dto.Id, dto.Active, UserId));

        [HttpDelete("Package")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(long id) => Ok(await _service.DeletePackageAsync(id, UserId));
    }

    public class CoachServiceActiveRequestDto
    {
        public long Id { get; set; }
        public bool Active { get; set; }
    }

    public class CoachServiceUsersRequestDto
    {
        public long CoachServiceId { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
