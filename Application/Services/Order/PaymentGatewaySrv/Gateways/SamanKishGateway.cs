using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Order.MerchantSrv.Dto.SamanKishDto;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence.Interface;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class SamanKishGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.samanKish;

        private readonly IAdminSettingHelper _adminSettingHelper;
        private readonly IDataBaseContext _context;

        public SamanKishGateway(IAdminSettingHelper adminSettingHelper, IDataBaseContext context)
        {
            _adminSettingHelper = adminSettingHelper;
            _context = context;
        }

        public async Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            try
            {
                // آدرس برگشت باید callbackToken همین پرداخت را داشته باشد (قبلاً PaymentUrl + id بود و callback بدون توکن رد می‌شد)
                if (string.IsNullOrWhiteSpace(dto.CallbackUrl))
                    return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.ErrorOnStartPayment };

                var requestDto = new SamanKishRequestDto
                {
                    TerminalId = merchant.TerminalKey,
                    Amount = dto.Amount * 10,
                    ResNum = dto.PaymentId.ToString(),
                    RedirectUrl = dto.CallbackUrl,
                    CellNumber = dto.User?.Mobile
                };

                var client = new RestClient("https://sep.shaparak.ir/");
                var request = new RestRequest("onlinepg/onlinepg", method: Method.Post);
                request.AddJsonBody(requestDto);

                var response = client.ExecutePost(request);
                if (!response.IsSuccessStatusCode)
                    return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };

                var item = JsonConvert.DeserializeObject<SamanKishResponseDto>(response.Content);

                if (item.status != 1)
                    return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = item.errordesc };

                // نکته: این بهتره تو فرانت هندل بشه، ولی فعلاً همان مدل خودت:
                var html = $"<form id='f' action='https://sep.shaparak.ir/OnlinePG/OnlinePG' method='post'>" +
                           $"<input type='hidden' name='Token' value='{item.token}' />" +
                           $"<input type='hidden' name='GetMethod' value='true' />" +
                           $"</form><script>document.getElementById('f').submit();</script>";

                return new GatewayStartResultDto
                {
                    IsSuccess = true,
                    PaymentIsLink = false,
                    HtmlForm = html,
                    Token = item.token,
                    GatewayOrderId = dto.PaymentId.ToString()
                };
            }
            catch (Exception)
            {
                return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };
            }
        }

        public async Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request)
        {
            try
            {
                var state = HttpRequestParamReaderHelper.Get(request, "State");
                var status = HttpRequestParamReaderHelper.Get(request, "Status");
                var refNum = HttpRequestParamReaderHelper.Get(request, "RefNum");
                var traceNo = HttpRequestParamReaderHelper.Get(request, "TraceNo");

                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(refNum))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                if (!string.Equals(state?.Trim(), "OK", StringComparison.OrdinalIgnoreCase))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.PaymentNotCompletedAtGateway, Description = $"GATEWAY_{state}" };

                // نتیجه باید مال همین پرداخت باشد: شماره‌ی سفارش (ResNum) و توکنی که SEP موقع شروع داد
                var resNum = HttpRequestParamReaderHelper.Get(request, "ResNum");
                var callbackToken = HttpRequestParamReaderHelper.Get(request, "Token");
                if (!string.Equals(resNum?.Trim(), payment.Id.ToString(), StringComparison.Ordinal) ||
                    !string.IsNullOrWhiteSpace(payment.Token) && !string.IsNullOrWhiteSpace(callbackToken) &&
                    !string.Equals(payment.Token.Trim(), callbackToken.Trim(), StringComparison.Ordinal))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                // یک RefNum فقط برای یک پرداخت موفق (verify تکراریِ همان تراکنش برای پرداخت دیگری پذیرفته نمی‌شود)
                if (await _context.Payments.AsNoTracking().AnyAsync(s =>
                        s.Id != payment.Id && s.IsSuccess == true && s.RefNumber == refNum && s.MerchantId == payment.MerchantId))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                var verifyDto = new SamanKishVerifyRequestDto
                {
                    RefNum = refNum,
                    TerminalNumber = merchant.TerminalKey
                };

                var client = new RestClient("https://sep.shaparak.ir/");
                var verifyReq = new RestRequest("verifyTxnRandomSessionkey/ipg/VerifyTransaction", method: Method.Post);
                verifyReq.AddJsonBody(verifyDto);

                var verifyRes = client.ExecutePost(verifyReq);
                if (!verifyRes.IsSuccessStatusCode)
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };

                var verifyObj = JsonConvert.DeserializeObject<SamanKishVerifyResponseDto>(verifyRes.Content);

                // فقط verify اولِ همین تراکنش (resultCode 0) با مبلغ همین پرداخت پذیرفته می‌شود؛ مبلغ نامطابق برگشت داده می‌شود
                var expectedAmount = Convert.ToInt64(payment.Amount * 10);
                if (verifyObj?.Success == true &&
                    (verifyObj.resultCode != 0 || verifyObj.TransactionDetail == null ||
                     verifyObj.TransactionDetail.OrginalAmount != expectedAmount))
                {
                    if (verifyObj.resultCode == 0)
                    {
                        var reverseReq = new RestRequest("verifyTxnRandomSessionkey/ipg/ReverseTransaction", method: Method.Post);
                        reverseReq.AddJsonBody(verifyDto);
                        try { client.ExecutePost(reverseReq); } catch { /* برگشت best-effort؛ تأییدنشده از سمت ما = ناموفق */ }
                    }
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = false,
                        RefNumber = refNum,
                        Description = $"SAMAN_MISMATCH-{verifyObj.resultCode}-{verifyObj.TransactionDetail?.OrginalAmount}",
                        ErrorMessage = Resource.Notification.Unsuccess
                    };
                }

                if (verifyObj?.Success == true)
                {
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = true,
                        RefNumber = refNum,
                        TraceNumber = traceNo,
                        Token = refNum,
                        Description = traceNo
                    };
                }

                return new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    RefNumber = refNum,
                    TraceNumber = traceNo,
                    Description = $"{verifyObj.resultCode}-{verifyObj.ResultDescription}",
                    ErrorMessage = Resource.Notification.Unsuccess
                };
            }
            catch (Exception)
            {
                return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };
            }
        }
    }
}
