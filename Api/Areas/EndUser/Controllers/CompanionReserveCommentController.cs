using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کامنت های رزرو نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveCommentController : ControllerBase
    {
        private ICompanionReserveCommentService CompanionReserveCommentService;
        /// <summary>
        /// مدیریت کامنت های رزرو نمایندگان
        /// </summary>
        public CompanionReserveCommentController(ICompanionReserveCommentService CompanionReserveCommentService)
        {
            this.CompanionReserveCommentService = CompanionReserveCommentService;
        }
        /// <summary>
        /// جستجو
        /// </summary>

        /// 
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CompanionReserveCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReserveCommentInputDto dto)
        {
            dto.Available = true;
            dto.AllStatus = false;
            var post = CompanionReserveCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveCommentDto>), 200)]
        public async Task<IActionResult> Post(CompanionReserveCommentDto CompanionReserveComment)
        {

            var dto = await CompanionReserveCommentService.InsertAsyncDto(CompanionReserveComment);
            return Ok(dto);
        }

    }
}
