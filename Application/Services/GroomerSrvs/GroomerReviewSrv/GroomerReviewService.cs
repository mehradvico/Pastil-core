using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.GroomerSrvs.GroomerReserveSrv;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerReviewSrv
{
    public class GroomerReviewInputDto
    {
        public long GroomerReserveId { get; set; }
        /// <summary>۱ تا ۵</summary>
        public int Rate { get; set; }
        public string Text { get; set; }
    }

    public class GroomerReviewVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public string GroomerName { get; set; }
        public long? GroomerReserveId { get; set; }
        public int? Rate { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>۱ منتظر بررسی، ۲ تأییدشده، ۳ ردشده (CommentEnum)</summary>
        public long StatusId { get; set; }
    }

    public class GroomerReviewSearchInputDto : BaseInputDto
    {
        public long? GroomerId { get; set; }
        public long? StatusId { get; set; }
        public long? OwnerUserId { get; set; }
    }

    public class GroomerReviewPage
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<GroomerReviewVDto> List { get; set; } = new List<GroomerReviewVDto>();
    }

    public interface IGroomerReviewService
    {
        /// <summary>فقط کاربرِ رزرو، فقط بعد از «تکمیل»، یک‌بار برای هر رزرو؛ نظر تا تأیید ادمین نمایش داده نمی‌شود.</summary>
        Task<BaseResultDto<GroomerReviewVDto>> CreateAsync(GroomerReviewInputDto dto, long bookerId);
        /// <summary>فقط نظرهای تأییدشده (عمومی).</summary>
        Task<GroomerReviewPage> SearchPublicAsync(GroomerReviewSearchInputDto input);
        Task<GroomerReviewPage> SearchManageAsync(GroomerReviewSearchInputDto input);
        /// <summary>تأیید/رد نظر توسط ادمین؛ امتیاز و تعداد نظر آرایشگاه دوباره محاسبه می‌شود.</summary>
        Task<BaseResultDto> ModerateAsync(long commentId, bool accept);
        /// <summary>پاسخ آرایشگاه (مالک) یا ادمین به یک نظر.</summary>
        Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId);
    }

    public class GroomerReviewService : IGroomerReviewService
    {
        private const int MaxTextLength = 1000;

        private readonly IDataBaseContext _context;
        private readonly IGroomerCoreService _groomers;
        private readonly ILogger<GroomerReviewService> _logger;

        public GroomerReviewService(IDataBaseContext context, IGroomerCoreService groomers, ILogger<GroomerReviewService> logger)
        {
            _context = context;
            _groomers = groomers;
            _logger = logger;
        }

        public async Task<BaseResultDto<GroomerReviewVDto>> CreateAsync(GroomerReviewInputDto dto, long bookerId)
        {
            try
            {
                if (dto == null || dto.GroomerReserveId <= 0)
                    return Fail(Resource.Notification.InvalidData);
                if (dto.Rate < 1 || dto.Rate > 5)
                    return Fail(Resource.Notification.GroomerReviewRateInvalid);
                var text = dto.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return Fail(Resource.Notification.GroomerCommentTextRequired);
                if (text.Length > MaxTextLength)
                    return Fail(Resource.Notification.PortfolioCommentTooLong);

                var complete = (int)GroomerReserveStatusEnum.Complete;
                var reserve = await _context.GroomerReserves.AsNoTracking()
                    .Where(r => r.Id == dto.GroomerReserveId && r.BookerId == bookerId && r.IsReserved && !r.IsCancel && r.Status == complete)
                    .Select(r => new { r.Id, r.GroomerId })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return Fail(Resource.Notification.GroomerReviewNotAllowed);
                if (await _context.GroomerComments.AnyAsync(c => c.GroomerReserveId == reserve.Id))
                    return Fail(Resource.Notification.GroomerReviewAlreadyExists);

                var comment = new GroomerComment
                {
                    GroomerId = reserve.GroomerId,
                    GroomerReserveId = reserve.Id,
                    IsReserved = true,
                    UserId = bookerId,
                    Rate = dto.Rate,
                    Text = text,
                    StatusId = (long)CommentEnum.Comment_NotChecked,
                    CreateDate = DateTime.Now,
                };
                await _context.GroomerComments.AddAsync(comment);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // ثبت هم‌زمان دو نظر برای یک رزرو (ایندکس یکتا)
                    return Fail(Resource.Notification.GroomerReviewAlreadyExists);
                }
                return await FindAsync(comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating groomer review failed for reserve {ReserveId}.", dto?.GroomerReserveId);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        public Task<GroomerReviewPage> SearchPublicAsync(GroomerReviewSearchInputDto input)
        {
            input.StatusId = (long)CommentEnum.Comment_Accept;
            input.OwnerUserId = null;
            return SearchAsync(input, publicOnly: true);
        }

        public Task<GroomerReviewPage> SearchManageAsync(GroomerReviewSearchInputDto input) => SearchAsync(input, publicOnly: false);

        public async Task<BaseResultDto> ModerateAsync(long commentId, bool accept)
        {
            var comment = await _context.GroomerComments.AsTracking().FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            comment.StatusId = (long)(accept ? CommentEnum.Comment_Accept : CommentEnum.Comment_Reject);
            await _context.SaveChangesAsync();
            await _groomers.UpdateCommentCountAsync(comment.GroomerId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId)
        {
            answer = answer?.Trim();
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > MaxTextLength)
                return new BaseResultDto(false, Resource.Notification.GroomerCommentTextRequired);
            var comment = await _context.GroomerComments.AsTracking()
                .Include(c => c.Groomer)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && comment.Groomer.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            comment.Answer = answer;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<GroomerReviewPage> SearchAsync(GroomerReviewSearchInputDto input, bool publicOnly)
        {
            var query = _context.GroomerComments.AsNoTracking().Where(c => !c.Groomer.Deleted);
            if (publicOnly)
                query = query.Where(c => c.Groomer.Active && c.Groomer.Approved);
            if (input.GroomerId.HasValue)
                query = query.Where(c => c.GroomerId == input.GroomerId.Value);
            if (input.StatusId.HasValue)
                query = query.Where(c => c.StatusId == input.StatusId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(c => c.Groomer.OwnerId == input.OwnerUserId.Value);

            query = query.OrderByDescending(c => c.Id);
            var total = await query.CountAsync();
            var rows = await Project(query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)).ToListAsync();
            return new GroomerReviewPage { PageIndex = input.PageIndex, PageSize = input.PageSize, TotalCount = total, List = rows };
        }

        private async Task<BaseResultDto<GroomerReviewVDto>> FindAsync(long id)
        {
            var item = await Project(_context.GroomerComments.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();
            return item == null ? Fail(Resource.Notification.NothingFound) : new BaseResultDto<GroomerReviewVDto>(true, item);
        }

        private static IQueryable<GroomerReviewVDto> Project(IQueryable<GroomerComment> query) =>
            query.Select(c => new GroomerReviewVDto
            {
                Id = c.Id,
                GroomerId = c.GroomerId,
                GroomerName = c.Groomer.Name,
                GroomerReserveId = c.GroomerReserveId,
                Rate = c.Rate,
                Text = c.Text,
                Answer = c.Answer,
                CreateDate = c.CreateDate,
                UserId = c.UserId,
                UserName = c.User == null ? null : (c.User.FirstName + " " + c.User.LastName).Trim(),
                StatusId = c.StatusId,
            });

        private static BaseResultDto<GroomerReviewVDto> Fail(string message) => new BaseResultDto<GroomerReviewVDto>(false, message, null);
    }
}
