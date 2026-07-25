using Application.Common.Dto.Result;
using Application.Services.Content.PostFileSrv.Dto;
using Application.Services.Content.PostFileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فایل های پست 
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostFileController : ControllerBase
    {
        private readonly IPostFileService postFileService;
        public PostFileController(IPostFileService postFileService)
        {
            this.postFileService = postFileService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PostFileDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var postFile = await postFileService.FindAsyncDto(id);
            return Ok(postFile);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PostFileSearchDto), 200)]
        public IActionResult Get([FromQuery] PostFileInputDto dto)
        {
            var postFile = postFileService.Search(dto);
            return Ok(postFile);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PostFileDto>), 200)]
        public async Task<IActionResult> Post(PostFileDto postFileDto)
        {
            var result = await postFileService.InsertAsyncDto(postFileDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PostFileDto>), 200)]
        public IActionResult Put(PostFileDto postFileDto)
        {
            var result = postFileService.UpdateDto(postFileDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PostFileDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = postFileService.DeleteDto(id);
            return Ok(result);
        }
    }
}
