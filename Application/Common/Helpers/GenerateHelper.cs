using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Helpers
{
    public static class GenerateHelper
    {
        /// <summary>
        /// کد عددی تصادفی با مولد امن رمزنگاری (برای OTP). رقم اول غیرصفر است تا طول ثابت بماند.
        /// </summary>
        public static string RandomDigit(int length = 4)
        {
            var random = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                random.Append(i == 0
                    ? RandomNumberGenerator.GetInt32(1, 10)
                    : RandomNumberGenerator.GetInt32(0, 10));
            }
            return random.ToString();
        }
    }
}
