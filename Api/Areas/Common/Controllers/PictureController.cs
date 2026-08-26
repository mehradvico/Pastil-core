using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیا
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PictureController : ControllerBase
    {
        private readonly IPictureService _pictureService;
        /// <summary>
        /// مدیا
        /// </summary>
        ///        
        public PictureController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }
        /// <summary>
        ///  دریافت 
        /// </summary>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await _pictureService.FindVDtoAsync(id);
            return Ok(item);
        }
    }
}
