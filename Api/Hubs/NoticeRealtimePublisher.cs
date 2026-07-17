using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class NoticeRealtimePublisher : INoticeRealtimePublisher
    {
        private readonly IHubContext<NoticeHub> _hubContext;

        public NoticeRealtimePublisher(IHubContext<NoticeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishAsync(NoticeVDto notice)
        {
            return _hubContext.Clients.Group(NoticeHub.AdminGroup).SendAsync("noticeCreated", notice);
        }
    }
}
