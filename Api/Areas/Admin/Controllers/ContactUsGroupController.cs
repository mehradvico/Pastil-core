using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Content.ContactUsGroupSrv.Dto;
using Application.Services.Content.ContactUsGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گروه ارتباط با ما
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactUsGroupController : ControllerBase
    {
        private readonly IContactUsGroupService ContactUsGroupService;
        public ContactUsGroupController(IContactUsGroupService ContactUsGroupService)
        {
            this.ContactUsGroupService = ContactUsGroupService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var getAll = await ContactUsGroupService.FindAsyncDto(id);
            return Ok(getAll);
        }
        ///<summary>
        ///جستجو
        /// </summary>
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = ContactUsGroupService.Search(dto);
            return Ok(searchDto);
        }
        ///<summary>
        ///بر اساس نقش
        /// </summary>
        ///<returns></returns>
        [HttpGet("getbyrole")]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public IActionResult Get()
        {
            var searchDto = ContactUsGroupService.GetForRole();
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public async Task<IActionResult> Post(ContactUsGroupDto ContactUsGroupDto)
        {
            var insertDto = await ContactUsGroupService.InsertAsyncDto(ContactUsGroupDto);
            return Ok(insertDto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public IActionResult Put(ContactUsGroupDto ContactUsGroupDto)
        {
            var updateDto = ContactUsGroupService.UpdateDto(ContactUsGroupDto);
            return Ok(updateDto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<ContactUsGroupDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = ContactUsGroupService.DeleteDto(id);
            return Ok(deleteDto);
        }



    }
}
