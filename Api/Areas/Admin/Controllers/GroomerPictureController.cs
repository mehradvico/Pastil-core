using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گالری آرایشگاه (محیط و تجهیزات؛ نمونه‌کار در GroomerPortfolio است)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPictureController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        public GroomerPictureController(IGroomerProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.GetPicturesAsync(groomerId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPictureVDto>), 200)]
        public async Task<IActionResult> Post(GroomerPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, ownerUserId: null));
    }
}
