using Application.Common.Dto.Result;
using Application.Services.CategorySrv.Dto;
using Application.Services.CodeSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  کدها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeController : ControllerBase
    {
        private readonly ICodeService codeService;
        /// <summary>
        ///  کدها
        /// </summary>
        public CodeController(ICodeService codeService)
        {
            this.codeService = codeService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CodeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var getAll = await codeService.FindAsyncDto(id);
            return Ok(getAll);
        }

        /// <summary>
        /// جستجو
        /// </summary>
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CodeSearchDto), 200)]
        public IActionResult Get([FromQuery] CodeInputDto dto)
        {
            var searchDto = codeService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CodeDto>), 200)]
        public async Task<IActionResult> Post(CodeDto codeDto)
        {
            var insertDto = await codeService.InsertAsyncDto(codeDto);
            return Ok(insertDto);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CodeDto>), 200)]
        public IActionResult Put(CodeDto codeDto)
        {
            var updateDto = codeService.UpdateDto(codeDto);
            return Ok(updateDto);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CodeDto>), 200)]
        public IActionResult Delete(long id)
        {
            var deleteDto = codeService.DeleteDto(id);
            return Ok(deleteDto);
        }


    }
}
