using Application.Common.Enumerable;
using System;
using System.Globalization;
using System.Security.Cryptography;

namespace Application.Services.Order.PaymentSrv
{
    public static class PaymentCodeGenerator
    {
        private const string PaymentTokenLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string PaymentTokenDigits = "23456789";
        private const string PaymentTokenCharacters = PaymentTokenLetters + PaymentTokenDigits;

        public static string CreatePaymentToken(int length = 24)
        {
            if (length < 8 || length > 40)
                throw new ArgumentOutOfRangeException(nameof(length));

            var characters = new char[length];
            characters[0] = PaymentTokenLetters[RandomNumberGenerator.GetInt32(PaymentTokenLetters.Length)];
            characters[1] = PaymentTokenDigits[RandomNumberGenerator.GetInt32(PaymentTokenDigits.Length)];

            for (var index = 2; index < characters.Length; index++)
                characters[index] = PaymentTokenCharacters[RandomNumberGenerator.GetInt32(PaymentTokenCharacters.Length)];

            for (var index = characters.Length - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
            }

            return new string(characters);
        }

        public static string Create(
            PaymentCallbackTypeEnum targetType,
            DateTime createdAt,
            long sequenceNumber)
        {
            if (sequenceNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(sequenceNumber));

            var calendar = new PersianCalendar();
            var year = calendar.GetYear(createdAt);
            var month = calendar.GetMonth(createdAt);
            var day = calendar.GetDayOfMonth(createdAt);
            var jalaliDate = string.Concat(
                year.ToString("D4", CultureInfo.InvariantCulture),
                month.ToString("D2", CultureInfo.InvariantCulture),
                day.ToString("D2", CultureInfo.InvariantCulture));
            var time = createdAt.ToString("HHmm", CultureInfo.InvariantCulture);
            var minuteKey = year * 100000000L + month * 1000000L + day * 10000L +
                            createdAt.Hour * 100L + createdAt.Minute;
            var sequencePosition = (sequenceNumber - 1) % 9000;
            var minuteOffset = (minuteKey * 3571 + 1877) % 9000;
            var permutationExponent = (sequencePosition + minuteOffset) % 9000;
            var randomLookingSuffix = ModPow(7, permutationExponent, 9001) + 999;

            return $"{GetPrefix(targetType)}-{jalaliDate}-{time}-{randomLookingSuffix.ToString(CultureInfo.InvariantCulture)}";
        }

        public static PaymentCallbackTypeEnum ParseTargetType(string value) =>
            Enum.TryParse<PaymentCallbackTypeEnum>(value, true, out var result)
                ? result
                : throw new ArgumentException("نوع پرداخت معتبر نیست.", nameof(value));

        private static int ModPow(int value, long exponent, int modulus)
        {
            long result = 1;
            long factor = value % modulus;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    result = result * factor % modulus;

                factor = factor * factor % modulus;
                exponent >>= 1;
            }

            return (int)result;
        }

        private static string GetPrefix(PaymentCallbackTypeEnum targetType) => targetType switch
        {
            PaymentCallbackTypeEnum.ProductOrder => "ORD",
            PaymentCallbackTypeEnum.Wallet => "WLT",
            PaymentCallbackTypeEnum.PastilAI => "PAI",
            PaymentCallbackTypeEnum.CompanionReserve => "RSV",
            PaymentCallbackTypeEnum.PansionReserve => "PAN",
            PaymentCallbackTypeEnum.Trip => "TRP",
            PaymentCallbackTypeEnum.Cargo => "CRG",
            PaymentCallbackTypeEnum.Insurance => "INS",
            _ => throw new ArgumentOutOfRangeException(nameof(targetType))
        };
    }
}
