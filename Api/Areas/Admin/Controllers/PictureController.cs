using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیا
    /// </summary>
    ///
    [Area("Admin")]
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
        ///  جستوجو 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseSearchDto<PictureVDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto baseInputDto)
        {
            var result = _pictureService.Search(baseInputDto);
            return Ok(result);
        }
    }
}
