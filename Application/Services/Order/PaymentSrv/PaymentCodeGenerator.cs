using Application.Common.Enumerable;
using Persistence.Context;
using System;
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
            => BusinessCodeFormatter.Format(GetPrefix(targetType), createdAt, sequenceNumber);

        public static PaymentCallbackTypeEnum ParseTargetType(string value) =>
            Enum.TryParse<PaymentCallbackTypeEnum>(value, true, out var result)
                ? result
                : throw new ArgumentException("نوع پرداخت معتبر نیست.", nameof(value));

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
            PaymentCallbackTypeEnum.SchoolReserve => "SCL",
            PaymentCallbackTypeEnum.ConsultationPurchase => "CNS",
            PaymentCallbackTypeEnum.GroomerReserve => "GRM",
            PaymentCallbackTypeEnum.ClinicReserve => "CLN",
            PaymentCallbackTypeEnum.CoachReserve => "CCH",
            _ => throw new ArgumentOutOfRangeException(nameof(targetType))
        };
    }
}
