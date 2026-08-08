using Entities.Entities.CommonField;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiProviderAttempt : Id_Field
    {
        public long MessageId { get; set; }
        [MaxLength(100)]
        public string Provider { get; set; }
        [MaxLength(200)]
        public string Model { get; set; }
        public int AttemptOrder { get; set; }
        public PastilAiProviderAttemptStatus Status { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public long? DurationMilliseconds { get; set; }
        public int? HttpStatusCode { get; set; }
        [MaxLength(100)]
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public PastilAiMessage Message { get; set; }
    }
}
