using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Iface
{
    public class AiProductMatchGeminiCallResult
    {
        public bool IsSuccess { get; private set; }
        public string RawJson { get; private set; }
        public string ErrorCode { get; private set; }

        public static AiProductMatchGeminiCallResult Success(string rawJson) =>
            new() { IsSuccess = true, RawJson = rawJson };

        public static AiProductMatchGeminiCallResult Failure(string errorCode) =>
            new() { IsSuccess = false, ErrorCode = errorCode };
    }

    public interface IAiProductMatchGeminiClient
    {
        // بررسی این‌که Provider جیمینای فعال و کلیدش تنظیم شده — بدون هیچ فراخوانی واقعی HTTP
        bool IsAvailable(out string providerName);

        // فراخوانی عمومی Gemini با درخواست صریح پاسخ JSON؛ images می‌تواند خالی باشد (فقط متن)
        Task<AiProductMatchGeminiCallResult> GenerateJsonAsync(
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            CancellationToken cancellationToken);
    }
}
