using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرات نمایندگان
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionCommentController : ControllerBase
    {
        private ICompanionCommentService _CompanionCommentService;
        public CompanionCommentController(ICompanionCommentService CompanionCommentService)
        {
            this._CompanionCommentService = CompanionCommentService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _CompanionCommentService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CompanionCommentDto>), 200)]
        public IActionResult Get([FromQuery] CompanionCommentInputDto dto)
        {
            var searchDto = _CompanionCommentService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionCommentDto CompanionCommentDto)
        {
            var dto = _CompanionCommentService.UpdateDto(CompanionCommentDto);
            return Ok(dto);
        }

    }
}
