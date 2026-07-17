using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv
{
    public class NullNoticeRealtimePublisher : INoticeRealtimePublisher
    {
        public Task PublishAsync(NoticeVDto notice) => Task.CompletedTask;
    }
}
