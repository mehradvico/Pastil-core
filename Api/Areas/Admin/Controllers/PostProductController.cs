using Application.Common.Dto.Result;
using Application.Services.Content.PostProductSrv.Dto;
using Application.Services.Content.PostProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// محصولات مرتبط با مقاله
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostProductController : ControllerBase
    {
        private readonly IPostProductService PostProductService;
        public PostProductController(IPostProductService PostProductService)
        {
            this.PostProductService = PostProductService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PostProductDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await PostProductService.GetPostProductsAsync(id);
            return Ok(item);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Post(PostProductDto PostProductDto)
        {
            var insertDto = PostProductService.InsertOrUpdate(PostProductDto);
            return Ok(insertDto);
        }




    }
}
