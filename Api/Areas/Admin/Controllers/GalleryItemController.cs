using Application.Common.Dto.Result;
using Application.Services.Content.GalleryItemSrv.Dto;
using Application.Services.Content.GalleryItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آیتم های گالری
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GalleryItemController : ControllerBase
    {
        private IGalleryItemService galleryItemService;
        public GalleryItemController(IGalleryItemService galleryItemService)
        {
            this.galleryItemService = galleryItemService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<GalleryItemDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await galleryItemService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<GalleryItemDto>), 200)]
        public IActionResult Get([FromQuery] GalleryItemInputDto dto)
        {
            var searchDto = galleryItemService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GalleryItemDto>), 200)]
        public async Task<IActionResult> Post(GalleryItemDto galleryItemDto)
        {

            var dto = await galleryItemService.InsertAsyncDto(galleryItemDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(GalleryItemDto galleryItemDto)
        {
            var dto = galleryItemService.UpdateDto(galleryItemDto);
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
            var dto = galleryItemService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
