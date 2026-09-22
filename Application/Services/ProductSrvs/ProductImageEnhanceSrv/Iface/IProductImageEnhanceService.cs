using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductImageEnhanceSrv.Iface
{
    public enum ProductImageEnhanceStatus
    {
        Success = 0,
        InvalidImage = 1,
        TooLarge = 2,
        // محصولی در تصویر پیدا نشد که بشود جدایش کرد — اپ در این حالت خودش تصویر را روی بوم سفید می‌گذارد.
        SubjectNotFound = 3,
        ProviderUnavailable = 4
    }

    public class ProductImageEnhanceResult
    {
        public ProductImageEnhanceStatus Status { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string Message { get; set; }
    }

    public interface IProductImageEnhanceService
    {
        Task<ProductImageEnhanceResult> EnhanceAsync(IFormFile image, CancellationToken cancellationToken);
    }
}
