using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// گالری کلینیک‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicPictureController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicPictureController(IClinicProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.GetPicturesAsync(clinicId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicPictureVDto>), 200)]
        public async Task<IActionResult> Post(ClinicPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, UserId));
    }
}
