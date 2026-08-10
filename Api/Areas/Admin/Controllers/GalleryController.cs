using Application.Common.Dto.Result;
using Application.Services.Content.GallerySrv.Dto;
using Application.Services.Content.GallerySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گالری ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GalleryController : ControllerBase
    {
        private IGalleryService galleryService;
        public GalleryController(IGalleryService galleryService)
        {
            this.galleryService = galleryService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<GalleryDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await galleryService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<GalleryDto>), 200)]
        public IActionResult Get([FromQuery] GalleryInputDto dto)
        {
            var searchDto = galleryService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GalleryDto>), 200)]
        public async Task<IActionResult> Post(GalleryDto galleryDto)
        {

            var dto = await galleryService.InsertAsyncDto(galleryDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(GalleryDto galleryDto)
        {
            var dto = galleryService.UpdateDto(galleryDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = galleryService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
