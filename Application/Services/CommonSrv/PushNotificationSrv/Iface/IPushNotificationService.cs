using Application.Common.Enumerable.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushNotificationSrv.Iface
{
    public interface IPushNotificationService
    {
        Task SendPushAsync(PushTypeEnum pushType, long userId, string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null, DateTime? sendDate = null);
        Task SendNoticeToAdminsAsync(long noticeId, string title, string body, string url);
        Task SendPushGroupAsync(int pageSize = 100);
    }
}
