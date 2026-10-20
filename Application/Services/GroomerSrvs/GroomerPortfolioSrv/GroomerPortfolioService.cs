using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using AutoMapper;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerPortfolioSrv
{
    public class GroomerPortfolioService : IGroomerPortfolioService
    {
        private const int MaxPictures = 10;
        private const int MaxCaptionLength = 2000;
        private const int MaxCommentLength = 1000;

        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GroomerPortfolioService> _logger;

        public GroomerPortfolioService(IDataBaseContext context, IMapper mapper, ILogger<GroomerPortfolioService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ------------------------------------------------------------------ پست‌ها (مدیریت)

        public async Task<BaseResultDto<GroomerPortfolioPostVDto>> CreateAsync(GroomerPortfolioPostInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.GroomerId <= 0)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.InvalidData);

                var groomer = await _context.Groomers.AsNoTracking()
                    .Where(g => g.Id == dto.GroomerId && !g.Deleted)
                    .Select(g => new { g.Id, g.OwnerId, g.Approved })
                    .FirstOrDefaultAsync();
                if (groomer == null)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.NothingFound);
                if (ownerUserId.HasValue && groomer.OwnerId != ownerUserId.Value)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.AccessDenied);
                // آرایشگاه تأییدنشده هنوز فعال نشده؛ نمونه‌کارش هم منتشر نمی‌شود.
                if (!groomer.Approved)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.AccessDenied);

                var error = await ValidatePostAsync(dto);
                if (error != null)
                    return Fail<GroomerPortfolioPostVDto>(error);

                var post = new GroomerPortfolioPost
                {
                    GroomerId = dto.GroomerId,
                    Caption = dto.Caption?.Trim(),
                    PetId = dto.PetId,
                    GroomerServiceId = dto.GroomerServiceId,
                    Active = true,
                    // پست‌ها بعد از انتشار بازبینی می‌شوند؛ ادمین می‌تواند هر پست را پنهان کند (ModerateAsync).
                    Approved = true,
                    CreateDate = DateTime.Now,
                    Pictures = BuildPictures(dto),
                };
                await _context.GroomerPortfolioPosts.AddAsync(post);
                await _context.SaveChangesAsync();
                await RefreshGroomerCountersAsync(dto.GroomerId);
                return await GetManageAsync(post.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating portfolio post for groomer {GroomerId} failed.", dto?.GroomerId);
                return Fail<GroomerPortfolioPostVDto>(Resource.Notification.Unsuccess);
            }
        }

        public async Task<BaseResultDto<GroomerPortfolioPostVDto>> UpdateAsync(GroomerPortfolioPostInputDto dto, long? ownerUserId)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.InvalidData);

                var post = await _context.GroomerPortfolioPosts.AsTracking()
                    .Include(p => p.Pictures)
                    .FirstOrDefaultAsync(p => p.Id == dto.Id && !p.Deleted);
                if (post == null)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.PortfolioPostNotFound);

                var ownerId = await _context.Groomers.AsNoTracking().Where(g => g.Id == post.GroomerId).Select(g => g.OwnerId).FirstOrDefaultAsync();
                if (ownerUserId.HasValue && ownerId != ownerUserId.Value)
                    return Fail<GroomerPortfolioPostVDto>(Resource.Notification.AccessDenied);

                // آرایشگاه پست را عوض نمی‌کند (GroomerId از خود پست می‌آید).
                dto.GroomerId = post.GroomerId;
                var error = await ValidatePostAsync(dto);
                if (error != null)
                    return Fail<GroomerPortfolioPostVDto>(error);

                post.Caption = dto.Caption?.Trim();
                post.PetId = dto.PetId;
                post.GroomerServiceId = dto.GroomerServiceId;
                _context.GroomerPortfolioPictures.RemoveRange(post.Pictures);
                foreach (var picture in BuildPictures(dto))
                {
                    picture.PostId = post.Id;
                    await _context.GroomerPortfolioPictures.AddAsync(picture);
                }
                await _context.SaveChangesAsync();
                return await GetManageAsync(post.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating portfolio post {PostId} failed.", dto?.Id);
                return Fail<GroomerPortfolioPostVDto>(Resource.Notification.Unsuccess);
            }
        }

        public async Task<BaseResultDto> DeleteAsync(long postId, long? ownerUserId)
        {
            var post = await _context.GroomerPortfolioPosts.AsNoTracking()
                .Where(p => p.Id == postId && !p.Deleted)
                .Select(p => new { p.Id, p.GroomerId })
                .FirstOrDefaultAsync();
            if (post == null)
                return new BaseResultDto(false, Resource.Notification.PortfolioPostNotFound);

            var ownerId = await _context.Groomers.AsNoTracking().Where(g => g.Id == post.GroomerId).Select(g => g.OwnerId).FirstOrDefaultAsync();
            if (ownerUserId.HasValue && ownerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            await _context.GroomerPortfolioPosts.Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Deleted, true));
            await RefreshGroomerCountersAsync(post.GroomerId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> ModerateAsync(GroomerPortfolioModerationDto dto)
        {
            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.ApprovalValue))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterTheActivationValueReason);

            var post = await _context.GroomerPortfolioPosts.AsTracking().FirstOrDefaultAsync(p => p.Id == dto.Id && !p.Deleted);
            if (post == null)
                return new BaseResultDto(false, Resource.Notification.PortfolioPostNotFound);

            post.Approved = dto.Approved;
            post.ApprovalValue = dto.Approved ? null : dto.ApprovalValue.Trim();
            await _context.SaveChangesAsync();
            await RefreshGroomerCountersAsync(post.GroomerId);
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ فید و جستجو

        public async Task<GroomerPortfolioPage<GroomerPortfolioPostVDto>> SearchPublicAsync(GroomerPortfolioPostSearchInputDto input, long? userId)
        {
            var query = _context.GroomerPortfolioPosts.AsNoTracking()
                .Where(p => !p.Deleted && p.Approved && p.Active && p.Groomer.Active && p.Groomer.Approved && !p.Groomer.Deleted);
            return await PageAsync(query, input, userId, includeModeration: false);
        }

        public async Task<BaseResultDto<GroomerPortfolioPostVDto>> GetPublicAsync(long postId, long? userId)
        {
            var query = _context.GroomerPortfolioPosts.AsNoTracking()
                .Where(p => p.Id == postId && !p.Deleted && p.Approved && p.Active && p.Groomer.Active && p.Groomer.Approved && !p.Groomer.Deleted);
            var item = (await ProjectAsync(query, userId, includeModeration: false)).FirstOrDefault();
            return item == null
                ? Fail<GroomerPortfolioPostVDto>(Resource.Notification.PortfolioPostNotFound)
                : new BaseResultDto<GroomerPortfolioPostVDto>(true, item);
        }

        public async Task<GroomerPortfolioPage<GroomerPortfolioPostVDto>> SearchManageAsync(GroomerPortfolioPostSearchInputDto input, long? ownerUserId)
        {
            var query = _context.GroomerPortfolioPosts.AsNoTracking().Where(p => !p.Deleted && !p.Groomer.Deleted);
            if (ownerUserId.HasValue)
                query = query.Where(p => p.Groomer.OwnerId == ownerUserId.Value);
            if (input.Approved.HasValue)
                query = query.Where(p => p.Approved == input.Approved.Value);
            return await PageAsync(query, input, ownerUserId, includeModeration: true);
        }

        // ------------------------------------------------------------------ لایک

        public async Task<BaseResultDto<GroomerPortfolioLikeResultDto>> LikeAsync(long postId, long userId)
        {
            var visible = await IsPublicPostAsync(postId);
            if (!visible)
                return Fail<GroomerPortfolioLikeResultDto>(Resource.Notification.PortfolioPostNotFound);

            if (!await _context.GroomerPortfolioLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId))
            {
                try
                {
                    await _context.GroomerPortfolioLikes.AddAsync(new GroomerPortfolioLike { PostId = postId, UserId = userId, CreateDate = DateTime.Now });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // لایک هم‌زمان همان کاربر (ایندکس یکتا): یعنی قبلاً ثبت شده؛ ادامه می‌دهیم و فقط شمارنده را درست می‌کنیم.
                    (_context as DbContext)?.ChangeTracker.Clear();
                }
            }
            return await RefreshLikeResultAsync(postId, liked: true);
        }

        public async Task<BaseResultDto<GroomerPortfolioLikeResultDto>> UnlikeAsync(long postId, long userId)
        {
            var exists = await _context.GroomerPortfolioPosts.AsNoTracking().AnyAsync(p => p.Id == postId && !p.Deleted);
            if (!exists)
                return Fail<GroomerPortfolioLikeResultDto>(Resource.Notification.PortfolioPostNotFound);

            await _context.GroomerPortfolioLikes.Where(l => l.PostId == postId && l.UserId == userId).ExecuteDeleteAsync();
            return await RefreshLikeResultAsync(postId, liked: false);
        }

        // ------------------------------------------------------------------ کامنت

        public async Task<BaseResultDto<GroomerPortfolioCommentVDto>> AddCommentAsync(GroomerPortfolioCommentInputDto dto, long userId)
        {
            try
            {
                var text = dto?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return Fail<GroomerPortfolioCommentVDto>(Resource.Notification.GroomerCommentTextRequired);
                if (text.Length > MaxCommentLength)
                    return Fail<GroomerPortfolioCommentVDto>(Resource.Notification.PortfolioCommentTooLong);
                if (!await IsPublicPostAsync(dto.PostId))
                    return Fail<GroomerPortfolioCommentVDto>(Resource.Notification.PortfolioPostNotFound);

                long? parentId = null;
                if (dto.ParentId.HasValue)
                {
                    var parent = await _context.GroomerPortfolioComments.AsNoTracking()
                        .Where(c => c.Id == dto.ParentId.Value && c.PostId == dto.PostId && !c.Deleted)
                        .Select(c => new { c.Id, c.ParentId })
                        .FirstOrDefaultAsync();
                    if (parent == null)
                        return Fail<GroomerPortfolioCommentVDto>(Resource.Notification.PortfolioParentCommentInvalid);
                    // فقط یک سطح پاسخ (مثل اینستاگرام): پاسخ به پاسخ به همان نظر اصلی وصل می‌شود.
                    parentId = parent.ParentId ?? parent.Id;
                }

                var comment = new GroomerPortfolioComment
                {
                    PostId = dto.PostId,
                    ParentId = parentId,
                    Text = text,
                    UserId = userId,
                    StatusId = (long)CommentEnum.Comment_Accept,
                    CreateDate = DateTime.Now,
                };
                await _context.GroomerPortfolioComments.AddAsync(comment);
                await _context.SaveChangesAsync();
                await RefreshCommentCountAsync(dto.PostId);

                var user = await _context.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => new { u.FirstName, u.LastName }).FirstOrDefaultAsync();
                return new BaseResultDto<GroomerPortfolioCommentVDto>(true, new GroomerPortfolioCommentVDto
                {
                    Id = comment.Id,
                    PostId = comment.PostId,
                    ParentId = comment.ParentId,
                    Text = comment.Text,
                    CreateDate = comment.CreateDate,
                    UserId = userId,
                    UserName = DisplayName(user?.FirstName, user?.LastName),
                    IsMine = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adding portfolio comment on post {PostId} failed.", dto?.PostId);
                return Fail<GroomerPortfolioCommentVDto>(Resource.Notification.Unsuccess);
            }
        }

        public async Task<GroomerPortfolioPage<GroomerPortfolioCommentVDto>> SearchCommentsAsync(GroomerPortfolioCommentSearchInputDto input, long? userId)
        {
            var accept = (long)CommentEnum.Comment_Accept;
            var query = _context.GroomerPortfolioComments.AsNoTracking()
                .Where(c => c.PostId == input.PostId && !c.Deleted && c.StatusId == accept && c.ParentId == input.ParentId);
            // نظر اصلی‌ها جدیدترین اول؛ پاسخ‌ها قدیمی‌ترین اول (مثل گفتگو).
            query = input.ParentId.HasValue ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id);

            var total = await query.CountAsync();
            var rows = await query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)
                .Select(c => new
                {
                    c.Id, c.PostId, c.ParentId, c.Text, c.CreateDate, c.UserId,
                    FirstName = c.User.FirstName, LastName = c.User.LastName,
                    ReplyCount = _context.GroomerPortfolioComments.Count(r => r.ParentId == c.Id && !r.Deleted && r.StatusId == accept),
                })
                .ToListAsync();

            return new GroomerPortfolioPage<GroomerPortfolioCommentVDto>
            {
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                TotalCount = total,
                List = rows.Select(c => new GroomerPortfolioCommentVDto
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    ParentId = c.ParentId,
                    Text = c.Text,
                    CreateDate = c.CreateDate,
                    UserId = c.UserId,
                    UserName = DisplayName(c.FirstName, c.LastName),
                    IsMine = userId.HasValue && c.UserId == userId,
                    ReplyCount = c.ReplyCount,
                }).ToList(),
            };
        }

        public async Task<BaseResultDto> DeleteCommentAsync(long commentId, long userId, bool isAdmin)
        {
            var comment = await _context.GroomerPortfolioComments.AsNoTracking()
                .Where(c => c.Id == commentId && !c.Deleted)
                .Select(c => new { c.Id, c.PostId, c.UserId, GroomerOwnerId = c.Post.Groomer.OwnerId })
                .FirstOrDefaultAsync();
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (!isAdmin && comment.UserId != userId && comment.GroomerOwnerId != userId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            // با حذف نظر اصلی، پاسخ‌هایش هم حذف می‌شوند.
            await _context.GroomerPortfolioComments
                .Where(c => c.Id == commentId || c.ParentId == commentId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Deleted, true));
            await RefreshCommentCountAsync(comment.PostId);
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کمکی

        private async Task<string> ValidatePostAsync(GroomerPortfolioPostInputDto dto)
        {
            var pictures = dto.Pictures ?? new List<GroomerPortfolioPictureInputDto>();
            if (pictures.Count == 0)
                return Resource.Notification.PortfolioNeedsPicture;
            if (pictures.Count > MaxPictures)
                return Resource.Notification.PortfolioTooManyPictures;
            if ((dto.Caption?.Trim().Length ?? 0) > MaxCaptionLength)
                return Resource.Notification.PortfolioCaptionTooLong;
            if (pictures.Any(p => p.Kind < 0 || p.Kind > (int)GroomerPortfolioPictureKind.Video))
                return Resource.Notification.InvalidData;

            var ids = pictures.Select(p => p.PictureId).Distinct().ToList();
            if (ids.Count != pictures.Count || ids.Any(id => id <= 0) || await _context.Pictures.CountAsync(p => ids.Contains(p.Id)) != ids.Count)
                return Resource.Notification.PortfolioPictureInvalid;

            if (dto.PetId.HasValue && !await _context.Pets.AnyAsync(p => p.Id == dto.PetId.Value))
                return Resource.Notification.InvalidData;
            if (dto.GroomerServiceId.HasValue &&
                !await _context.GroomerServices.AnyAsync(s => s.Id == dto.GroomerServiceId.Value && s.GroomerId == dto.GroomerId && !s.Deleted))
                return Resource.Notification.InvalidData;
            return null;
        }

        private static List<GroomerPortfolioPicture> BuildPictures(GroomerPortfolioPostInputDto dto) =>
            dto.Pictures.Select((p, index) => new GroomerPortfolioPicture
            {
                PictureId = p.PictureId,
                Kind = p.Kind,
                SortOrder = index,
                Label = string.IsNullOrWhiteSpace(p.Label) ? null : p.Label.Trim(),
            }).ToList();

        private async Task<bool> IsPublicPostAsync(long postId) =>
            await _context.GroomerPortfolioPosts.AsNoTracking()
                .AnyAsync(p => p.Id == postId && !p.Deleted && p.Approved && p.Active && p.Groomer.Active && p.Groomer.Approved && !p.Groomer.Deleted);

        private async Task<BaseResultDto<GroomerPortfolioPostVDto>> GetManageAsync(long postId)
        {
            var item = (await ProjectAsync(_context.GroomerPortfolioPosts.AsNoTracking().Where(p => p.Id == postId), null, includeModeration: true)).FirstOrDefault();
            return item == null
                ? Fail<GroomerPortfolioPostVDto>(Resource.Notification.PortfolioPostNotFound)
                : new BaseResultDto<GroomerPortfolioPostVDto>(true, item);
        }

        private async Task<GroomerPortfolioPage<GroomerPortfolioPostVDto>> PageAsync(
            IQueryable<GroomerPortfolioPost> query, GroomerPortfolioPostSearchInputDto input, long? userId, bool includeModeration)
        {
            if (input.GroomerId.HasValue)
                query = query.Where(p => p.GroomerId == input.GroomerId.Value);
            if (input.CityId.HasValue)
                query = query.Where(p => p.Groomer.CityId == input.CityId.Value);
            if (input.PetId.HasValue)
                query = query.Where(p => p.PetId == input.PetId.Value);
            if (input.GroomerServiceId.HasValue)
                query = query.Where(p => p.GroomerServiceId == input.GroomerServiceId.Value);

            query = input.SortBy == Common.Enumerable.SortEnum.Old
                ? query.OrderBy(p => p.Id)
                : query.OrderByDescending(p => p.Id);

            var total = await query.CountAsync();
            var page = query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize);
            return new GroomerPortfolioPage<GroomerPortfolioPostVDto>
            {
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                TotalCount = total,
                List = await ProjectAsync(page, userId, includeModeration),
            };
        }

        private async Task<List<GroomerPortfolioPostVDto>> ProjectAsync(IQueryable<GroomerPortfolioPost> query, long? userId, bool includeModeration)
        {
            var rows = await query.Select(p => new
            {
                p.Id, p.GroomerId, GroomerName = p.Groomer.Name, GroomerIcon = p.Groomer.Icon, p.Caption, p.PetId,
                PetName = p.Pet.Name, p.GroomerServiceId, ServiceName = p.GroomerService.Assistance.Name,
                p.CreateDate, p.LikeCount, p.CommentCount, p.Approved, p.Active, p.ApprovalValue,
                LikedByMe = userId.HasValue && _context.GroomerPortfolioLikes.Any(l => l.PostId == p.Id && l.UserId == userId.Value),
                Pictures = p.Pictures.Where(x => !x.Deleted).OrderBy(x => x.SortOrder)
                    .Select(x => new { x.Id, x.PictureId, x.Kind, x.SortOrder, x.Label, x.Picture }).ToList(),
            }).AsSplitQuery().ToListAsync();

            return rows.Select(r => new GroomerPortfolioPostVDto
            {
                Id = r.Id,
                GroomerId = r.GroomerId,
                GroomerName = r.GroomerName,
                GroomerIcon = r.GroomerIcon == null ? null : _mapper.Map<PictureVDto>(r.GroomerIcon),
                Caption = r.Caption,
                PetId = r.PetId,
                PetName = r.PetName,
                GroomerServiceId = r.GroomerServiceId,
                ServiceName = r.ServiceName,
                CreateDate = r.CreateDate,
                LikeCount = r.LikeCount,
                CommentCount = r.CommentCount,
                LikedByMe = r.LikedByMe,
                // وضعیت تأیید/دلیل رد فقط در لیست‌های مدیریتی؛ در فید عمومی همیشه فقط پست‌های تأییدشده‌اند.
                Approved = includeModeration ? r.Approved : true,
                Active = includeModeration ? r.Active : true,
                ApprovalValue = includeModeration ? r.ApprovalValue : null,
                Pictures = r.Pictures.Select(x => new GroomerPortfolioPictureVDto
                {
                    Id = x.Id,
                    PictureId = x.PictureId,
                    Kind = x.Kind,
                    SortOrder = x.SortOrder,
                    Label = x.Label,
                    Picture = x.Picture == null ? null : _mapper.Map<PictureVDto>(x.Picture),
                }).ToList(),
            }).ToList();
        }

        private async Task<BaseResultDto<GroomerPortfolioLikeResultDto>> RefreshLikeResultAsync(long postId, bool liked)
        {
            var count = await _context.GroomerPortfolioLikes.CountAsync(l => l.PostId == postId);
            await _context.GroomerPortfolioPosts.Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, count));
            var groomerId = await _context.GroomerPortfolioPosts.AsNoTracking().Where(p => p.Id == postId).Select(p => p.GroomerId).FirstOrDefaultAsync();
            if (groomerId > 0)
                await RefreshGroomerCountersAsync(groomerId);
            return new BaseResultDto<GroomerPortfolioLikeResultDto>(true, new GroomerPortfolioLikeResultDto { Liked = liked, LikeCount = count });
        }

        private async Task RefreshCommentCountAsync(long postId)
        {
            var accept = (long)CommentEnum.Comment_Accept;
            var count = await _context.GroomerPortfolioComments.CountAsync(c => c.PostId == postId && !c.Deleted && c.StatusId == accept);
            await _context.GroomerPortfolioPosts.Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, count));
        }

        /// <summary>شمارنده‌های سریع آرایشگاه (تعداد نمونه‌کار و مجموع لایک پست‌های قابل‌نمایش) را از روی داده‌ی واقعی دوباره می‌سازد.</summary>
        private async Task RefreshGroomerCountersAsync(long groomerId)
        {
            var visible = _context.GroomerPortfolioPosts.AsNoTracking().Where(p => p.GroomerId == groomerId && !p.Deleted && p.Approved && p.Active);
            var postCount = await visible.CountAsync();
            var likeCount = await visible.SumAsync(p => (int?)p.LikeCount) ?? 0;
            await _context.Groomers.Where(g => g.Id == groomerId)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.PortfolioCount, postCount).SetProperty(g => g.PortfolioLikeCount, likeCount));
        }

        private static string DisplayName(string firstName, string lastName)
        {
            var name = $"{firstName} {lastName}".Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }

        private static BaseResultDto<T> Fail<T>(string message) => new BaseResultDto<T>(false, message, default);
    }
}
