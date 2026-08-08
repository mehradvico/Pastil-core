using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiConversation : Id_Field
    {
        public long UserId { get; set; }
        [MaxLength(200)]
        public string Title { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime UpdateDateUtc { get; set; }
        public bool Deleted { get; set; }
        public User User { get; set; }
        public ICollection<PastilAiMessage> Messages { get; set; }
    }
}
