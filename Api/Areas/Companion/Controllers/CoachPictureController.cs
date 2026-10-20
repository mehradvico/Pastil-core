using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// گالری مربی‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachPictureController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachPictureController(ICoachProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachPictureVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.GetPicturesAsync(coachId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachPictureVDto>), 200)]
        public async Task<IActionResult> Post(CoachPictureAddDto dto) => Ok(await _service.AddPictureAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemovePictureAsync(id, UserId));
    }
}
