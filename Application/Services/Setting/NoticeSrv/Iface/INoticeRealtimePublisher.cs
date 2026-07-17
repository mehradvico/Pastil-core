using Application.Services.Setting.NoticeSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv.Iface
{
    public interface INoticeRealtimePublisher
    {
        Task PublishAsync(NoticeVDto notice);
    }
}
