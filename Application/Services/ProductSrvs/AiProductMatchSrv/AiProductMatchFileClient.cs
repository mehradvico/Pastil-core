using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // آپلود واقعی تصویر را (طبق تصمیم ۰.۶ سند طرح پیاده‌سازی) به سرویس جدای File می‌سپارد، نه
    // پردازش در‌جا داخل Api — چون Api و File در Production دو Container/دامنه‌ی جدا هستند.
    public class AiProductMatchFileClient : IAiProductMatchFileClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiProductMatchFileClient> _logger;

        public AiProductMatchFileClient(HttpClient httpClient, IConfiguration configuration, ILogger<AiProductMatchFileClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Ok, long? PictureId, string Error)> UploadAsync(
            IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken)
        {
            if (image == null || image.Length <= 0)
                return (false, null, "empty_file");

            var fileBaseUrl = _configuration["Urls:FileBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(fileBaseUrl))
            {
                _logger.LogError("Urls:FileBaseUrl is not configured; cannot forward picture upload for AiProductMatch.");
                return (false, null, "file_service_not_configured");
            }

            try
            {
                await using var stream = image.OpenReadStream();
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType);
                // نام فیلد باید دقیقاً "PictureFile" باشد — همان چیزی که File/Controllers/PictureUploadController.cs انتظار دارد
                content.Add(streamContent, "PictureFile", image.FileName);

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{fileBaseUrl}/api/PictureUpload")
                {
                    Content = content
                };
                if (!string.IsNullOrWhiteSpace(authorizationHeaderValue))
                    request.Headers.TryAddWithoutValidation("Authorization", authorizationHeaderValue);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Picture upload forwarding failed with HTTP {StatusCode}.", (int)response.StatusCode);
                    return (false, null, "http_" + (int)response.StatusCode);
                }

                var root = JsonNode.Parse(body);
                var isSuccess = root?["isSuccess"]?.GetValue<bool>() ?? false;
                if (!isSuccess)
                {
                    var message = root?["messages"]?[0]?["item1"]?.GetValue<string>();
                    return (false, null, message ?? "upload_rejected");
                }

                var pictureId = root?["data"]?["id"]?.GetValue<long?>();
                return (true, pictureId, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error forwarding picture upload for AiProductMatch.");
                return (false, null, "exception");
            }
        }
    }
}
