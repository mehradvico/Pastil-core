using Application.Common.Enumerable;
using Application.Services.Order.PaymentSrv;
using System;
using Xunit;

namespace Application.Tests.Payment
{
    public class PaymentCodeGeneratorTests
    {
        [Fact]
        public void CreatePaymentToken_UsesRandomUppercaseLettersAndDigits()
        {
            var tokens = Enumerable.Range(0, 1000)
                .Select(_ => PaymentCodeGenerator.CreatePaymentToken())
                .ToArray();

            Assert.Equal(tokens.Length, tokens.Distinct().Count());
            Assert.All(tokens, token =>
            {
                Assert.Equal(24, token.Length);
                Assert.All(token, character => Assert.True(
                    character is >= 'A' and <= 'Z' or >= '0' and <= '9'));
                Assert.Contains(token, char.IsLetter);
                Assert.Contains(token, char.IsDigit);
            });
        }

        [Theory]
        [InlineData(PaymentCallbackTypeEnum.ProductOrder, "ORD")]
        [InlineData(PaymentCallbackTypeEnum.Wallet, "WLT")]
        [InlineData(PaymentCallbackTypeEnum.PastilAI, "PAI")]
        [InlineData(PaymentCallbackTypeEnum.CompanionReserve, "RSV")]
        [InlineData(PaymentCallbackTypeEnum.PansionReserve, "PAN")]
        [InlineData(PaymentCallbackTypeEnum.Trip, "TRP")]
        [InlineData(PaymentCallbackTypeEnum.Cargo, "CRG")]
        [InlineData(PaymentCallbackTypeEnum.Insurance, "INS")]
        public void Create_UsesExpectedPrefixJalaliDateAndFourDigitSuffix(
            PaymentCallbackTypeEnum targetType,
            string prefix)
        {
            var createdAt = new DateTime(2026, 8, 21, 12, 22, 0, DateTimeKind.Local);

            var result = PaymentCodeGenerator.Create(targetType, createdAt, 1);

            Assert.StartsWith($"{prefix}-14050530-1222-", result);
            var suffix = int.Parse(result.Split('-')[3]);
            Assert.InRange(suffix, 1000, 9999);
        }

        [Fact]
        public void Create_ProducesEveryFourDigitSuffixOnceWithinSameMinute()
        {
            var createdAt = new DateTime(2026, 8, 21, 12, 22, 0);
            var codes = Enumerable.Range(1, 9000)
                .Select(sequence => PaymentCodeGenerator.Create(
                    PaymentCallbackTypeEnum.ProductOrder,
                    createdAt,
                    sequence))
                .ToArray();

            Assert.Equal(9000, codes.Distinct().Count());
            Assert.All(codes, code =>
                Assert.InRange(int.Parse(code.Split('-')[3]), 1000, 9999));
        }
    }
}
