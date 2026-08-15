using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
    public static class ReferralCodeGenerator
    {
        private const int MaximumAttemptCount = 50;

        public static Task<string> CreateUserCodeAsync(IDataBaseContext context)
        {
            return CreateAsync(context, prefix: null, randomDigitCount: 7);
        }

        public static Task<string> CreateCompanionCodeAsync(IDataBaseContext context)
        {
            return CreateAsync(context, prefix: "1", randomDigitCount: 9);
        }

        public static Task<string> CreateStoreCodeAsync(IDataBaseContext context)
        {
            return CreateAsync(context, prefix: "2", randomDigitCount: 9);
        }

        private static async Task<string> CreateAsync(
            IDataBaseContext context,
            string prefix,
            int randomDigitCount)
        {
            var minimum = randomDigitCount == 7 ? 1_000_000 : 0;
            var maximum = randomDigitCount == 7 ? 10_000_000 : 1_000_000_000;

            for (var attempt = 0; attempt < MaximumAttemptCount; attempt++)
            {
                var randomPart = RandomNumberGenerator.GetInt32(minimum, maximum)
                    .ToString($"D{randomDigitCount}");
                var code = string.Concat(prefix, randomPart);

                var exists = await context.Users.AnyAsync(item => item.ReferralCode == code) ||
                             await context.Companions.AnyAsync(item => item.ReferralCode == code) ||
                             await context.Stores.AnyAsync(item => item.ReferralCode == code);

                if (!exists)
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Unable to generate a unique referral code.");
        }
    }
}
