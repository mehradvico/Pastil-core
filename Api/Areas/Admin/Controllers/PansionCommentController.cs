using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرات پانسیون ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionCommentController : ControllerBase
    {
        private readonly IPansionCommentService PansionCommentService;
        public PansionCommentController(IPansionCommentService PansionCommentService)
        {
            this.PansionCommentService = PansionCommentService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var PansionComment = await PansionCommentService.FindAsyncDto(id);
            return Ok(PansionComment);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PansionCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionCommentInputDto dto)
        {
            var PansionComment = PansionCommentService.Search(dto);
            return Ok(PansionComment);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PansionCommentDto>), 200)]
        public async Task<IActionResult> Put(PansionCommentDto PansionCommentDto)
        {
            var result = await PansionCommentService.UpdateDtoAsync(PansionCommentDto);
            return Ok(result);
        }
    }
}
