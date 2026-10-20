using Application.Common.Dto.Input;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto
{
    /// <summary>نوع رسانه‌ی نمونه‌کار.</summary>
    public enum GroomerPortfolioPictureKind
    {
        Normal = 0,
        Before = 1,
        After = 2,
        Video = 3,
    }

    public class GroomerPortfolioPictureInputDto
    {
        public long PictureId { get; set; }
        public int Kind { get; set; }
        public string Label { get; set; }
    }

    /// <summary>ساخت/ویرایش پست نمونه‌کار. ترتیب Pictures همان ترتیب نمایش است.</summary>
    public class GroomerPortfolioPostInputDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public string Caption { get; set; }
        public long? PetId { get; set; }
        public long? GroomerServiceId { get; set; }
        public List<GroomerPortfolioPictureInputDto> Pictures { get; set; } = new List<GroomerPortfolioPictureInputDto>();
    }

    public class GroomerPortfolioPictureVDto
    {
        public long Id { get; set; }
        public long PictureId { get; set; }
        public int Kind { get; set; }
        public int SortOrder { get; set; }
        public string Label { get; set; }
        public PictureVDto Picture { get; set; }
    }

    public class GroomerPortfolioPostVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public string GroomerName { get; set; }
        public PictureVDto GroomerIcon { get; set; }
        public string Caption { get; set; }
        public long? PetId { get; set; }
        public string PetName { get; set; }
        public long? GroomerServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime CreateDate { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        /// <summary>کاربر جاری این پست را لایک کرده است (برای کاربر ناشناس همیشه false).</summary>
        public bool LikedByMe { get; set; }
        public bool Approved { get; set; }
        public bool Active { get; set; }
        public string ApprovalValue { get; set; }
        public List<GroomerPortfolioPictureVDto> Pictures { get; set; } = new List<GroomerPortfolioPictureVDto>();
    }

    public class GroomerPortfolioPostSearchInputDto : BaseInputDto
    {
        public long? GroomerId { get; set; }
        public long? CityId { get; set; }
        public long? PetId { get; set; }
        public long? GroomerServiceId { get; set; }
        /// <summary>فقط برای ادمین/مالک: فیلتر وضعیت تأیید (در فید عمومی نادیده گرفته می‌شود).</summary>
        public bool? Approved { get; set; }
    }

    public class GroomerPortfolioPage<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> List { get; set; } = new List<T>();
    }

    public class GroomerPortfolioCommentInputDto
    {
        public long PostId { get; set; }
        public string Text { get; set; }
        /// <summary>اگر پر باشد پاسخ به یک نظر است (پاسخ به پاسخ به نظر اصلی وصل می‌شود).</summary>
        public long? ParentId { get; set; }
    }

    public class GroomerPortfolioCommentVDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long? ParentId { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public bool IsMine { get; set; }
        public int ReplyCount { get; set; }
    }

    public class GroomerPortfolioCommentSearchInputDto : BaseInputDto
    {
        public long PostId { get; set; }
        /// <summary>خالی = نظرهای اصلی؛ پر = پاسخ‌های همان نظر.</summary>
        public long? ParentId { get; set; }
    }

    public class GroomerPortfolioLikeResultDto
    {
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
    }

    public class GroomerPortfolioModerationDto
    {
        public long Id { get; set; }
        public bool Approved { get; set; }
        public string ApprovalValue { get; set; }
    }
}
