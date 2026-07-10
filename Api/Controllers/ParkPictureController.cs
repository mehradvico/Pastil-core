using Application.Common.Dto.Result;
using Application.Services.LocationFields.ParkPictureSrv.Dto;
using Application.Services.LocationFields.ParkPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت تصویر پارک ها
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class ParkPictureController : ControllerBase
    {
        private readonly IParkPictureService ParkPictureService;

        public ParkPictureController(IParkPictureService ParkPictureService)
        {
            this.ParkPictureService = ParkPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ParkPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var ParkPicture = await ParkPictureService.FindAsyncVDto(id);
            return Ok(ParkPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ParkPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] ParkPictureInputDto dto)
        {
            var ParkPicture = ParkPictureService.Search(dto);
            return Ok(ParkPicture);
        }
    }
}
