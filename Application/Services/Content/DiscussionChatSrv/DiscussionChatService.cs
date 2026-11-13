using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionChatSrv.Dto;
using Application.Services.Content.DiscussionChatSrv.Iface;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionChatSrv
{
    /// <summary>
    /// پرسش و پاسخ کالا به‌صورت چت‌روم: هر کاربر لاگین‌شده (مشتری، پرسنل فروشگاه) می‌تواند به پرسش یا به هر پاسخِ تأییدشده جواب بدهد؛
    /// هر پیام تا تأیید ادمین نمایش داده نمی‌شود و تا آن موقع نویسنده نمی‌تواند پیام جدید بفرستد (ولی می‌تواند ویرایش/حذفش کند).
    /// </summary>
    public class DiscussionChatService : IDiscussionChatService
    {
        public const int MinContent = 3;
        public const int MaxContent = 1000;
        /// <summary>پیام ردشده تا این‌قدر روز در «وضعیت پیام‌های من» نشان داده می‌شود.</summary>
        public const int RejectedVisibleDays = 7;

        private readonly IDataBaseContext _context;

        public DiscussionChatService(IDataBaseContext context) => _context = context;

        // ───────────── ثبت پرسش ─────────────
        public async Task<BaseResultDto<DiscussionQuestionDto>> SubmitQuestionAsync(long userId, DiscussionQuestionDto dto)
        {
            if (dto == null || dto.ProductId <= 0)
                return new BaseResultDto<DiscussionQuestionDto>(false, Resource.Notification.DiscussionProductInvalid, dto);
            var content = Clean(dto.Content);
            if (content == null)
                return new BaseResultDto<DiscussionQuestionDto>(false, Resource.Notification.DiscussionContentInvalid, dto);
            var productOk = await _context.Products.AsNoTracking().AnyAsync(p => p.Id == dto.ProductId && !p.Deleted && p.Active);
            if (!productOk)
                return new BaseResultDto<DiscussionQuestionDto>(false, Resource.Notification.DiscussionProductInvalid, dto);
            if (await HasPendingAsync(userId))
                return new BaseResultDto<DiscussionQuestionDto>(false, Resource.Notification.DiscussionPendingExists, dto);

            var item = new DiscussionQuestion
            {
                Content = content,
                ProductId = dto.ProductId,
                UserId = userId,
                CreateDate = DateTime.Now,
                AdminConfirm = null,   // در انتظار تأیید ادمین؛ هرچه کلاینت فرستاده نادیده گرفته می‌شود
                Active = false,
                AnswerCount = 0,
            };
            await _context.DiscussionQuestions.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<DiscussionQuestionDto>(true, new DiscussionQuestionDto
            {
                Id = item.Id, Content = item.Content, ProductId = item.ProductId, UserId = item.UserId, CreateDate = item.CreateDate,
                AdminConfirm = item.AdminConfirm, Active = item.Active,
            });
        }

        // ───────────── ثبت پاسخ (به پرسش یا به پاسخی دیگر) ─────────────
        public async Task<BaseResultDto<DiscussionAnswerDto>> SubmitAnswerAsync(long userId, DiscussionAnswerDto dto)
        {
            if (dto == null || dto.DiscussionQuestionId <= 0)
                return new BaseResultDto<DiscussionAnswerDto>(false, Resource.Notification.DiscussionQuestionNotAvailable, dto);
            var content = Clean(dto.Content);
            if (content == null)
                return new BaseResultDto<DiscussionAnswerDto>(false, Resource.Notification.DiscussionContentInvalid, dto);

            var question = await _context.DiscussionQuestions.AsNoTracking()
                .Where(q => q.Id == dto.DiscussionQuestionId && !q.Deleted && q.Active && q.AdminConfirm == true)
                .Select(q => new { q.Id, q.ProductId }).FirstOrDefaultAsync();
            if (question == null)
                return new BaseResultDto<DiscussionAnswerDto>(false, Resource.Notification.DiscussionQuestionNotAvailable, dto);

            long? replyTo = dto.ReplyToAnswerId is > 0 ? dto.ReplyToAnswerId : null;
            if (replyTo.HasValue)
            {
                var targetOk = await _context.DiscussionAnswers.AsNoTracking().AnyAsync(a => a.Id == replyTo.Value
                    && a.DiscussionQuestionId == question.Id && !a.Deleted && a.Active && a.AdminConfirm == true);
                if (!targetOk)
                    return new BaseResultDto<DiscussionAnswerDto>(false, Resource.Notification.DiscussionReplyTargetInvalid, dto);
            }
            if (await HasPendingAsync(userId))
                return new BaseResultDto<DiscussionAnswerDto>(false, Resource.Notification.DiscussionPendingExists, dto);

            var item = new DiscussionAnswer
            {
                Content = content,
                DiscussionQuestionId = question.Id,
                UserId = userId,
                CreateDate = DateTime.Now,
                Active = false,
                AdminConfirm = null,   // در انتظار تأیید ادمین
                ReplyToAnswerId = replyTo,
                StoreId = await StoreOfAsync(userId, question.ProductId),
            };
            await _context.DiscussionAnswers.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<DiscussionAnswerDto>(true, new DiscussionAnswerDto
            {
                Id = item.Id, Content = item.Content, DiscussionQuestionId = item.DiscussionQuestionId, UserId = item.UserId,
                Active = item.Active, ReplyToAnswerId = item.ReplyToAnswerId,
            });
        }

        // ───────────── ویرایش / حذف پیامِ در انتظار ─────────────
        public async Task<BaseResultDto> EditAsync(long userId, DiscussionChatEditDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            var content = Clean(dto.Content);
            if (content == null)
                return new BaseResultDto(false, Resource.Notification.DiscussionContentInvalid);
            if (dto.Kind == DiscussionChatKind.Question)
            {
                var item = await _context.DiscussionQuestions.AsTracking().FirstOrDefaultAsync(q => q.Id == dto.Id && !q.Deleted);
                var denied = Guard(item?.UserId, item?.AdminConfirm, userId);
                if (denied != null) return denied;
                item.Content = content;
            }
            else
            {
                var item = await _context.DiscussionAnswers.AsTracking().FirstOrDefaultAsync(a => a.Id == dto.Id && !a.Deleted);
                var denied = Guard(item?.UserId, item?.AdminConfirm, userId);
                if (denied != null) return denied;
                item.Content = content;
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> DeleteAsync(long userId, DiscussionChatKind kind, long id)
        {
            if (id <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            if (kind == DiscussionChatKind.Question)
            {
                var item = await _context.DiscussionQuestions.AsTracking().FirstOrDefaultAsync(q => q.Id == id && !q.Deleted);
                var denied = Guard(item?.UserId, item?.AdminConfirm, userId);
                if (denied != null) return denied;
                item.Deleted = true;
            }
            else
            {
                var item = await _context.DiscussionAnswers.AsTracking().FirstOrDefaultAsync(a => a.Id == id && !a.Deleted);
                var denied = Guard(item?.UserId, item?.AdminConfirm, userId);
                if (denied != null) return denied;
                item.Deleted = true;
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ───────────── «پیام‌های من» ─────────────
        public async Task<DiscussionMyStatusVDto> MyStatusAsync(long userId, long productId)
        {
            var since = DateTime.Now.AddDays(-RejectedVisibleDays);
            var questions = await _context.DiscussionQuestions.AsNoTracking()
                .Where(q => q.UserId == userId && !q.Deleted && q.ProductId == productId && (q.AdminConfirm == null || (q.AdminConfirm == false && q.CreateDate >= since)))
                .OrderByDescending(q => q.Id)
                .Select(q => new DiscussionMyItemVDto { Kind = DiscussionChatKind.Question, Id = q.Id, Content = q.Content, CreateDate = q.CreateDate, Status = q.AdminConfirm == null ? 0 : 2, DiscussionQuestionId = q.Id })
                .ToListAsync();
            var answers = await _context.DiscussionAnswers.AsNoTracking()
                .Where(a => a.UserId == userId && !a.Deleted && a.DiscussionQuestion.ProductId == productId && (a.AdminConfirm == null || (a.AdminConfirm == false && a.CreateDate >= since)))
                .OrderByDescending(a => a.Id)
                .Select(a => new DiscussionMyItemVDto { Kind = DiscussionChatKind.Answer, Id = a.Id, Content = a.Content, CreateDate = a.CreateDate, Status = a.AdminConfirm == null ? 0 : 2, DiscussionQuestionId = a.DiscussionQuestionId, ReplyToAnswerId = a.ReplyToAnswerId })
                .ToListAsync();
            return new DiscussionMyStatusVDto
            {
                HasPending = await HasPendingAsync(userId),
                Items = questions.Concat(answers).OrderByDescending(x => x.CreateDate).ToList(),
            };
        }

        // ───────────── کمکی‌ها ─────────────

        /// <summary>کاربر پیامی (پرسش یا پاسخ) دارد که هنوز ادمین بررسی نکرده؟ (کل کالاها)</summary>
        private async Task<bool> HasPendingAsync(long userId) =>
            await _context.DiscussionQuestions.AsNoTracking().AnyAsync(q => q.UserId == userId && !q.Deleted && q.AdminConfirm == null)
            || await _context.DiscussionAnswers.AsNoTracking().AnyAsync(a => a.UserId == userId && !a.Deleted && a.AdminConfirm == null);

        /// <summary>اگر نویسنده از پرسنل فروشگاهی باشد که این کالا را می‌فروشد، همان فروشگاه (برای نشان «پاسخ فروشگاه»).</summary>
        private async Task<long?> StoreOfAsync(long userId, long productId)
        {
            var fromItems = await _context.ProductItems.AsNoTracking()
                .Where(i => i.ProductId == productId && !i.Deleted && !i.Store.Deleted && i.Store.Users.Any(u => u.Id == userId))
                .Select(i => (long?)i.StoreId).FirstOrDefaultAsync();
            if (fromItems.HasValue) return fromItems;
            return await _context.Products.AsNoTracking()
                .Where(p => p.Id == productId && p.StoreId != null && !p.Stores.Deleted && p.Stores.Users.Any(u => u.Id == userId))
                .Select(p => p.StoreId).FirstOrDefaultAsync();
        }

        private static string Clean(string content)
        {
            var text = content?.Trim();
            return string.IsNullOrEmpty(text) || text.Length < MinContent || text.Length > MaxContent ? null : text;
        }

        // فقط صاحب پیام و فقط تا وقتی ادمین بررسی نکرده
        private static BaseResultDto Guard(long? ownerId, bool? adminConfirm, long userId)
        {
            if (ownerId == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerId.Value != userId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (adminConfirm != null)
                return new BaseResultDto(false, Resource.Notification.DiscussionNotEditable);
            return null;
        }
    }
}
