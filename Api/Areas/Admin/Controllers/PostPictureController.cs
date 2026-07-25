using Application.Common.Dto.Result;
using Application.Services.Content.PostPictureSrv.Dto;
using Application.Services.Content.PostPictureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تصاویر پست
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostPictureController : ControllerBase
    {
        private readonly IPostPictureService postPictureService;
        public PostPictureController(IPostPictureService postPictureService)
        {
            this.postPictureService = postPictureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PostPictureDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var postPicture = await postPictureService.FindAsyncDto(id);
            return Ok(postPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PostPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] PostPictureInputDto dto)
        {
            var postPicture = postPictureService.Search(dto);
            return Ok(postPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PostPictureDto>), 200)]
        public async Task<IActionResult> Post(PostPictureDto postPictureDto)
        {
            var result = await postPictureService.InsertAsyncDto(postPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PostPictureDto>), 200)]
        public IActionResult Put(PostPictureDto postPictureDto)
        {
            var result = postPictureService.UpdateDto(postPictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PostPictureDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = postPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
