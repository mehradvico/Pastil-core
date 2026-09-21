using System;
using System.Security.Cryptography;
using System.Text;

namespace Persistence.Security
{
    /// <summary>
    /// رمزنگاری ستونی (at-rest) برای داده‌های حساس دیتابیس، فعلاً شماره کارت و شبا.
    ///  - AES-256-GCM با nonce تصادفی برای هر مقدار؛ قالب ذخیره: <c>enc:v1:</c> + base64(nonce|tag|cipher)
    ///  - مقدار بدون پیشوند (داده‌ی قدیمی) هنگام خواندن همان‌طور برمی‌گردد و با اولین ذخیره‌ی بعدی/Backfill رمز می‌شود
    ///  - برای جست‌وجوی برابری (چک تکراری‌بودن کارت) از شاخص کور HMAC-SHA256 استفاده می‌شود؛ رمزنگاری تصادفی جواب برابری نمی‌دهد
    /// کلید: <c>Security:BankCardEncryptionKey</c> = base64 از ۳۲ بایت. دو کلید مستقل (رمزنگاری و شاخص) از آن مشتق می‌شود.
    /// اگر کلید تنظیم نباشد مقدار جدید بدون رمز ذخیره می‌شود (و در استارتاپ هشدار داده می‌شود)، ولی خواندنِ مقدار رمزشده بدون کلید خطا می‌دهد.
    /// </summary>
    public static class SensitiveDataProtector
    {
        public const string Prefix = "enc:v1:";

        private static byte[] _encryptionKey;
        private static byte[] _indexKey;

        public static bool IsConfigured => _encryptionKey != null;

        /// <summary>کلید base64 (۳۲ بایت) را می‌گیرد. مقدار نامعتبر/خالی ⇒ false و رمزنگاری غیرفعال.</summary>
        public static bool Configure(string base64Key)
        {
            _encryptionKey = null;
            _indexKey = null;

            if (string.IsNullOrWhiteSpace(base64Key))
                return false;

            byte[] master;
            try
            {
                master = Convert.FromBase64String(base64Key.Trim());
            }
            catch (FormatException)
            {
                return false;
            }

            if (master.Length != 32)
                return false;

            _encryptionKey = Derive(master, "pastil-sensitive-data-enc-v1");
            _indexKey = Derive(master, "pastil-sensitive-data-idx-v1");
            return true;
        }

        public static bool IsProtected(string value) =>
            !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);

        public static string Protect(string value)
        {
            if (string.IsNullOrEmpty(value) || IsProtected(value) || _encryptionKey == null)
                return value;

            var nonce = RandomNumberGenerator.GetBytes(12);
            var plain = Encoding.UTF8.GetBytes(value);
            var cipher = new byte[plain.Length];
            var tag = new byte[16];
            using var aes = new AesGcm(_encryptionKey, 16);
            aes.Encrypt(nonce, plain, cipher, tag);

            var payload = new byte[nonce.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);
            return Prefix + Convert.ToBase64String(payload);
        }

        public static string Unprotect(string value)
        {
            if (!IsProtected(value))
                return value;

            if (_encryptionKey == null)
                throw new InvalidOperationException("Encrypted data was read but Security:BankCardEncryptionKey is not configured.");

            var payload = Convert.FromBase64String(value.Substring(Prefix.Length));
            if (payload.Length < 28)
                throw new CryptographicException("Encrypted value is malformed.");

            var nonce = payload[..12];
            var tag = payload[12..28];
            var cipher = payload[28..];
            var plain = new byte[cipher.Length];
            using var aes = new AesGcm(_encryptionKey, 16);
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        /// <summary>شاخص کور قطعی (hex) برای مقایسه‌ی برابری؛ بدون کلید null برمی‌گرداند.</summary>
        public static string BlindIndex(string value)
        {
            if (string.IsNullOrEmpty(value) || _indexKey == null)
                return null;

            using var hmac = new HMACSHA256(_indexKey);
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static byte[] Derive(byte[] master, string purpose)
        {
            using var hmac = new HMACSHA256(master);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(purpose));
        }
    }
}
