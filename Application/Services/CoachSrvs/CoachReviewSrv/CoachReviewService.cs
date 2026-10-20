using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.CoachSrvs.CoachReserveSrv;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReviewSrv
{
    public class CoachReviewInputDto
    {
        public long CoachReserveId { get; set; }
        /// <summary>۱ تا ۵</summary>
        public int Rate { get; set; }
        public string Text { get; set; }
    }

    public class CoachReviewVDto
    {
        public long Id { get; set; }
        public long CoachId { get; set; }
        public string CoachName { get; set; }
        public long? CoachReserveId { get; set; }
        public int? Rate { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>۱ منتظر بررسی، ۲ تأییدشده، ۳ ردشده (CommentEnum)</summary>
        public long StatusId { get; set; }
    }

    public class CoachReviewSearchInputDto : BaseInputDto
    {
        public long? CoachId { get; set; }
        public long? StatusId { get; set; }
        public long? OwnerUserId { get; set; }
    }

    public class CoachReviewPage
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<CoachReviewVDto> List { get; set; } = new List<CoachReviewVDto>();
    }

    public interface ICoachReviewService
    {
        /// <summary>فقط کاربرِ رزرو، فقط بعد از «تکمیل»، یک‌بار برای هر رزرو؛ نظر تا تأیید ادمین نمایش داده نمی‌شود.</summary>
        Task<BaseResultDto<CoachReviewVDto>> CreateAsync(CoachReviewInputDto dto, long bookerId);
        /// <summary>فقط نظرهای تأییدشده (عمومی).</summary>
        Task<CoachReviewPage> SearchPublicAsync(CoachReviewSearchInputDto input);
        Task<CoachReviewPage> SearchManageAsync(CoachReviewSearchInputDto input);
        /// <summary>تأیید/رد نظر توسط ادمین؛ امتیاز و تعداد نظر مربی دوباره محاسبه می‌شود.</summary>
        Task<BaseResultDto> ModerateAsync(long commentId, bool accept);
        /// <summary>پاسخ مربی (مالک) یا ادمین به یک نظر.</summary>
        Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId);
    }

    public class CoachReviewService : ICoachReviewService
    {
        private const int MaxTextLength = 1000;

        private readonly IDataBaseContext _context;
        private readonly ICoachCoreService _coachs;
        private readonly ILogger<CoachReviewService> _logger;

        public CoachReviewService(IDataBaseContext context, ICoachCoreService coaches, ILogger<CoachReviewService> logger)
        {
            _context = context;
            _coachs = coaches;
            _logger = logger;
        }

        public async Task<BaseResultDto<CoachReviewVDto>> CreateAsync(CoachReviewInputDto dto, long bookerId)
        {
            try
            {
                if (dto == null || dto.CoachReserveId <= 0)
                    return Fail(Resource.Notification.InvalidData);
                if (dto.Rate < 1 || dto.Rate > 5)
                    return Fail(Resource.Notification.CoachReviewRateInvalid);
                var text = dto.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return Fail(Resource.Notification.CoachCommentTextRequired);
                if (text.Length > MaxTextLength)
                    return Fail(Resource.Notification.PortfolioCommentTooLong);

                var complete = (int)CoachReserveStatusEnum.Complete;
                var reserve = await _context.CoachReserves.AsNoTracking()
                    .Where(r => r.Id == dto.CoachReserveId && r.BookerId == bookerId && r.IsReserved && !r.IsCancel && r.Status == complete)
                    .Select(r => new { r.Id, r.CoachId })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return Fail(Resource.Notification.CoachReviewNotAllowed);
                if (await _context.CoachComments.AnyAsync(c => c.CoachReserveId == reserve.Id))
                    return Fail(Resource.Notification.CoachReviewAlreadyExists);

                var comment = new CoachComment
                {
                    CoachId = reserve.CoachId,
                    CoachReserveId = reserve.Id,
                    IsReserved = true,
                    UserId = bookerId,
                    Rate = dto.Rate,
                    Text = text,
                    StatusId = (long)CommentEnum.Comment_NotChecked,
                    CreateDate = DateTime.Now,
                };
                await _context.CoachComments.AddAsync(comment);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // ثبت هم‌زمان دو نظر برای یک رزرو (ایندکس یکتا)
                    return Fail(Resource.Notification.CoachReviewAlreadyExists);
                }
                return await FindAsync(comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating coach review failed for reserve {ReserveId}.", dto?.CoachReserveId);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        public Task<CoachReviewPage> SearchPublicAsync(CoachReviewSearchInputDto input)
        {
            input.StatusId = (long)CommentEnum.Comment_Accept;
            input.OwnerUserId = null;
            return SearchAsync(input, publicOnly: true);
        }

        public Task<CoachReviewPage> SearchManageAsync(CoachReviewSearchInputDto input) => SearchAsync(input, publicOnly: false);

        public async Task<BaseResultDto> ModerateAsync(long commentId, bool accept)
        {
            var comment = await _context.CoachComments.AsTracking().FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            comment.StatusId = (long)(accept ? CommentEnum.Comment_Accept : CommentEnum.Comment_Reject);
            await _context.SaveChangesAsync();
            await _coachs.UpdateCommentCountAsync(comment.CoachId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId)
        {
            answer = answer?.Trim();
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > MaxTextLength)
                return new BaseResultDto(false, Resource.Notification.CoachCommentTextRequired);
            var comment = await _context.CoachComments.AsTracking()
                .Include(c => c.Coach)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && comment.Coach.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            comment.Answer = answer;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<CoachReviewPage> SearchAsync(CoachReviewSearchInputDto input, bool publicOnly)
        {
            var query = _context.CoachComments.AsNoTracking().Where(c => !c.Coach.Deleted);
            if (publicOnly)
                query = query.Where(c => c.Coach.Active && c.Coach.Approved);
            if (input.CoachId.HasValue)
                query = query.Where(c => c.CoachId == input.CoachId.Value);
            if (input.StatusId.HasValue)
                query = query.Where(c => c.StatusId == input.StatusId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(c => c.Coach.OwnerId == input.OwnerUserId.Value);

            query = query.OrderByDescending(c => c.Id);
            var total = await query.CountAsync();
            var rows = await Project(query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)).ToListAsync();
            return new CoachReviewPage { PageIndex = input.PageIndex, PageSize = input.PageSize, TotalCount = total, List = rows };
        }

        private async Task<BaseResultDto<CoachReviewVDto>> FindAsync(long id)
        {
            var item = await Project(_context.CoachComments.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();
            return item == null ? Fail(Resource.Notification.NothingFound) : new BaseResultDto<CoachReviewVDto>(true, item);
        }

        private static IQueryable<CoachReviewVDto> Project(IQueryable<CoachComment> query) =>
            query.Select(c => new CoachReviewVDto
            {
                Id = c.Id,
                CoachId = c.CoachId,
                CoachName = c.Coach.Name,
                CoachReserveId = c.CoachReserveId,
                Rate = c.Rate,
                Text = c.Text,
                Answer = c.Answer,
                CreateDate = c.CreateDate,
                UserId = c.UserId,
                UserName = c.User == null ? null : (c.User.FirstName + " " + c.User.LastName).Trim(),
                StatusId = c.StatusId,
            });

        private static BaseResultDto<CoachReviewVDto> Fail(string message) => new BaseResultDto<CoachReviewVDto>(false, message, null);
    }
}
