using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کامنت های رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveCommentController : ControllerBase
    {
        private readonly ICompanionReserveCommentService CompanionReserveCommentService;
        public CompanionReserveCommentController(ICompanionReserveCommentService CompanionReserveCommentService)
        {
            this.CompanionReserveCommentService = CompanionReserveCommentService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveCommentVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionReserveComment = await CompanionReserveCommentService.FindAsyncVDto(id);
            return Ok(CompanionReserveComment);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CompanionReserveCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReserveCommentInputDto dto)
        {
            var CompanionReserveComment = CompanionReserveCommentService.Search(dto);
            return Ok(CompanionReserveComment);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveCommentDto>), 200)]
        public async Task<IActionResult> Put(CompanionReserveCommentDto CompanionReserveCommentDto)
        {
            var result = await CompanionReserveCommentService.UpdateDtoAsync(CompanionReserveCommentDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveCommentDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionReserveCommentService.DeleteDto(id);
            return Ok(result);
        }
    }
}