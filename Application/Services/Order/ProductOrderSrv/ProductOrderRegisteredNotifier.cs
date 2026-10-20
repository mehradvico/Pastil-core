using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderSrv
{
    // همه‌ی مقدارها از قبل خوانده شده‌اند تا این کار بعد از Commit و در scope جدا (بدون موجودیت‌های tracked درخواست) اجرا شود.
    public sealed class ProductOrderRegisteredNotification
    {
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string OrderId { get; set; }
        public string OrderCode { get; set; }
        public string StoreNamesText { get; set; }
        public List<long> StoreIds { get; set; } = new();
        public List<string> StoreMobiles { get; set; } = new();
        public string NameToken { get; set; }
    }

    public interface IProductOrderRegisteredNotifier
    {
        Task NotifyAsync(ProductOrderRegisteredNotification data);
    }

    // پیامک ثبت سفارش (کاربر و فروشگاه‌ها)، پوش فروشگاه/ادمین و اعلان ادمین. هر مرحله مستقل و best-effort است.
    public sealed class ProductOrderRegisteredNotifier : IProductOrderRegisteredNotifier
    {
        private readonly IDataBaseContext _context;
        private readonly IMessageSenderService _messageSender;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INoticeService _noticeService;
        private readonly ILogger<ProductOrderRegisteredNotifier> _logger;

        public ProductOrderRegisteredNotifier(
            IDataBaseContext context,
            IMessageSenderService messageSender,
            IPushNotificationService pushNotificationService,
            INoticeService noticeService,
            ILogger<ProductOrderRegisteredNotifier> logger)
        {
            _context = context;
            _messageSender = messageSender;
            _pushNotificationService = pushNotificationService;
            _noticeService = noticeService;
            _logger = logger;
        }

        public async Task NotifyAsync(ProductOrderRegisteredNotification data)
        {
            await StepAsync("user sms", () => _messageSender.SendMessageAsync(
                messageType: MessageTypeEnum.UserRegisterOrder, mobileReceptor: data.UserMobile, emailReceptor: null,
                token1: data.NameToken, token2: data.OrderCode, token3: data.OrderId, token4: data.StoreNamesText), data);

            await StepAsync("pushes", () => SendPushesAsync(data), data);

            await StepAsync("admin notice", () => _noticeService.CreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.ProductOrderRegistered,
                ActorUserId = data.UserId,
                ReferenceType = "ProductOrder",
                ReferenceId = long.Parse(data.OrderId),
                DeduplicationKey = $"{NoticeTypeLabels.ProductOrderRegistered}:{data.OrderId}",
                Metadata = new Dictionary<string, string>
                {
                    { "userName", data.UserFullName }, { "orderId", data.OrderId }, { "mobile", data.UserMobile }
                }
            }), data);

            foreach (var mobile in data.StoreMobiles)
            {
                await StepAsync("store sms", () => _messageSender.SendMessageAsync(
                    messageType: MessageTypeEnum.StoreRegisterOrder, mobileReceptor: mobile, emailReceptor: null,
                    token1: data.NameToken, token2: data.OrderCode), data);
            }
        }

        private async Task StepAsync(string step, Func<Task> action, ProductOrderRegisteredNotification data)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order {OrderCode} registration notification step '{Step}' failed.", data.OrderCode, step);
            }
            finally
            {
                if (watch.ElapsedMilliseconds > 3000)
                    _logger.LogWarning("Order {OrderCode} notification step '{Step}' took {Ms} ms.", data.OrderCode, step, watch.ElapsedMilliseconds);
            }
        }

        // پوش ثبت سفارش فقط برای فروشگاه (کاربران همان فروشگاه‌ها) و ادمین؛ برای مشتری پوش نمی‌رود.
        private async Task SendPushesAsync(ProductOrderRegisteredNotification data)
        {
            var storeUserIds = await _context.Stores.AsNoTracking()
                .Where(store => data.StoreIds.Contains(store.Id))
                .SelectMany(store => store.Users)
                .Where(user => !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .Distinct()
                .ToListAsync();
            var adminUserIds = await _context.Users.AsNoTracking()
                .Where(user => user.RoleId == (long)RoleEnum.Admin && !user.Deleted && !user.Locked)
                .Select(user => user.Id)
                .ToListAsync();

            foreach (var userId in storeUserIds)
                await PushSafeAsync(PushTypeEnum.PushRegisterOrderStore, userId, data);
            foreach (var userId in adminUserIds.Except(storeUserIds))
                await PushSafeAsync(PushTypeEnum.PushRegisterOrderAdmin, userId, data);
        }

        private async Task PushSafeAsync(PushTypeEnum pushType, long userId, ProductOrderRegisteredNotification data)
        {
            try
            {
                await _pushNotificationService.SendPushAsync(pushType, userId, token1: data.NameToken, token2: data.OrderCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Order push {PushType} to user {UserId} failed.", pushType, userId);
            }
        }
    }
}
