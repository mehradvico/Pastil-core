using Application.Common.Dto.Result;
using Application.Services.PastilAISrv.Dto;
using Application.Services.PastilAISrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت PastilAI
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilAIController : ControllerBase
    {
        private readonly IPastilAiChatService _chatService;
        private readonly IPastilAiPlanService _planService;

        /// <summary>
        /// مدیریت PastilAI
        /// </summary>
        public PastilAIController(IPastilAiChatService chatService, IPastilAiPlanService planService)
        {
            _chatService = chatService;
            _planService = planService;
        }

        /// <summary>
        /// لیست پلن های PastilAI
        /// </summary>
        /// <returns></returns>
        [HttpGet("plans")]
        [ProducesResponseType(typeof(BaseResultDto<List<PastilAiPlanVDto>>), 200)]
        public async Task<IActionResult> GetPlans(CancellationToken cancellationToken) =>
            Ok(await _planService.GetPlansAsync(true, cancellationToken));

        /// <summary>
        /// ویرایش پلن PastilAI
        /// </summary>
        /// <returns></returns>
        [HttpPut("plans")]
        [ProducesResponseType(typeof(BaseResultDto<PastilAiPlanVDto>), 200)]
        public async Task<IActionResult> PutPlan(PastilAiPlanUpdateDto dto, CancellationToken cancellationToken) =>
            Ok(await _planService.UpdateAsync(dto, cancellationToken));

        /// <summary>
        /// جستجوی گفت و گوهای PastilAI
        /// </summary>
        /// <returns></returns>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(PastilAiConversationSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] PastilAiConversationInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _chatService.SearchAdminAsync(dto, cancellationToken));

        /// <summary>
        /// اطلاعات گفت و گوی PastilAI
        /// </summary>
        /// <param name="id">شناسه گفت و گو</param>
        /// <param name="cancellationToken">توکن لغو درخواست</param>
        /// <returns></returns>
        [HttpGet("conversations/{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilAiConversationDto>), 200)]
        public async Task<IActionResult> Get(long id, CancellationToken cancellationToken) =>
            Ok(await _chatService.GetAdminConversationAsync(id, cancellationToken));
    }
}
