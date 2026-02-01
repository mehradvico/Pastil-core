using Application.Common.Dto.Result;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت آیتم های استوری
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoryItemController : ControllerBase
    {
        private IStoryItemService StoryItemService;
        public StoryItemController(IStoryItemService StoryItemService)
        {
            this.StoryItemService = StoryItemService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoryItemVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await StoryItemService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StoryItemDto>), 200)]
        public IActionResult Get([FromQuery] StoryItemInputDto dto)
        {
            var searchDto = StoryItemService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StoryItemDto>), 200)]
        public async Task<IActionResult> Post(StoryItemDto dto)
        {

            var model = await StoryItemService.InsertAsyncDto(dto);
            return Ok(model);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(StoryItemDto StoryItemDto)
        {
            var dto = StoryItemService.UpdateDto(StoryItemDto);
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
            var dto = StoryItemService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
