using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گالری کلینیک (محیط و تجهیزات)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicPictureController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        public ClinicPictureController(IClinicProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.GetPicturesAsync(clinicId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicPictureVDto>), 200)]
        public async Task<IActionResult> Post(ClinicPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, ownerUserId: null));
    }
}
