using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    /// <summary>
    /// درگاه تستی پاستیل: بدون ارتباط با بانک، کاربر را مستقیم به callback برمی‌گرداند.
    /// فقط ادمین‌های پاستیل می‌توانند آن را ببینند (MerchantService.Search) و استفاده کنند (همین کلاس).
    /// </summary>
    public class PastilTestGateway : IPaymentGateway
    {
        public const string ResultQueryName = PaymentTestModeService.ResultQueryName;

        private readonly ICurrentUserHelper _currentUser;

        public PastilTestGateway(ICurrentUserHelper currentUser)
        {
            _currentUser = currentUser;
        }

        public MerchantEnum Provider => MerchantEnum.pastilTest;

        public Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            if (!IsAdmin(dto.UserId) || string.IsNullOrWhiteSpace(dto.CallbackUrl))
            {
                return Task.FromResult(new GatewayStartResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = Resource.Notification.Unsuccess
                });
            }

            dto.IsTestMode = true;
            dto.TestSuccessUrl = AddResult(dto.CallbackUrl, PaymentTestModeService.SuccessResult);
            dto.TestFailureUrl = AddResult(dto.CallbackUrl, PaymentTestModeService.FailureResult);

            return Task.FromResult(new GatewayStartResultDto
            {
                IsSuccess = true,
                PaymentIsLink = true,
                PaymentUrl = dto.TestSuccessUrl,
                Token = $"PASTIL-TEST-{dto.PaymentId}"
            });
        }

        public Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request)
        {
            var result = request?.Query[ResultQueryName].ToString();
            var failed = result?.Trim().ToLowerInvariant() is "0" or "false" or "fail" or "failed" or "failure";

            if (failed)
            {
                return Task.FromResult(new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = Resource.Notification.Unsuccess,
                    Description = "PASTIL_TEST_FAILED"
                });
            }

            return Task.FromResult(new GatewayCallbackResultDto
            {
                IsSuccess = true,
                RefNumber = $"PASTIL-TEST-{payment.Id}",
                Token = $"PASTIL-TEST-{payment.Id}",
                Description = "PASTIL_TEST_SUCCESS"
            });
        }

        private bool IsAdmin(long? userId)
        {
            // با Id نقش (main-admin)، نه Label: RoleEnum همان Roles.Label است و با "Admin" هرگز برابر نمی‌شد
            var user = _currentUser.CurrentUser;
            return userId.HasValue && user != null && user.UserId == userId.Value &&
                   Application.Services.Order.MerchantSrv.MerchantGatewayRules.IsMainAdmin(user);
        }

        private static string AddResult(string callbackUrl, string result)
        {
            var separator = callbackUrl.Contains('?') ? "&" : "?";
            return $"{callbackUrl}{separator}{ResultQueryName}={result}";
        }
    }
}
