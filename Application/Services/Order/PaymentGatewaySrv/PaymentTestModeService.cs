using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace Application.Services.Order.PaymentGatewaySrv
{
    public sealed class PaymentTestModeService : IPaymentTestModeService
    {
        public const string ResultQueryName = "testResult";
        public const string SuccessResult = "success";
        public const string FailureResult = "failed";

        private readonly PaymentTestModeOptions _options;

        public PaymentTestModeService(IOptions<PaymentTestModeOptions> options)
        {
            _options = options.Value;
        }

        public bool IsEnabled => _options.Enabled;

        public void ConfigureStartResult(PaymentStartDto dto)
        {
            if (!IsEnabled)
            {
                return;
            }

            dto.IsTestMode = true;
            dto.PaymentIsLink = true;
            dto.TestSuccessUrl = AddResult(dto.CallbackUrl, SuccessResult);
            dto.TestFailureUrl = AddResult(dto.CallbackUrl, FailureResult);
            dto.PaymentUrl = IsFailure(_options.DefaultResult)
                ? dto.TestFailureUrl
                : dto.TestSuccessUrl;
        }

        public GatewayCallbackResultDto CreateCallbackResult(Payment payment, HttpRequest request)
        {
            var requestedResult = _options.DefaultResult;

            if (_options.AllowResultOverride && request != null)
            {
                var queryResult = request.Query[ResultQueryName].ToString();
                if (!string.IsNullOrWhiteSpace(queryResult))
                {
                    requestedResult = queryResult;
                }
            }

            if (IsFailure(requestedResult))
            {
                return new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "TEST_PAYMENT_FAILED",
                    Description = "TEST_MODE_FAILED"
                };
            }

            return new GatewayCallbackResultDto
            {
                IsSuccess = true,
                RefNumber = $"TEST-{payment.Id}",
                Description = "TEST_MODE_SUCCESS"
            };
        }

        private static string AddResult(string callbackUrl, string result)
        {
            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new InvalidOperationException("Payment callback URL is not configured.");
            }

            var separator = callbackUrl.Contains('?') ? "&" : "?";
            return $"{callbackUrl}{separator}{ResultQueryName}={result}";
        }

        private static bool IsFailure(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "0" or "false" or "fail" or "failed" or "failure" => true,
                _ => false
            };
        }
    }
}
