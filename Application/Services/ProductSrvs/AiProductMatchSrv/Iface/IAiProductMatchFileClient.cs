using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Iface
{
    public interface IAiProductMatchFileClient
    {
        Task<(bool Ok, long? PictureId, string Error)> UploadAsync(
            IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken);
    }
}
