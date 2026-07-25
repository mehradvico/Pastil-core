using Application.Common.Dto.Result;
using Application.Services.Content.PostCommentSrv.Dto;
using Application.Services.Content.PostCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  نظرات پست ها 
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostCommentController : ControllerBase
    {
        private IPostCommentService PostCommentService;
        public PostCommentController(IPostCommentService PostCommentService)
        {
            this.PostCommentService = PostCommentService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PostCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await PostCommentService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PostCommentDto>), 200)]
        public IActionResult Get([FromQuery] PostCommentInputDto dto)
        {
            var searchDto = PostCommentService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PostCommentDto PostCommentDto)
        {
            var dto = PostCommentService.UpdateDto(PostCommentDto);
            return Ok(dto);
        }

    }
}
