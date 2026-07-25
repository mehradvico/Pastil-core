using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Setting.CodeGroupSrv.Dto;
using Application.Services.Setting.CodeGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  گروه های کد
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeGroupController : ControllerBase
    {
        private readonly ICodeGroupService codeGroupService;
        /// <summary>
        ///  گروه های کد
        /// </summary>
        ///  
        public CodeGroupController(ICodeGroupService codeGroupService)
        {
            this.codeGroupService = codeGroupService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CodeGroupDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var getAll = await codeGroupService.FindAsyncDto(id);
            return Ok(getAll);
        }
        ///<summary>
        ///جستجو
        /// </summary>
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CodeGroupDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = codeGroupService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CodeGroupDto>), 200)]
        public async Task<IActionResult> Post(CodeGroupDto codeGroupDto)
        {
            var insertDto = await codeGroupService.InsertAsyncDto(codeGroupDto);
            return Ok(insertDto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CodeGroupDto>), 200)]
        public IActionResult Put(CodeGroupDto codeGroupDto)
        {
            var updateDto = codeGroupService.UpdateDto(codeGroupDto);
            return Ok(updateDto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CodeGroupDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = codeGroupService.DeleteDto(id);
            return Ok(deleteDto);
        }



    }
}
