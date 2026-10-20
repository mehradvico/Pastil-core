using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// گالری آرایشگاه‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPictureController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerPictureController(IGroomerProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.GetPicturesAsync(groomerId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPictureVDto>), 200)]
        public async Task<IActionResult> Post(GroomerPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, UserId));
    }
}
