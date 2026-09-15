using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CommonSrv.PushInboxSrv.Dto;
using Application.Services.CommonSrv.PushInboxSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushInboxSrv
{
    public class PushInboxService : IPushInboxService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;

        public PushInboxService(IDataBaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // پایه‌ی مشترک همه‌ی کوئری‌های Inbox.
        //
        // فیلتر Title != null عمدی است: ردیف PushNotification در لحظه‌ی ساخت فقط
        // Tokenها را دارد و Title/Body بعداً هنگام Dispatch از روی PushPattern
        // ساخته می‌شود. بدون این فیلتر، اعلان‌های زمان‌بندی‌شده‌ی آینده و ردیف‌هایی که
        // قبل از ساخت متن Fail شده‌اند به‌صورت آیتم خالی در Inbox دیده می‌شدند.
        //
        // توجه: عدم موفقیت تحویل (SentDate == null) آیتم را حذف نمی‌کند؛ کل هدف
        // Inbox همین است که کاربر اعلان تحویل‌نشده را هم ببیند.
        private IQueryable<PushNotification> InboxQuery(long userId) =>
            _context.PushNotifications
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Title != null);

        public async Task<NotificationInboxSearchDto> SearchAsync(long userId, NotificationInboxInputDto dto)
        {
            var model = InboxQuery(userId)
                .Include(x => x.PushPattern)
                    .ThenInclude(p => p.PushType);

            var filtered = dto.UnreadOnly
                ? model.Where(x => x.ReadDateUtc == null)
                : (IQueryable<PushNotification>)model;

            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                filtered = filtered.Where(x => x.Title.Contains(q) || x.Body.Contains(q));
            }

            filtered = dto.SortBy == SortEnum.Old
                ? filtered.OrderBy(x => x.CreateDate).ThenBy(x => x.Id)
                : filtered.OrderByDescending(x => x.CreateDate).ThenByDescending(x => x.Id);

            var result = new NotificationInboxSearchDto(dto, filtered, _mapper);
            result.UnreadCount = await GetUnreadCountAsync(userId);
            return result;
        }

        public Task<int> GetUnreadCountAsync(long userId) =>
            InboxQuery(userId).CountAsync(x => x.ReadDateUtc == null);

        public async Task<BaseResultDto> MarkReadAsync(long userId, long notificationId)
        {
            // AsTracking اجباری است: DataBaseContext سراسری NoTracking است و بدون آن
            // مقداردهی ReadDateUtc بی‌صدا ذخیره نمی‌شود.
            var item = await _context.PushNotifications
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);

            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            // Idempotent: خواندن دوباره‌ی یک اعلان خوانده‌شده خطا نیست و زمان اولیه را عوض نمی‌کند.
            if (item.ReadDateUtc == null)
            {
                item.ReadDateUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> MarkAllReadAsync(long userId)
        {
            var now = DateTime.UtcNow;

            var items = await _context.PushNotifications
                .AsTracking()
                .Where(x => x.UserId == userId && x.Title != null && x.ReadDateUtc == null)
                .ToListAsync();

            if (items.Count == 0)
                return new BaseResultDto(true);

            foreach (var item in items)
                item.ReadDateUtc = now;

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
    }
}
