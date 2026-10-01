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

        /// <summary>Returns null (not throws) when the key doesn't exist in the bucket.</summary>
        Task<ObjectStorageFile?> TryGetAsync(string key, CancellationToken cancellationToken = default);
    }
}
