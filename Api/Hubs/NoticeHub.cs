using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class NoticeHub : Hub
    {
        public const string AdminGroup = "NoticeAdmins";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnConnectedAsync();
        }
    }
}
