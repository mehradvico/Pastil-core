using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Order.MerchantSrv.Dto.SamanKishDto;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class SamanKishGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.samanKish;

        private readonly IAdminSettingHelper _adminSettingHelper;

        public SamanKishGateway(IAdminSettingHelper adminSettingHelper)
        {
            _adminSettingHelper = adminSettingHelper;
        }

        public async Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            try
            {
                var requestDto = new SamanKishRequestDto
                {
                    TerminalId = merchant.TerminalKey,
                    Amount = dto.Amount * 10,
                    ResNum = dto.PaymentId.ToString(),
                    RedirectUrl = _adminSettingHelper.BaseAdminSetting.PaymentUrl + dto.PaymentId,
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
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };

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

                if (verifyObj.Success)
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
