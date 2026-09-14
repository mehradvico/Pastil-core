using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    /// <summary>
    /// کانال زنده‌ی پوش برای اپ فلاتر ویندوز — هر کاربر لاگین‌کرده به گروه اختصاصی خودش اضافه می‌شود
    /// تا PushNotificationService بتواند مستقیم برایش پیام بفرستد، بدون نیاز به توکن ذخیره‌شده.
    /// </summary>
    [Authorize]
    public class PushHub : Hub
    {
        public static string GroupName(long userId) => $"push-user-{userId}";

        private long? CurrentUserId
        {
            get
            {
                var claim = Context.User?.FindFirst("UserId")?.Value;
                return long.TryParse(claim, out var id) ? id : null;
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = CurrentUserId;
            if (userId.HasValue)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(userId.Value));

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = CurrentUserId;
            if (userId.HasValue)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(userId.Value));

            await base.OnDisconnectedAsync(exception);
        }
    }
}
