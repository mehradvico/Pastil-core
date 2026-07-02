using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class DiscussionAnswer : Id_Field
    {
        public string Content { get; set; }
        public long DiscussionQuestionId { get; set; }
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public int LikeCount { get; set; }
        public int DisLikeCount { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }


        public DiscussionQuestion DiscussionQuestion { get; set; }
        public User User { get; set; }
        public ICollection<DiscussionAnswerLike> DiscussionAnswerLikes { get; set; }

    }
}
