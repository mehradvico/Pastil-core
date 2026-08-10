using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ورود و داشبورد مدیریت سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteDashboardController : ControllerBase
    {
        private readonly IDataBaseContext _context;

        public SiteDashboardController(IDataBaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// بررسی دسترسی کاربر به پنل مدیریت سایت
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool accessOnly = false)
        {
            if (accessOnly)
            {
                return Ok(new { isSuccess = true });
            }

            var now = DateTime.Now;
            var posts = _context.Posts.AsNoTracking().Where(item => !item.Deleted);
            var comments = _context.PostComments.AsNoTracking().Where(item => !item.Post.Deleted);

            var result = new SiteDashboardDto
            {
                PostCount = await posts.CountAsync(),
                PublishedPostCount = await posts.CountAsync(item => item.Active && item.AdminConfirm == true && item.PublishDate <= now),
                PendingPostCount = await posts.CountAsync(item => item.AdminConfirm != true),
                ScheduledPostCount = await posts.CountAsync(item => item.Active && item.AdminConfirm == true && item.PublishDate > now),
                VisitCount = await posts.SumAsync(item => (int?)item.VisitCount) ?? 0,
                CommentCount = await comments.CountAsync(),
                PendingCommentCount = await comments.CountAsync(item => item.Status.Label == CommentEnum.Comment_NotChecked.ToString()),
                LikeCount = await comments.SumAsync(item => (int?)item.LikeCount) ?? 0,
                BannerCount = await _context.Banners.AsNoTracking().CountAsync(item => !item.Deleted && item.ShowToSite),
                GalleryCount = await _context.Galleries.AsNoTracking().CountAsync(item => !item.Deleted && item.Active),
                CompanionCount = await _context.Companions.AsNoTracking().CountAsync(item => !item.Deleted && item.ShowToSite),
                AssistanceCount = await _context.Assistances.AsNoTracking().CountAsync(item => !item.Deleted && item.ShowToSite),
                PansionCount = await _context.Pansions.AsNoTracking().CountAsync(item => item.ShowToSite),
                StoreCount = await _context.Stores.AsNoTracking().CountAsync(item => !item.Deleted && item.ShowToSite),
                RecentPosts = await posts
                    .OrderByDescending(item => item.PublishDate)
                    .Take(6)
                    .Select(item => new SiteDashboardPostDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Subject = item.Subject,
                        PublishDate = item.PublishDate,
                        Active = item.Active,
                        AdminConfirm = item.AdminConfirm,
                        VisitCount = item.VisitCount,
                        CommentCount = item.CommentCount
                    })
                    .ToListAsync(),
                RecentComments = await comments
                    .OrderByDescending(item => item.CreateDate)
                    .Take(6)
                    .Select(item => new SiteDashboardCommentDto
                    {
                        Id = item.Id,
                        PostId = item.PostId,
                        PostName = item.Post.Name,
                        Text = item.Text,
                        Answer = item.Answer,
                        CreateDate = item.CreateDate,
                        LikeCount = item.LikeCount,
                        StatusName = item.Status.Name,
                        StatusLabel = item.Status.Label,
                        UserName = item.User == null
                            ? string.Empty
                            : ((item.User.FirstName + " " + item.User.LastName).Trim() != string.Empty
                                ? (item.User.FirstName + " " + item.User.LastName).Trim()
                                : item.User.Mobile)
                    })
                    .ToListAsync()
            };

            return Ok(new BaseResultDto<SiteDashboardDto>(true, result));
        }
    }
}
