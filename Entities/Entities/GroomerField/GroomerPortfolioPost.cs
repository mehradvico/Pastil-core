using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.GroomerField
{
    /// <summary>
    /// نمونه‌کار آرایشگاه به شکل پست (اینستاگرامی): چند عکس/ویدیو، توضیح، لایک و کامنت کاربران.
    /// پست جدید تا تأیید ادمین در فرانت نمایش داده نمی‌شود.
    /// </summary>
    public class GroomerPortfolioPost : Id_Field
    {
        public long GroomerId { get; set; }
        public string Caption { get; set; }
        public long? PetId { get; set; }
        public long? GroomerServiceId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ApprovalValue { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        public Groomer Groomer { get; set; }
        public Pet Pet { get; set; }
        public GroomerService GroomerService { get; set; }
        public ICollection<GroomerPortfolioPicture> Pictures { get; set; }
        public ICollection<GroomerPortfolioLike> Likes { get; set; }
        public ICollection<GroomerPortfolioComment> Comments { get; set; }
    }
}
