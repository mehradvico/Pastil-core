using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.ClinicSrvs.ClinicReserveSrv;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReviewSrv
{
    public class ClinicReviewInputDto
    {
        public long ClinicReserveId { get; set; }
        /// <summary>۱ تا ۵</summary>
        public int Rate { get; set; }
        public string Text { get; set; }
    }

    public class ClinicReviewVDto
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public string ClinicName { get; set; }
        public long? ClinicReserveId { get; set; }
        public int? Rate { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>۱ منتظر بررسی، ۲ تأییدشده، ۳ ردشده (CommentEnum)</summary>
        public long StatusId { get; set; }
    }

    public class ClinicReviewSearchInputDto : BaseInputDto
    {
        public long? ClinicId { get; set; }
        public long? StatusId { get; set; }
        public long? OwnerUserId { get; set; }
    }

    public class ClinicReviewPage
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<ClinicReviewVDto> List { get; set; } = new List<ClinicReviewVDto>();
    }

    public interface IClinicReviewService
    {
        /// <summary>فقط کاربرِ رزرو، فقط بعد از «تکمیل»، یک‌بار برای هر رزرو؛ نظر تا تأیید ادمین نمایش داده نمی‌شود.</summary>
        Task<BaseResultDto<ClinicReviewVDto>> CreateAsync(ClinicReviewInputDto dto, long bookerId);
        /// <summary>فقط نظرهای تأییدشده (عمومی).</summary>
        Task<ClinicReviewPage> SearchPublicAsync(ClinicReviewSearchInputDto input);
        Task<ClinicReviewPage> SearchManageAsync(ClinicReviewSearchInputDto input);
        /// <summary>تأیید/رد نظر توسط ادمین؛ امتیاز و تعداد نظر کلینیک دوباره محاسبه می‌شود.</summary>
        Task<BaseResultDto> ModerateAsync(long commentId, bool accept);
        /// <summary>پاسخ کلینیک (مالک) یا ادمین به یک نظر.</summary>
        Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId);
    }

    public class ClinicReviewService : IClinicReviewService
    {
        private const int MaxTextLength = 1000;

        private readonly IDataBaseContext _context;
        private readonly IClinicCoreService _clinics;
        private readonly ILogger<ClinicReviewService> _logger;

        public ClinicReviewService(IDataBaseContext context, IClinicCoreService clinics, ILogger<ClinicReviewService> logger)
        {
            _context = context;
            _clinics = clinics;
            _logger = logger;
        }

        public async Task<BaseResultDto<ClinicReviewVDto>> CreateAsync(ClinicReviewInputDto dto, long bookerId)
        {
            try
            {
                if (dto == null || dto.ClinicReserveId <= 0)
                    return Fail(Resource.Notification.InvalidData);
                if (dto.Rate < 1 || dto.Rate > 5)
                    return Fail(Resource.Notification.ClinicReviewRateInvalid);
                var text = dto.Text?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return Fail(Resource.Notification.ClinicCommentTextRequired);
                if (text.Length > MaxTextLength)
                    return Fail(Resource.Notification.PortfolioCommentTooLong);

                var complete = (int)ClinicReserveStatusEnum.Complete;
                var reserve = await _context.ClinicReserves.AsNoTracking()
                    .Where(r => r.Id == dto.ClinicReserveId && r.BookerId == bookerId && r.IsReserved && !r.IsCancel && r.Status == complete)
                    .Select(r => new { r.Id, r.ClinicId })
                    .FirstOrDefaultAsync();
                if (reserve == null)
                    return Fail(Resource.Notification.ClinicReviewNotAllowed);
                if (await _context.ClinicComments.AnyAsync(c => c.ClinicReserveId == reserve.Id))
                    return Fail(Resource.Notification.ClinicReviewAlreadyExists);

                var comment = new ClinicComment
                {
                    ClinicId = reserve.ClinicId,
                    ClinicReserveId = reserve.Id,
                    IsReserved = true,
                    UserId = bookerId,
                    Rate = dto.Rate,
                    Text = text,
                    StatusId = (long)CommentEnum.Comment_NotChecked,
                    CreateDate = DateTime.Now,
                };
                await _context.ClinicComments.AddAsync(comment);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // ثبت هم‌زمان دو نظر برای یک رزرو (ایندکس یکتا)
                    return Fail(Resource.Notification.ClinicReviewAlreadyExists);
                }
                return await FindAsync(comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Creating clinic review failed for reserve {ReserveId}.", dto?.ClinicReserveId);
                return Fail(Resource.Notification.Unsuccess);
            }
        }

        public Task<ClinicReviewPage> SearchPublicAsync(ClinicReviewSearchInputDto input)
        {
            input.StatusId = (long)CommentEnum.Comment_Accept;
            input.OwnerUserId = null;
            return SearchAsync(input, publicOnly: true);
        }

        public Task<ClinicReviewPage> SearchManageAsync(ClinicReviewSearchInputDto input) => SearchAsync(input, publicOnly: false);

        public async Task<BaseResultDto> ModerateAsync(long commentId, bool accept)
        {
            var comment = await _context.ClinicComments.AsTracking().FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            comment.StatusId = (long)(accept ? CommentEnum.Comment_Accept : CommentEnum.Comment_Reject);
            await _context.SaveChangesAsync();
            await _clinics.UpdateCommentCountAsync(comment.ClinicId);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> AnswerAsync(long commentId, string answer, long? ownerUserId)
        {
            answer = answer?.Trim();
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > MaxTextLength)
                return new BaseResultDto(false, Resource.Notification.ClinicCommentTextRequired);
            var comment = await _context.ClinicComments.AsTracking()
                .Include(c => c.Clinic)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && comment.Clinic.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            comment.Answer = answer;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<ClinicReviewPage> SearchAsync(ClinicReviewSearchInputDto input, bool publicOnly)
        {
            var query = _context.ClinicComments.AsNoTracking().Where(c => !c.Clinic.Deleted);
            if (publicOnly)
                query = query.Where(c => c.Clinic.Active && c.Clinic.Approved);
            if (input.ClinicId.HasValue)
                query = query.Where(c => c.ClinicId == input.ClinicId.Value);
            if (input.StatusId.HasValue)
                query = query.Where(c => c.StatusId == input.StatusId.Value);
            if (input.OwnerUserId.HasValue)
                query = query.Where(c => c.Clinic.OwnerId == input.OwnerUserId.Value);

            query = query.OrderByDescending(c => c.Id);
            var total = await query.CountAsync();
            var rows = await Project(query.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize)).ToListAsync();
            return new ClinicReviewPage { PageIndex = input.PageIndex, PageSize = input.PageSize, TotalCount = total, List = rows };
        }

        private async Task<BaseResultDto<ClinicReviewVDto>> FindAsync(long id)
        {
            var item = await Project(_context.ClinicComments.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();
            return item == null ? Fail(Resource.Notification.NothingFound) : new BaseResultDto<ClinicReviewVDto>(true, item);
        }

        private static IQueryable<ClinicReviewVDto> Project(IQueryable<ClinicComment> query) =>
            query.Select(c => new ClinicReviewVDto
            {
                Id = c.Id,
                ClinicId = c.ClinicId,
                ClinicName = c.Clinic.Name,
                ClinicReserveId = c.ClinicReserveId,
                Rate = c.Rate,
                Text = c.Text,
                Answer = c.Answer,
                CreateDate = c.CreateDate,
                UserId = c.UserId,
                UserName = c.User == null ? null : (c.User.FirstName + " " + c.User.LastName).Trim(),
                StatusId = c.StatusId,
            });

        private static BaseResultDto<ClinicReviewVDto> Fail(string message) => new BaseResultDto<ClinicReviewVDto>(false, message, null);
    }
}
