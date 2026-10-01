using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Application.Common.Storage
{
    public class ObjectStorageService : IObjectStorageService
    {
        private readonly ObjectStorageOptions options;
        private readonly IAmazonS3? client;

        public ObjectStorageService(IOptions<ObjectStorageOptions> options)
        {
            this.options = options.Value;
            if (this.options.IsConfigured)
            {
                client = new AmazonS3Client(
                    new BasicAWSCredentials(this.options.AccessKey, this.options.SecretKey),
                    new AmazonS3Config
                    {
                        ServiceURL = this.options.Endpoint,
                        ForcePathStyle = true
                    });
            }
        }

        public bool IsConfigured => options.IsConfigured;

        public async Task UploadAsync(string key, Stream content, string? contentType, CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new System.InvalidOperationException("Object storage is not configured.");

            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                AutoCloseStream = false
            }, cancellationToken);
        }

        public async Task<ObjectStorageFile?> TryGetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (client == null)
                return null;

            try
            {
                var response = await client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = options.BucketName,
                    Key = key
                }, cancellationToken);

                return new ObjectStorageFile
                {
                    Content = response.ResponseStream,
                    ContentType = response.Headers["Content-Type"]
                };
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
