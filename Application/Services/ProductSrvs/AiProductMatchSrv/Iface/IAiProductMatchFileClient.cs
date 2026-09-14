using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Iface
{
    public interface IAiProductMatchFileClient
    {
        // authorizationHeaderValue = دقیقاً همان مقدار هدر Authorization درخواست اصلی (مثلاً "Bearer xxx")،
        // بدون تغییر به سرویس جدای File فوروارد می‌شود چون آن Endpoint خودش [Authorize] است.
        Task<(bool Ok, long? PictureId, string Error)> UploadAsync(
            IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken);
    }
}
