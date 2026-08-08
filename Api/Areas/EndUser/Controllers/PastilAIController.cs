using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilAISrv.Dto;
using Application.Services.PastilAISrv.Iface;
using Application.Services.PastilAISrv.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت گفت و گوهای PastilAI
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilAIController : ControllerBase
    {
        private readonly IPastilAiChatService _chatService;
        private readonly IPastilAiPlanService _planService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly PastilAiProviderOptions _providerOptions;

        /// <summary>
        /// مدیریت گفت و گوهای PastilAI
        /// </summary>
        public PastilAIController(
            IPastilAiChatService chatService,
            IPastilAiPlanService planService,
            ICurrentUserHelper currentUser,
            IOptions<PastilAiProviderOptions> providerOptions)
        {
            _chatService = chatService;
            _planService = planService;
            _currentUser = currentUser;
            _providerOptions = providerOptions.Value;
        }

        /// <summary>
        /// لیست Providerهای فعال PastilAI بدون نمایش کلید API
        /// </summary>
        [HttpGet("providers")]
        [ProducesResponseType(typeof(BaseResultDto<List<PastilAiProviderVDto>>), 200)]
        public IActionResult Providers()
        {
            var providers = _providerOptions.Providers
                .Where(x => x.Enabled)
                .OrderBy(x => x.Order)
                .Select(x => new PastilAiProviderVDto
                {
                    Name = x.Name,
                    TextModel = x.TextModel,
                    VisionModel = x.VisionModel,
                    Order = x.Order,
                    Configured = !string.IsNullOrWhiteSpace(x.ResolveApiKey()),
                    SupportsImage = x.SupportsImage,
                    SupportsAudio = x.SupportsAudio,
                    SupportsVideo = x.SupportsVideo
                })
                .ToList();

            return Ok(new BaseResultDto<List<PastilAiProviderVDto>>(true, providers));
        }

        /// <summary>
        /// لیست پلن های قابل استفاده PastilAI
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<List<PastilAiPlanVDto>>), 200)]
        public async Task<IActionResult> Plans(CancellationToken cancellationToken) =>
            Ok(await _planService.GetPlansAsync(false, cancellationToken));

        /// <summary>
        /// سهمیه روزانه کاربر
        /// </summary>
        [HttpGet("quota")]
        [ProducesResponseType(typeof(BaseResultDto<PastilAiQuotaDto>), 200)]
        public async Task<IActionResult> Quota(CancellationToken cancellationToken) =>
            Ok(await _planService.GetQuotaAsync(_currentUser.CurrentUser.UserId, cancellationToken));

        /// <summary>
        /// خرید اشتراک PastilAI
        /// </summary>
        [HttpPost("purchase")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Purchase(PastilAiPurchaseDto dto, CancellationToken cancellationToken) =>
            Ok(await _planService.PurchaseAsync(_currentUser.CurrentUser.UserId, dto, cancellationToken));

        /// <summary>
        /// ارسال پرسش به PastilAI
        /// </summary>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(BaseResultDto<PastilAiAskResultDto>), 200)]
        public async Task<IActionResult> Ask(PastilAiAskDto dto, CancellationToken cancellationToken) =>
            Ok(await _chatService.AskAsync(_currentUser.CurrentUser.UserId, dto, cancellationToken));

        /// <summary>
        /// جستجوی گفت و گوهای کاربر
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(PastilAiConversationSearchDto), 200)]
        public async Task<IActionResult> Conversations([FromQuery] BaseInputDto dto, CancellationToken cancellationToken) =>
            Ok(await _chatService.GetUserConversationsAsync(_currentUser.CurrentUser.UserId, dto, cancellationToken));

        /// <summary>
        /// اطلاعات گفت و گوی کاربر
        /// </summary>
        [HttpGet("conversations/{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilAiConversationDto>), 200)]
        public async Task<IActionResult> Conversation(long id, CancellationToken cancellationToken) =>
            Ok(await _chatService.GetUserConversationAsync(_currentUser.CurrentUser.UserId, id, cancellationToken));
    }
}
