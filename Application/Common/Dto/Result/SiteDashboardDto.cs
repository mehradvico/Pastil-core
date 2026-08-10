using System;
using System.Collections.Generic;

namespace Application.Common.Dto.Result
{
    public class SiteDashboardDto
    {
        public int PostCount { get; set; }
        public int PublishedPostCount { get; set; }
        public int PendingPostCount { get; set; }
        public int ScheduledPostCount { get; set; }
        public int VisitCount { get; set; }
        public int CommentCount { get; set; }
        public int PendingCommentCount { get; set; }
        public int LikeCount { get; set; }
        public int BannerCount { get; set; }
        public int GalleryCount { get; set; }
        public int CompanionCount { get; set; }
        public int AssistanceCount { get; set; }
        public int PansionCount { get; set; }
        public int StoreCount { get; set; }
        public List<SiteDashboardPostDto> RecentPosts { get; set; } = new();
        public List<SiteDashboardCommentDto> RecentComments { get; set; } = new();
    }

    public class SiteDashboardPostDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public DateTime PublishDate { get; set; }
        public bool Active { get; set; }
        public bool? AdminConfirm { get; set; }
        public int VisitCount { get; set; }
        public int CommentCount { get; set; }
    }

    public class SiteDashboardCommentDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public string PostName { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreateDate { get; set; }
        public int LikeCount { get; set; }
        public string StatusName { get; set; }
        public string StatusLabel { get; set; }
        public string UserName { get; set; }
    }
}
