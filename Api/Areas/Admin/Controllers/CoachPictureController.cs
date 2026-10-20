using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گالری مربی (محیط و تجهیزات)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachPictureController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        public CoachPictureController(ICoachProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.GetPicturesAsync(coachId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachPictureVDto>), 200)]
        public async Task<IActionResult> Post(CoachPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, ownerUserId: null));
    }
}
