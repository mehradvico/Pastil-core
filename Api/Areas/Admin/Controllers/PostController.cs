using Application.Common.Dto.Result;
using Application.Services.Content.PostSrv.Dto;
using Application.Services.Content.PostSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پست ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private IPostService postService;
        public PostController(IPostService postService)
        {
            this.postService = postService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PostDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var post = await postService.FindAsyncDto(id);
            return Ok(post);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(PostSearchDto), 200)]
        public IActionResult Get([FromQuery] PostInputDto dto)
        {
            var post = postService.Search(dto);
            return Ok(post);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// 

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PostDto>), 200)]
        public async Task<IActionResult> Post(PostDto postDto)
        {
            var item = await postService.InsertAsyncDto(postDto);
            return Ok(item);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PostDto>), 200)]
        public IActionResult Put(PostDto postDto)
        {
            var item = postService.UpdateDto(postDto);
            return Ok(item);
        }
        /// <summary>
        /// تایید آیتم
        /// </summary>
        [HttpPut("confirm")]
        [ActionName("confirm")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PostConfirmDto postDto)
        {
            var item = postService.Confirm(postDto);
            return Ok(item);
        }
        /// <summary>
        /// تغییر کاربر
        /// </summary>
        [HttpPut("changeuser")]
        [ActionName("changeuser")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ChangePostUserDto postDto)
        {
            var item = postService.ChangeUser(postDto);
            return Ok(item);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PostDto>), 200)]
        public IActionResult Delete(long id)
        {
            var item = postService.DeleteDto(id);
            return Ok(item);
        }
    }
}
