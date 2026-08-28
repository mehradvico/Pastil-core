using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Iface
{
    public interface IPushScheduleService
    {
        Task DispatchDueAsync(CancellationToken cancellationToken = default);
    }
}
