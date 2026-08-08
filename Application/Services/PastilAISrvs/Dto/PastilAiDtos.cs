using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Entities.Entities.PastilAIField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilAISrv.Dto
{
    public class PastilAiPlanVDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int? DailyChatLimit { get; set; }
        public int? DailyImageLimit { get; set; }
        public int? DailyAudioLimit { get; set; }
        public int? DailyVideoLimit { get; set; }
        public bool PurchaseEnabled { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
    }

    public class PastilAiPlanUpdateDto
    {
        public long Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Range(1, 3650)]
        public int DurationDays { get; set; }
        [Range(1, int.MaxValue)]
        public int? DailyChatLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyImageLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyAudioLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyVideoLimit { get; set; }
        public bool PurchaseEnabled { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
    }

    public class PastilAiPurchaseDto
    {
        public long PlanId { get; set; }
        public long MerchantId { get; set; }
    }

    public class PastilAiQuotaDto
    {
        public string PlanCode { get; set; }
        public string PlanName { get; set; }
        public DateTime? SubscriptionEndDateUtc { get; set; }
        public int UsedChats { get; set; }
        public int UsedImages { get; set; }
        public int UsedAudio { get; set; }
        public int UsedVideo { get; set; }
        public int? DailyChatLimit { get; set; }
        public int? DailyImageLimit { get; set; }
        public int? DailyAudioLimit { get; set; }
        public int? DailyVideoLimit { get; set; }
        public int? RemainingChats => DailyChatLimit.HasValue ? Math.Max(0, DailyChatLimit.Value - UsedChats) : null;
        public int? RemainingImages => DailyImageLimit.HasValue ? Math.Max(0, DailyImageLimit.Value - UsedImages) : null;
    }

    public class PastilAiConversationInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public DateTime? FromDateUtc { get; set; }
        public DateTime? ToDateUtc { get; set; }
    }

    public class PastilAiConversationListItemDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string Title { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime UpdateDateUtc { get; set; }
        public int MessageCount { get; set; }
        public string LastMessage { get; set; }
    }

    public class PastilAiConversationSearchDto : BaseSearchDto<PastilAiConversationListItemDto>
    {
    }

    public class PastilAiMessageDto
    {
        public long Id { get; set; }
        public PastilAiMessageRole Role { get; set; }
        public PastilAiMessageStatus Status { get; set; }
        public PastilAiInputType InputType { get; set; }
        public PastilAiScope Scope { get; set; }
        public string Content { get; set; }
        public string Provider { get; set; }
        public string Model { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public long? DurationMilliseconds { get; set; }
        public List<PastilAiAttachmentDto> Attachments { get; set; } = new();
        public List<PastilAiProviderAttemptDto> ProviderAttempts { get; set; } = new();
    }

    public class PastilAiAttachmentDto
    {
        public long? PictureId { get; set; }
        public long? FileId { get; set; }
        public PastilAiInputType Type { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
    }

    public class PastilAiProviderAttemptDto
    {
        public string Provider { get; set; }
        public string Model { get; set; }
        public int AttemptOrder { get; set; }
        public PastilAiProviderAttemptStatus Status { get; set; }
        public long? DurationMilliseconds { get; set; }
        public int? HttpStatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PastilAiConversationDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string Title { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime UpdateDateUtc { get; set; }
        public List<PastilAiMessageDto> Messages { get; set; } = new();
    }

    public class PastilAiAskDto
    {
        public long? ConversationId { get; set; }
        [Required, MaxLength(4000)]
        public string Message { get; set; }
        public long? PictureId { get; set; }
        public long? FileId { get; set; }
        public long? ProductId { get; set; }
        public long? UserPetId { get; set; }
    }

    public class PastilAiAskResultDto
    {
        public long ConversationId { get; set; }
        public PastilAiMessageDto UserMessage { get; set; }
        public PastilAiMessageDto AssistantMessage { get; set; }
        public PastilAiQuotaDto Quota { get; set; }
    }
}
