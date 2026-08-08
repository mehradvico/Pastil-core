using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiMessage : Id_Field
    {
        public long ConversationId { get; set; }
        public PastilAiMessageRole Role { get; set; }
        public PastilAiMessageStatus Status { get; set; }
        public PastilAiInputType InputType { get; set; }
        public PastilAiScope Scope { get; set; }
        public string Content { get; set; }
        [MaxLength(100)]
        public string Provider { get; set; }
        [MaxLength(200)]
        public string Model { get; set; }
        public string MetadataJson { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public long? DurationMilliseconds { get; set; }
        public PastilAiConversation Conversation { get; set; }
        public ICollection<PastilAiAttachment> Attachments { get; set; }
        public ICollection<PastilAiProviderAttempt> ProviderAttempts { get; set; }
    }
}
