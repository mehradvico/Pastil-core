using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
    // اعتبارسنجی تصویر روی مرز اعتماد: پسوند به‌تنهایی قابل اعتماد نیست، پس بایت‌های ابتدای فایل هم
    // بررسی می‌شوند. یک نسخه برای همه‌ی مسیرهای آپلود تصویر تا قواعد از هم جدا نیفتند.
    public static class ImageContentValidator
    {
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static async Task<bool> IsAcceptableAsync(IFormFile image, long maxBytes, CancellationToken cancellationToken)
        {
            if (image == null || image.Length <= 0 || image.Length > maxBytes)
                return false;

            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return false;

            var header = new byte[12];
            await using var stream = image.OpenReadStream();
            var read = await stream.ReadAtLeastAsync(header, 12, throwOnEndOfStream: false, cancellationToken);
            return read >= 12 && HasImageMagicBytes(header);
        }

        public static bool HasImageMagicBytes(ReadOnlySpan<byte> header)
        {
            if (header.Length < 12)
                return false;

            var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            var isWebp = header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
                         && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P';
            return isJpeg || isPng || isWebp;
        }
    }
}
