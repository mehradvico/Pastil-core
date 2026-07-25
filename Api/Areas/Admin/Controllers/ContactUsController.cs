using Application.Common.Dto.Result;
using Application.Services.Content.ContactUsSrv.Dto;
using Application.Services.Content.ContactUsSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ارتباط با ما
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactUsController : ControllerBase
    {
        private IContactUsService ContactUsService;
        public ContactUsController(IContactUsService ContactUsService)
        {
            this.ContactUsService = ContactUsService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsVDto>), 200)]
        public IActionResult Get([FromQuery] ContactUsInputDto dto)
        {
            var searchDto = ContactUsService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await ContactUsService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ContactUsDto contactUs)
        {
            var dto = ContactUsService.Update(contactUs);
            return Ok(dto);
        }
    }
}
