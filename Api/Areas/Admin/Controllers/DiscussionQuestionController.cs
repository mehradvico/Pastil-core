using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت موضوع های تالار گفت و گو
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionQuestionController : ControllerBase
    {
        private IDiscussionQuestionService _DiscussionQuestionService;
        /// <summary>
        /// مدیریت موضوع گفت و گو
        /// </summary>
        ///
        public DiscussionQuestionController(IDiscussionQuestionService DiscussionQuestionService)
        {
            _DiscussionQuestionService = DiscussionQuestionService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionQuestionVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {

            var dto = await _DiscussionQuestionService.FindAdminAsyncVDto(id);
            return Ok(dto);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscussionQuestionInputDto), 200)]
        public IActionResult Get([FromQuery] DiscussionQuestionInputDto dto)
        {
            var searchDto = _DiscussionQuestionService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionQuestionDto>), 200)]
        public async Task<IActionResult> Post(DiscussionQuestionDto DiscussionQuestionDto)
        {
            var dto = await _DiscussionQuestionService.InsertAsyncDto(DiscussionQuestionDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(DiscussionQuestionDto), 200)]
        public IActionResult Put(DiscussionQuestionDto DiscussionQuestionDto)
        {
            var dto = _DiscussionQuestionService.UpdateDto(DiscussionQuestionDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(DiscussionQuestionDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _DiscussionQuestionService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
