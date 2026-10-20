using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Application.Common.Storage
{
    public class ObjectStorageFile
    {
        public required Stream Content { get; init; }
        public string? ContentType { get; init; }
        /// <summary>طول بدنه‌ی همین پاسخ (برای درخواست Range، طول همان بازه).</summary>
        public long? ContentLength { get; init; }
        /// <summary>فقط برای پاسخ جزئی (206) پر است، مثلاً "bytes 0-1/52341".</summary>
        public string? ContentRange { get; init; }
        /// <summary>بازه‌ی درخواستی خارج از طول فایل بود (416).</summary>
        public bool RangeNotSatisfiable { get; init; }
    }

    /// <summary>
    /// Thin wrapper around the Liara Object Storage (S3-compatible) bucket used by
    /// the File service. Keys are the same relative paths ("StaticFile/2026/9/29/x.jpg",
    /// "Media/2026/9/29/x.webp") that were historically written under wwwroot, so
    /// existing DB Url values keep working unchanged.
    /// </summary>
    public interface IObjectStorageService
    {
        bool IsConfigured { get; }

        Task UploadAsync(string key, Stream content, string? contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns null (not throws) when the key doesn't exist in the bucket.
        /// <paramref name="byteRange"/> is a single HTTP range ("bytes=0-1", "bytes=100-", "bytes=-500") forwarded to S3.
        /// </summary>
        Task<ObjectStorageFile?> TryGetAsync(string key, string? byteRange = null, CancellationToken cancellationToken = default);
    }
}
