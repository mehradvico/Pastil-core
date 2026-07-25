using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت کامنت های رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveCommentController : ControllerBase
    {
        private readonly ICompanionReserveCommentService CompanionReserveCommentService;
        /// <summary>
        /// مدیریت کامنت های رزرو نمایندگان
        /// </summary>

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
    }
}
