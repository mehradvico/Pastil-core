using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface
{
    public interface ILiveKitService
    {
        /// <summary>توکن JWT ورود به اتاق - مربی: canPublish=true/canSubscribe=false، بیننده: عکسش.</summary>
        string GenerateAccessToken(string roomName, string identity, string displayName, bool canPublish, bool canSubscribe, int ttlMinutes = 180);

        Task CreateRoomAsync(string roomName);
        Task DeleteRoomAsync(string roomName);

        /// <summary>ضبط ترکیبیِ اتاق را شروع می‌کند (فقط ویدیوی مربی، چون بیننده‌ها publish نمی‌کنند) و EgressId برمی‌گرداند.</summary>
        Task<string> StartRecordingAsync(string roomName);
        Task StopRecordingAsync(string egressId);
    }
}
