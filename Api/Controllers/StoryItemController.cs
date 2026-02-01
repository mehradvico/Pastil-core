using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با استوری ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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
            var role = await StoryItemService.FindAsyncVDto(id, true);
            return Ok(role);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(StoryItemSearchDto), 200)]
        public IActionResult Get([FromQuery] StoryItemInputDto dto)
        {
            dto.Available = true;
            var post = StoryItemService.Search(dto);
            return Ok(post);
        }
    }
}
