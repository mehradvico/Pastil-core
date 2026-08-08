using Application.Common.Dto.Result;
using Application.Common.Dto.Input;
using Application.Services.PastilAISrv.Dto;
using Application.Services.PastilAISrv.Iface;
using Application.Services.PastilAISrv.Provider;
using Entities.Entities.PastilAIField;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System.Data;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv
{
    public class PastilAiChatService : IPastilAiChatService
    {
        private readonly IDataBaseContext _context;
        private readonly IPastilAiCompletionRouter _router;
        private readonly PastilAiProviderOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;

        public PastilAiChatService(
            IDataBaseContext context,
            IPastilAiCompletionRouter router,
            IOptions<PastilAiProviderOptions> options,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _router = router;
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResultDto<PastilAiAskResultDto>> AskAsync(long userId, PastilAiAskDto dto, CancellationToken cancellationToken)
        {
            var text = dto.Message?.Trim();
            if (string.IsNullOrWhiteSpace(text) || text.Length > 4000)
                return new BaseResultDto<PastilAiAskResultDto>(false, Resource.Notification.InvalidData, null);

            PastilAiConversation conversation;
            if (dto.ConversationId.HasValue)
            {
                conversation = await _context.PastilAiConversations.AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.ConversationId && x.UserId == userId, cancellationToken);
                if (conversation == null)
                    return new BaseResultDto<PastilAiAskResultDto>(false, Resource.Notification.NothingFound, null);
            }
            else
            {
                conversation = null;
            }

            if (dto.PictureId.HasValue && dto.FileId.HasValue)
                return new BaseResultDto<PastilAiAskResultDto>(false, Resource.Notification.InvalidData, null);

            Picture picture = null;
            Entities.Entities.File mediaFile = null;
            var inputType = PastilAiInputType.Text;
            if (dto.PictureId.HasValue)
            {
                picture = await _context.Pictures.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.PictureId.Value, cancellationToken);
                if (picture == null || !IsAllowedImage(picture))
                    return new BaseResultDto<PastilAiAskResultDto>(false, Resource.Notification.FileNotAllow, null);
                inputType = PastilAiInputType.Image;
            }
            if (dto.FileId.HasValue)
            {
                mediaFile = await _context.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.FileId.Value, cancellationToken);
                inputType = GetFileInputType(mediaFile);
                if (mediaFile == null || inputType == PastilAiInputType.Text || !IsAllowedMediaFile(mediaFile, inputType))
                    return new BaseResultDto<PastilAiAskResultDto>(false, Resource.Notification.FileNotAllow, null);
            }

            var quotaReservation = await ReserveQuotaAsync(userId, inputType, cancellationToken);
            if (!quotaReservation.IsSuccess)
                return new BaseResultDto<PastilAiAskResultDto>(false, quotaReservation.Error, null);

            PastilAiMessage assistant = null;
            try
            {
                if (conversation == null)
                {
                    conversation = new PastilAiConversation
                    {
                        UserId = userId,
                        Title = text.Length <= 100 ? text : text[..100],
                        CreateDateUtc = DateTime.UtcNow,
                        UpdateDateUtc = DateTime.UtcNow
                    };
                    await _context.PastilAiConversations.AddAsync(conversation, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var now = DateTime.UtcNow;
                var userMessage = new PastilAiMessage
                {
                    ConversationId = conversation.Id,
                    Role = PastilAiMessageRole.User,
                    Status = PastilAiMessageStatus.Completed,
                    InputType = inputType,
                    Content = text,
                    CreateDateUtc = now
                };
                await _context.PastilAiMessages.AddAsync(userMessage, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                if (picture != null)
                {
                    await _context.PastilAiAttachments.AddAsync(new PastilAiAttachment
                    {
                        MessageId = userMessage.Id,
                        PictureId = picture.Id,
                        Type = PastilAiInputType.Image
                    }, cancellationToken);
                }
                else if (mediaFile != null)
                {
                    await _context.PastilAiAttachments.AddAsync(new PastilAiAttachment
                    {
                        MessageId = userMessage.Id,
                        FileId = mediaFile.Id,
                        Type = inputType
                    }, cancellationToken);
                }

                assistant = new PastilAiMessage
                {
                    ConversationId = conversation.Id,
                    Role = PastilAiMessageRole.Assistant,
                    Status = PastilAiMessageStatus.Pending,
                    InputType = PastilAiInputType.Text,
                    Content = string.Empty,
                    CreateDateUtc = DateTime.UtcNow
                };
                await _context.PastilAiMessages.AddAsync(assistant, cancellationToken);
                conversation.UpdateDateUtc = DateTime.UtcNow;
                _context.PastilAiConversations.Update(conversation);
                await _context.SaveChangesAsync(cancellationToken);

                var history = await _context.PastilAiMessages.AsNoTracking()
                    .Where(x => x.ConversationId == conversation.Id && x.Id < userMessage.Id &&
                                (x.Role == PastilAiMessageRole.User || x.Role == PastilAiMessageRole.Assistant) &&
                                x.Status == PastilAiMessageStatus.Completed)
                    .OrderByDescending(x => x.Id).Take(12).OrderBy(x => x.Id)
                    .Select(x => new PastilAiProviderChatMessage { Role = x.Role, Content = x.Content })
                    .ToListAsync(cancellationToken);

                var context = await BuildPastilContextAsync(userId, text, dto.ProductId, dto.UserPetId, cancellationToken);
                var providerRequest = new PastilAiProviderRequest
                {
                    SystemPrompt = BuildSystemPrompt(context),
                    UserMessage = text,
                    History = history,
                    PreferredProvider = dto.Provider?.Trim(),
                    InputType = inputType,
                    MediaDataUrl = inputType == PastilAiInputType.Text
                        ? null
                        : await LoadMediaDataUrlAsync(picture, mediaFile, cancellationToken)
                };

                var routed = IsEmergencyIntent(text)
                    ? new PastilAiRoutedResponse
                    {
                        Provider = "PastilSafety",
                        Response = new PastilAiProviderResponse
                        {
                            IsSuccess = true,
                            Answer = "این وضعیت می‌تواند اورژانسی باشد. همین حالا با نزدیک‌ترین مرکز دامپزشکی تماس بگیرید یا حیوان را به مرکز شبانه‌روزی برسانید. برای دریافت پاسخ آنلاین منتظر نمانید.",
                            Scope = PastilAiScope.PetMedical,
                            IsEmergency = true,
                            Model = "emergency-rule-v1"
                        }
                    }
                    : await _router.CompleteAsync(providerRequest, cancellationToken);
                var providerAttemptEntities = routed.Attempts
                    .Select((attempt, index) => new PastilAiProviderAttempt
                    {
                        MessageId = assistant.Id,
                        Provider = attempt.Provider,
                        Model = attempt.Model,

                        AttemptOrder = index + 1,

                        Status = attempt.Response.IsSuccess
                            ? PastilAiProviderAttemptStatus.Succeeded
                            : PastilAiProviderAttemptStatus.Failed,

                        StartDateUtc = attempt.StartDateUtc,
                        EndDateUtc = attempt.EndDateUtc,

                        DurationMilliseconds =
                            (long)(attempt.EndDateUtc - attempt.StartDateUtc)
                            .TotalMilliseconds,

                        HttpStatusCode = attempt.Response.HttpStatusCode,
                        ErrorCode = attempt.Response.ErrorCode,

                        ErrorMessage = Truncate(
                            attempt.Response.ErrorMessage,
                            2000),

                        PromptTokens = attempt.Response.PromptTokens,
                        CompletionTokens = attempt.Response.CompletionTokens
                    })
                    .ToList();

                await _context.PastilAiProviderAttempts.AddRangeAsync(
                    providerAttemptEntities,
                    cancellationToken);
                if (!routed.Response.IsSuccess)
                {
                    assistant.Status = PastilAiMessageStatus.Failed;
                    assistant.Content = "در حال حاضر امکان پاسخ‌گویی وجود ندارد. لطفاً کمی بعد دوباره تلاش کنید.";
                    await ReleaseQuotaAsync(userId, inputType, cancellationToken);
                }
                else
                {
                    assistant.Status = PastilAiMessageStatus.Completed;
                    assistant.Content = routed.Response.Answer;
                    assistant.Scope = routed.Response.Scope;
                    assistant.Provider = routed.Provider;
                    assistant.Model = routed.Response.Model;
                    assistant.PromptTokens = routed.Response.PromptTokens;
                    assistant.CompletionTokens = routed.Response.CompletionTokens;
                    var successful = routed.Attempts.LastOrDefault(x => x.Response.IsSuccess);
                    assistant.DurationMilliseconds = successful == null
                        ? 0
                        : (long)(successful.EndDateUtc - successful.StartDateUtc).TotalMilliseconds;
                }

                _context.PastilAiMessages.Update(assistant);
                await _context.SaveChangesAsync(cancellationToken);
                var quota = await GetQuotaInternalAsync(userId, cancellationToken);
                return new BaseResultDto<PastilAiAskResultDto>(routed.Response.IsSuccess,
                    routed.Response.IsSuccess ? null : assistant.Content,
                    new PastilAiAskResultDto
                    {
                        ConversationId = conversation.Id,
                        UserMessage = MapMessage(userMessage, inputType == PastilAiInputType.Text ? new List<PastilAiAttachment>() :
                            new List<PastilAiAttachment>
                            {
                                new()
                                {
                                    PictureId = picture?.Id, Picture = picture, FileId = mediaFile?.Id,
                                    File = mediaFile, Type = inputType
                                }
                            }),
                        AssistantMessage = MapMessage(assistant, new List<PastilAiAttachment>()),
                        Quota = quota
                    });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                if (assistant != null)
                {
                    assistant.Status = PastilAiMessageStatus.Failed;
                    assistant.Content = "درخواست لغو شد.";
                    _context.PastilAiMessages.Update(assistant);
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
                await ReleaseQuotaAsync(userId, inputType, CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                if (assistant != null)
                {
                    var pendingAttempts = _context.PastilAiProviderAttempts.Local
                        .Where(x =>
                            x.MessageId == assistant.Id &&
                            _context.Entry(x).State == EntityState.Added)
                        .ToList();

                    foreach (var pendingAttempt in pendingAttempts)
                    {
                        _context.Entry(pendingAttempt).State =
                            EntityState.Detached;
                    }

                    assistant.Status = PastilAiMessageStatus.Failed;
                    assistant.Content =
                        "در حال حاضر امکان پاسخ‌گویی وجود ندارد. لطفاً کمی بعد دوباره تلاش کنید.";

                    _context.PastilAiMessages.Update(assistant);

                    await _context.SaveChangesAsync(
                        CancellationToken.None);
                }

                await ReleaseQuotaAsync(
                    userId,
                    inputType,
                    CancellationToken.None);

                return new BaseResultDto<PastilAiAskResultDto>(
                    false,
                    ex.InnerException?.Message ?? ex.Message,
                    null);
            }
        }

        public async Task<PastilAiConversationSearchDto> GetUserConversationsAsync(long userId, BaseInputDto dto, CancellationToken cancellationToken)
        {
            var query = _context.PastilAiConversations.AsNoTracking().Where(x => x.UserId == userId);
            var pageIndex = Math.Max(1, dto.PageIndex);
            var pageSize = Math.Clamp(dto.PageSize, 1, 100);
            var totalCount = await query.CountAsync(cancellationToken);
            var list = await ProjectConversationList(query.OrderByDescending(x => x.UpdateDateUtc))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return new PastilAiConversationSearchDto
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Q = dto.Q,
                SortBy = dto.SortBy,
                TotalCount = totalCount,
                List = list
            };
        }

        public async Task<BaseResultDto<PastilAiConversationDto>> GetUserConversationAsync(long userId, long id, CancellationToken cancellationToken)
        {
            var item = await FullConversationQuery().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
            return item == null
                ? new BaseResultDto<PastilAiConversationDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<PastilAiConversationDto>(true, MapConversation(item, false));
        }

        public async Task<PastilAiConversationSearchDto> SearchAdminAsync(PastilAiConversationInputDto dto, CancellationToken cancellationToken)
        {
            var query = _context.PastilAiConversations.AsNoTracking().AsQueryable();
            if (dto.UserId.HasValue) query = query.Where(x => x.UserId == dto.UserId);
            if (dto.FromDateUtc.HasValue) query = query.Where(x => x.CreateDateUtc >= dto.FromDateUtc);
            if (dto.ToDateUtc.HasValue) query = query.Where(x => x.CreateDateUtc <= dto.ToDateUtc);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                query = query.Where(x => x.Title.Contains(q) || x.User.Mobile.Contains(q) ||
                                         x.User.FirstName.Contains(q) || x.User.LastName.Contains(q));
            }
            var pageIndex = Math.Max(1, dto.PageIndex);
            var pageSize = Math.Clamp(dto.PageSize, 1, 100);
            var totalCount = await query.CountAsync(cancellationToken);
            var list = await ProjectConversationList(query.OrderByDescending(x => x.UpdateDateUtc))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return new PastilAiConversationSearchDto
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Q = dto.Q,
                SortBy = dto.SortBy,
                TotalCount = totalCount,
                List = list
            };
        }

        public async Task<BaseResultDto<PastilAiConversationDto>> GetAdminConversationAsync(long id, CancellationToken cancellationToken)
        {
            var item = await FullConversationQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return item == null
                ? new BaseResultDto<PastilAiConversationDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<PastilAiConversationDto>(true, MapConversation(item, true));
        }

        private IQueryable<PastilAiConversation> FullConversationQuery() =>
            _context.PastilAiConversations.AsNoTracking().AsSplitQuery().Include(x => x.User)
                .Include(x => x.Messages).ThenInclude(x => x.Attachments).ThenInclude(x => x.Picture)
                .Include(x => x.Messages).ThenInclude(x => x.Attachments).ThenInclude(x => x.File)
                .Include(x => x.Messages).ThenInclude(x => x.ProviderAttempts);

        private async Task<(bool IsSuccess, string Error)> ReserveQuotaAsync(long userId, PastilAiInputType inputType, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var (plan, _) = await ResolvePlanAsync(userId, cancellationToken);
                var today = DateTime.UtcNow.Date;
                var usage = await _context.PastilAiDailyUsages.AsTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDate == today, cancellationToken);
                usage ??= new PastilAiDailyUsage { UserId = userId, UsageDate = today };
                var quotaError = PastilAiQuotaPolicy.Validate(plan, usage, inputType);
                if (quotaError != null)
                    return (false, quotaError);
                usage.ChatCount++;
                if (inputType == PastilAiInputType.Image) usage.ImageCount++;
                if (inputType == PastilAiInputType.Audio) usage.AudioCount++;
                if (inputType == PastilAiInputType.Video) usage.VideoCount++;
                if (usage.Id == 0) await _context.PastilAiDailyUsages.AddAsync(usage, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return (true, null);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        private async Task ReleaseQuotaAsync(long userId, PastilAiInputType inputType, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var today = DateTime.UtcNow.Date;
            var usage = await _context.PastilAiDailyUsages.AsTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDate == today, cancellationToken);
            if (usage == null)
                return;
            usage.ChatCount = Math.Max(0, usage.ChatCount - 1);
            if (inputType == PastilAiInputType.Image) usage.ImageCount = Math.Max(0, usage.ImageCount - 1);
            if (inputType == PastilAiInputType.Audio) usage.AudioCount = Math.Max(0, usage.AudioCount - 1);
            if (inputType == PastilAiInputType.Video) usage.VideoCount = Math.Max(0, usage.VideoCount - 1);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task<(PastilAiPlan Plan, PastilAiSubscription Subscription)> ResolvePlanAsync(long userId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var subscription = await _context.PastilAiSubscriptions.AsNoTracking().Include(x => x.Plan)
                .Where(x => x.UserId == userId && x.Status == PastilAiSubscriptionStatus.Active &&
                            x.StartDateUtc <= now && x.EndDateUtc > now && x.Plan.Active)
                .OrderByDescending(x => x.Plan.SortOrder).ThenByDescending(x => x.EndDateUtc)
                .FirstOrDefaultAsync(cancellationToken);
            if (subscription != null) return (subscription.Plan, subscription);
            var free = await _context.PastilAiPlans.AsNoTracking()
                .FirstAsync(x => x.Code == PastilAiPlanCode.Free.ToString() && x.Active, cancellationToken);
            return (free, null);
        }

        private async Task<PastilAiQuotaDto> GetQuotaInternalAsync(long userId, CancellationToken cancellationToken)
        {
            var (plan, subscription) = await ResolvePlanAsync(userId, cancellationToken);
            var today = DateTime.UtcNow.Date;
            var usage = await _context.PastilAiDailyUsages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.UsageDate == today, cancellationToken);
            return new PastilAiQuotaDto
            {
                PlanCode = plan.Code,
                PlanName = plan.Name,
                SubscriptionEndDateUtc = subscription?.EndDateUtc,
                UsedChats = usage?.ChatCount ?? 0,
                UsedImages = usage?.ImageCount ?? 0,
                UsedAudio = usage?.AudioCount ?? 0,
                UsedVideo = usage?.VideoCount ?? 0,
                DailyChatLimit = plan.DailyChatLimit,
                DailyImageLimit = plan.DailyImageLimit,
                DailyAudioLimit = plan.DailyAudioLimit,
                DailyVideoLimit = plan.DailyVideoLimit
            };
        }

        private async Task<string> BuildPastilContextAsync(long userId, string question, long? productId, long? userPetId, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            var productQuery = _context.Products.AsNoTracking().Where(x => !x.Deleted && x.Active);
            productQuery = productId.HasValue
                ? productQuery.Where(x => x.Id == productId.Value)
                : productQuery.Where(x =>
                    question.Contains(x.Name) ||
                    (x.ProductLabel != null && question.Contains(x.ProductLabel)) ||
                    (x.SecondName != null && question.Contains(x.SecondName)));
            var products = await productQuery.Take(5).Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Price,
                x.DiscountPercent,
                AvailableQuantity = x.ProductItems.Where(i => !i.Deleted && i.Active && i.SystemActive && i.Store.Active)
                    .Sum(i => (int?)i.Quantity) ?? 0,
                MinimumItemPrice = x.ProductItems.Where(i => !i.Deleted && i.Active && i.SystemActive && i.Store.Active)
                    .Min(i => (long?)i.Price)
            }).ToListAsync(cancellationToken);
            foreach (var product in products)
                sb.AppendLine($"محصول پاستیل: شناسه={product.Id}، نام={product.Name}، توضیح={product.Description}، " +
                              $"قیمت ثبت‌شده={product.MinimumItemPrice ?? product.Price}، تخفیف={product.DiscountPercent} درصد، " +
                              $"موجودی قابل فروش={product.AvailableQuantity}");
            if (userPetId.HasValue)
            {
                var pet = await _context.UserPets.AsNoTracking().Include(x => x.PetBreed).Include(x => x.PetBreed2)
                    .FirstOrDefaultAsync(x => x.Id == userPetId && x.UserId == userId && !x.Deleted, cancellationToken);
                if (pet != null)
                    sb.AppendLine($"حیوان کاربر: نام={pet.Name}، نژاد={pet.PetBreed?.Name} {pet.PetBreed2?.Name}");
            }

            if (ContainsNearbyIntent(question))
            {
                var location = await _context.UserCurrentLocations.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
                if (location == null)
                    sb.AppendLine("موقعیت فعلی کاربر در پاستیل ثبت نشده است؛ برای نتیجه نزدیک باید از کاربر خواسته شود موقعیتش را ثبت کند.");
                else
                {
                    var stores = await _context.Stores.AsNoTracking()
                        .Where(x => !x.Deleted && x.Active && x.Location != null)
                        .OrderBy(x => x.Location.Distance(location.Location)).Take(3)
                        .Select(x => new { x.Id, x.Name, x.Address, Distance = x.Location.Distance(location.Location) })
                        .ToListAsync(cancellationToken);
                    foreach (var x in stores)
                        sb.AppendLine($"پت‌شاپ نزدیک: id={x.Id}، نام={x.Name}، آدرس={x.Address}، فاصله={Math.Round(x.Distance)} متر");

                    var companions = await _context.Companions.AsNoTracking()
                        .Where(x => !x.Deleted && x.Active && x.Approved && x.Location != null)
                        .OrderBy(x => x.Location.Distance(location.Location)).Take(3)
                        .Select(x => new { x.Id, x.Name, Address = x.AddressValue, Distance = x.Location.Distance(location.Location) })
                        .ToListAsync(cancellationToken);
                    foreach (var x in companions)
                        sb.AppendLine($"مرکز/Companion نزدیک: id={x.Id}، نام={x.Name}، آدرس={x.Address}، فاصله={Math.Round(x.Distance)} متر");
                }
            }
            return sb.Length == 0 ? "داده داخلی مرتبطی برای این سؤال بازیابی نشد." : sb.ToString();
        }

        private static string BuildSystemPrompt(string context) => $$"""
            تو PastilAI، دستیار فارسی پلتفرم پاستیل برای حیوانات خانگی هستی.
            فقط درباره حیوانات خانگی، محصولات و خدمات پاستیل، فروشگاه‌ها، Companionها، نگهداری، تغذیه، رفتار و سلامت عمومی حیوان پاسخ بده.
            اگر موضوع خارج از این حوزه است، محترمانه بگو «این موضوع در دایره خدمات PastilAI نیست.»
            اطلاعات دقیق محصول، قیمت، موجودی، فروشگاه و مرکز را فقط از داده داخلی زیر بیان کن و هرگز آن را حدس نزن.
            اگر داده داخلی کافی نیست، دانش عمومی مرتبط با حیوانات را با شفافیت ارائه کن.
            تشخیص قطعی پزشکی نده. در علائم خطرناک، فوریت مراجعه به دامپزشک را روشن بیان کن.
            پاسخ باید فقط JSON معتبر با این ساختار باشد:
            {"answer":"متن فارسی","scope":"PastilData|PetGeneral|PetMedical|NearbyService|OutOfScope","isEmergency":false}

            داده داخلی پاستیل:
            {{context}}
            """;

        private async Task<string> LoadMediaDataUrlAsync(Picture picture, Entities.Entities.File file, CancellationToken cancellationToken)
        {
            var baseUrl = _options.PublicMediaBaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("PastilAI:PublicMediaBaseUrl is not configured.");
            var url = picture != null
                ? $"{baseUrl}/{picture.Url?.Trim('/')}/{picture.Name}"
                : $"{baseUrl}/{file.Url?.Trim('/')}/{file.Name}";
            var bytes = await _httpClientFactory.CreateClient().GetByteArrayAsync(url, cancellationToken);
            if (bytes.Length > 20 * 1024 * 1024)
                throw new InvalidOperationException("Media exceeds the PastilAI size limit.");
            var contentType = picture?.ContentType ?? file.ContentType;
            return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
        }

        private static bool ContainsNearbyIntent(string value) =>
            value.Contains("نزدیک", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("اطراف", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("پت شاپ", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("پت‌شاپ", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("کلینیک", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("دامپزشک", StringComparison.OrdinalIgnoreCase);

        private static bool IsEmergencyIntent(string value)
        {
            var normalized = value.Replace("‌", " ").ToLowerInvariant();
            return normalized.Contains("نفس نمی") ||
                   normalized.Contains("نفس نمیکش") ||
                   normalized.Contains("بیهوش") ||
                   normalized.Contains("بی هوش") ||
                   normalized.Contains("تشنج") ||
                   normalized.Contains("خونریزی شدید") ||
                   normalized.Contains("خون ریزی شدید") ||
                   normalized.Contains("مسموم") ||
                   normalized.Contains("ادرار نمی") ||
                   normalized.Contains("ادرار نمیکن");
        }

        private static bool IsAllowedImage(Picture picture) =>
            picture.Size > 0 && picture.Size <= 5 * 1024 * 1024 &&
            new[] { "image/jpeg", "image/png", "image/webp" }.Contains(picture.ContentType?.ToLowerInvariant());

        private static PastilAiInputType GetFileInputType(Entities.Entities.File file)
        {
            var contentType = file?.ContentType?.ToLowerInvariant();
            if (contentType?.StartsWith("audio/") == true) return PastilAiInputType.Audio;
            if (contentType?.StartsWith("video/") == true) return PastilAiInputType.Video;
            return PastilAiInputType.Text;
        }

        private static bool IsAllowedMediaFile(Entities.Entities.File file, PastilAiInputType type)
        {
            if (file.Size <= 0 || file.Size > 20 * 1024 * 1024) return false;
            var mime = file.ContentType?.ToLowerInvariant();
            return type switch
            {
                PastilAiInputType.Audio => new[] { "audio/mpeg", "audio/mp3", "audio/wav", "audio/ogg", "audio/webm", "audio/mp4" }.Contains(mime),
                PastilAiInputType.Video => new[] { "video/mp4", "video/webm", "video/quicktime", "video/mpeg" }.Contains(mime),
                _ => false
            };
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];

        private static IQueryable<PastilAiConversationListItemDto> ProjectConversationList(IQueryable<PastilAiConversation> query) =>
            query.Select(x => new PastilAiConversationListItemDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserFullName = (x.User.FirstName + " " + x.User.LastName).Trim(),
                UserMobile = x.User.Mobile,
                Title = x.Title,
                CreateDateUtc = x.CreateDateUtc,
                UpdateDateUtc = x.UpdateDateUtc,
                MessageCount = x.Messages.Count,
                LastMessage = x.Messages.OrderByDescending(m => m.Id).Select(m => m.Content).FirstOrDefault()
            });

        private static PastilAiConversationDto MapConversation(PastilAiConversation x, bool includeAttempts) => new()
        {
            Id = x.Id,
            UserId = x.UserId,
            UserFullName = $"{x.User.FirstName} {x.User.LastName}".Trim(),
            UserMobile = x.User.Mobile,
            Title = x.Title,
            CreateDateUtc = x.CreateDateUtc,
            UpdateDateUtc = x.UpdateDateUtc,
            Messages = x.Messages.OrderBy(m => m.Id).Select(m => MapMessage(m, m.Attachments?.ToList(), includeAttempts)).ToList()
        };

        private static PastilAiMessageDto MapMessage(PastilAiMessage x, List<PastilAiAttachment> attachments, bool includeAttempts = false) => new()
        {
            Id = x.Id,
            Role = x.Role,
            Status = x.Status,
            InputType = x.InputType,
            Scope = x.Scope,
            Content = x.Content,
            Provider = x.Provider,
            Model = x.Model,
            CreateDateUtc = x.CreateDateUtc,
            DurationMilliseconds = x.DurationMilliseconds,
            Attachments = attachments?.Select(a => new PastilAiAttachmentDto
            {
                PictureId = a.PictureId,
                FileId = a.FileId,
                Type = a.Type,
                Url = a.Picture != null
                    ? $"{a.Picture.Url?.TrimEnd('/')}/{a.Picture.Name}"
                    : a.File == null ? null : $"{a.File.Url?.TrimEnd('/')}/{a.File.Name}",
                ContentType = a.Picture?.ContentType ?? a.File?.ContentType
            }).ToList() ?? new(),
            ProviderAttempts = includeAttempts && x.ProviderAttempts != null
                ? x.ProviderAttempts.OrderBy(a => a.AttemptOrder).Select(a => new PastilAiProviderAttemptDto
                {
                    Provider = a.Provider,
                    Model = a.Model,
                    AttemptOrder = a.AttemptOrder,
                    Status = a.Status,
                    DurationMilliseconds = a.DurationMilliseconds,
                    HttpStatusCode = a.HttpStatusCode,
                    ErrorCode = a.ErrorCode,
                    ErrorMessage = a.ErrorMessage
                }).ToList()
                : new()
        };
    }
}
