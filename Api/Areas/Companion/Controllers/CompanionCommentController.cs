using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت نظرات نمایندگان
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionCommentController : ControllerBase
    {
        private ICompanionCommentService _CompanionCommentService;
        private ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت نظرات نمایندگان
        /// </summary>
        ///
        public CompanionCommentController(ICompanionCommentService CompanionCommentService, ICurrentUserHelper currentUser)
        {
            this._CompanionCommentService = CompanionCommentService;
            this._currentUser = currentUser;
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
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var searchDto = _CompanionCommentService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ویرایش نظرات
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionCommentDto CompanionCommentDto)
        {
            CompanionCommentDto.CompanionId = _currentUser.CurrentUser.CompanionId!.Value;
            var dto = _CompanionCommentService.UpdateDto(CompanionCommentDto);
            return Ok(dto);
        }
    }

}

