using Application.Common.Dto.Result;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Content.StoryGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// استوری ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoryGroupController : ControllerBase
    {
        private IStoryGroupService StoryGroupService;
        public StoryGroupController(IStoryGroupService StoryGroupService)
        {
            this.StoryGroupService = StoryGroupService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoryGroupVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await StoryGroupService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StoryGroupDto>), 200)]
        public IActionResult Get([FromQuery] StoryGroupInputDto dto)
        {
            var searchDto = StoryGroupService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StoryGroupDto>), 200)]
        public async Task<IActionResult> Post(StoryGroupDto dto)
        {

            var model = await StoryGroupService.InsertAsyncDto(dto);
            return Ok(model);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(StoryGroupDto StoryGroupDto)
        {
            var dto = StoryGroupService.UpdateDto(StoryGroupDto);
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
            var dto = StoryGroupService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
