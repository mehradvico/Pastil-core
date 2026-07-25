using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Content.NewsletterSrv.Dto;
using Application.Services.Content.NewsletterSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خبرنامه
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsletterController : ControllerBase
    {
        private readonly INewsletterService NewsletterService;
        public NewsletterController(INewsletterService NewsletterService)
        {
            this.NewsletterService = NewsletterService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var getAll = await NewsletterService.FindAsyncDto(id);
            return Ok(getAll);
        }
        ///<summary>
        ///جستجو
        /// </summary>
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = NewsletterService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public async Task<IActionResult> Post(NewsletterDto NewsletterDto)
        {
            var insertDto = await NewsletterService.InsertAsyncDto(NewsletterDto);
            return Ok(insertDto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public IActionResult Put(NewsletterDto NewsletterDto)
        {
            var updateDto = NewsletterService.UpdateDto(NewsletterDto);
            return Ok(updateDto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<NewsletterDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = NewsletterService.DeleteDto(id);
            return Ok(deleteDto);
        }



    }
}
