using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Post : Seo_Full_Field
    {
        public DateTime CreateDate { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public bool? AdminConfirm { get; set; }
        public int VisitCount { get; set; }
        public string Subject { get; set; }
        public string SubNews { get; set; }
        public bool IsOld { get; set; }
        public string PictureUrl { get; set; }
        //FKey
        public long? CategoryId { get; set; }
        public long? PictureId { get; set; }
        public long UserId { get; set; }
        public long? AdminId { get; set; }
        public long? ParentId { get; set; }
        public bool Edited { get; set; }
        public int CommentCount { get; set; }
        //Relation
        public User User { get; set; }
        public User Admin { get; set; }
        public Picture Picture { get; set; }
        public Category Category { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<PostFile> PostFiles { get; set; }
        public ICollection<PostPicture> PostPictures { get; set; }
        public ICollection<Hashtag> Hashtags { get; set; }
        public ICollection<PostComment> PostComments { get; set; }
        public ICollection<Post> Children { get; set; }
        public Post Parent { get; set; }
        public ICollection<Product> Products { get; set; }

    }
}
