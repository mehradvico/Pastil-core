using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers.Iface;
using Application.Common.Service;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.MerchantSrv.Dto.SamanKishDto;
using Application.Services.Order.MerchantSrv.Iface;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence.Interface;
using RestSharp;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.MerchantSrv
{
    public class MerchantService : CommonSrv<Merchant, MerchantDto>, IMerchantService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAdminSettingHelper _adminSettingHelper;
        private readonly IPaymentGatewayResolver _gatewayResolver;



        public MerchantService(HttpClient httpClient, IDataBaseContext _context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAdminSettingHelper adminSettingHelper, IPaymentGatewayResolver gatewayResolver) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _adminSettingHelper = adminSettingHelper;
            _httpClient = httpClient;
            _gatewayResolver = gatewayResolver;
        }


        public BaseSearchDto<MerchantVDto> Search(BaseInputDto baseSearchDto)
        {
            var query = _context.Merchants.Include(s => s.Bank).ThenInclude(s => s.Picture).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.Bank.Name.Contains(baseSearchDto.Q));
            }
            if (baseSearchDto.Available.HasValue)
            {
                query = query.Where(s => s.Active == baseSearchDto.Available);
            }
            return new BaseSearchDto<Merchant, MerchantVDto>(baseSearchDto, query, mapper);
        }
        public async Task<BaseResultDto> StartAsync(PaymentStartDto dto)
        {
            var merchant = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == dto.MerchantId);
            if (merchant == null || !merchant.Active)
                return new BaseResultDto<PaymentStartDto>(false, Resource.Notification.Unsuccess, dto);

            var provider = (MerchantEnum)merchant.BankId;
            var gateway = _gatewayResolver.Resolve(provider);

            var res = await gateway.StartAsync(dto, merchant);
            if (!res.IsSuccess)
                return new BaseResultDto<PaymentStartDto>(false, res.ErrorMessage, dto);

            dto.PaymentIsLink = res.PaymentIsLink;
            dto.PaymentUrl = res.PaymentIsLink ? res.PaymentUrl : res.HtmlForm;

            return new BaseResultDto<PaymentStartDto>(true, dto);
        }
        public async Task<BaseResultDto> CallbackAsync(Payment payment, bool test)
        {
            var merchant = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == payment.MerchantId);
            if (merchant == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);

            var provider = (MerchantEnum)merchant.BankId;
            var gateway = _gatewayResolver.Resolve(provider);

            var res = await gateway.CallbackAsync(payment, merchant, _httpContextAccessor.HttpContext.Request, test);

            payment.IsSuccess = res.IsSuccess;
            payment.RefNumber = res.RefNumber;
            payment.Description = res.Description ?? res.ErrorMessage;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return new BaseResultDto(res.IsSuccess, res.ErrorMessage);
        }

        private async Task<BaseResultDto> ZarinPalStartAsync(PaymentStartDto dto)
        {
            try
            {
                var merchantDto = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == dto.MerchantId);




                var requestData = new
                {
                    merchant_id = merchantDto.MerchantNo,
                    amount = Convert.ToInt32(dto.Amount * 10),
                    callback_url = _adminSettingHelper.BaseAdminSetting.PaymentUrl + dto.PaymentId,
                    description = string.Format(Resource.Pattern.PaymentDescription, dto.ProductOrderId),
                    metadata = new { email = dto.User.Email, mobile = dto.User.Mobile }

                };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(ZarinPalApiUrl, content);

                //if (!response.IsSuccessStatusCode)
                //{
                var result = await response.Content.ReadAsStringAsync();
                var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ZarinPalPaymentResponse>(result);
                //if (jsonResponse.data.code == 100)
                //{
                // موفقیت آمیز، باید کاربر را به صفحه پرداخت زرین پال هدایت کنید

                dto.PaymentIsLink = true;
                dto.PaymentUrl = $"https://www.zarinpal.com/pg/StartPay/{jsonResponse.data.authority}";
                return new BaseResultDto<PaymentStartDto>(isSuccess: true, dto);
                //}
                //}


                //return new BaseResultDto<PaymentStartDto>(isSuccess: false, data: dto);

            }
            catch
            {
                return new BaseResultDto<PaymentStartDto>(isSuccess: false, val: Resource.Notification.Unsuccess, dto);
            }
        }
        private async Task<BaseResultDto> ZarinPalCallbackAsync(Entities.Entities.Payment payment, bool test)
        {
            try
            {
                if (!test)
                {
                    var merchantDto = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == payment.MerchantId);

                    string status = _httpContextAccessor.HttpContext.Request.Query["Status"];
                    string authority = _httpContextAccessor.HttpContext.Request.Query["Authority"];
                    if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(authority))
                    {
                        payment.IsSuccess = false;
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);
                    }
                    else
                    {
                        if (status.Trim().ToLower() != "ok")
                        {
                            payment.IsSuccess = false;
                            return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
                        }
                        else
                        {
                            var requestData = new
                            {
                                merchant_id = merchantDto.MerchantNo,
                                authority = authority,
                                amount = Convert.ToInt32(payment.Amount * 10),
                            };

                            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                            var response = await _httpClient.PostAsync(ZarinPalVerificationUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ZarinPalVerificationResponse>(result);


                                if (jsonResponse.data.code == 100)
                                {
                                    payment.RefNumber = jsonResponse.data.ref_id.ToString();
                                    payment.IsSuccess = true;
                                }
                                else
                                {
                                    payment.IsSuccess = false;
                                }
                                payment.Description = jsonResponse.data.code.ToString() + "--" + jsonResponse.data.ref_id.ToString();
                            }

                        }
                        _context.Payments.Update(payment);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    payment.IsSuccess = true;
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();
                }

                var paymentDto = mapper.Map<PaymentDto>(payment);
                return new BaseResultDto<PaymentDto>(isSuccess: payment.IsSuccess.Value, paymentDto);

            }
            catch
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }

        }
        private async Task<BaseResultDto> SamanKishStartAsync(PaymentStartDto dto)
        {
            try
            {
                var merchantDto = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == dto.MerchantId);
                var requestDto = new SamanKishRequestDto() { TerminalId = merchantDto.TerminalKey, Amount = dto.Amount * 10, ResNum = dto.PaymentId.ToString(), RedirectUrl = _adminSettingHelper.BaseAdminSetting.PaymentUrl + dto.PaymentId, CellNumber = dto.User.Mobile };
                var client = new RestClient("https://sep.shaparak.ir/");
                var request = new RestRequest($"onlinepg/onlinepg", method: Method.Post);
                request.AddJsonBody(requestDto);
                var response = client.ExecutePost(request);
                if (response.IsSuccessStatusCode)
                {
                    var item = JsonConvert.DeserializeObject<SamanKishResponseDto>(response.Content);
                    if (item.status == 1)
                    {
                        dto.PaymentIsLink = false;
                        dto.PaymentUrl = $"<form onload='document.forms['forms'].submit()' action='https://sep.shaparak.ir/OnlinePG/OnlinePG' method='post'><input type='hidden' name='Token' value='{item.token}' /><input name='GetMethod' type='text' value='true'></form>";
                        return new BaseResultDto<PaymentStartDto>(isSuccess: true, dto);
                    }
                    else
                    {
                        return new BaseResultDto<PaymentStartDto>(isSuccess: false, val: item.errordesc, dto);
                    }
                }
                else
                {
                    return new BaseResultDto<PaymentStartDto>(isSuccess: false, val: Resource.Notification.Unsuccess, dto);
                }
            }
            catch
            {
                return new BaseResultDto<PaymentStartDto>(isSuccess: false, val: Resource.Notification.Unsuccess, dto);
            }

        }
        private async Task<BaseResultDto> SamanKishCallbackAsync(Entities.Entities.Payment payment)
        {
            try
            {
                var merchantDto = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == payment.MerchantId);

                string state = _httpContextAccessor.HttpContext.Request.Query["State"];
                string status = _httpContextAccessor.HttpContext.Request.Query["Status"];
                string refNumber = _httpContextAccessor.HttpContext.Request.Query["RefNum"];
                string TraceNo = _httpContextAccessor.HttpContext.Request.Query["TraceNo"];
                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(refNumber))
                {
                    payment.IsSuccess = false;
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);
                }
                else
                {
                    if (state.Trim().ToLower() != "ok")
                    {
                        payment.IsSuccess = false;
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
                    }
                    else
                    {
                        var requestDto = new SamanKishVerifyRequestDto() { RefNum = refNumber, TerminalNumber = merchantDto.TerminalKey };

                        var client = new RestClient("https://sep.shaparak.ir/");
                        var request = new RestRequest($"verifyTxnRandomSessionkey/ipg/VerifyTransaction", method: Method.Post);
                        request.AddJsonBody(requestDto);
                        var response = client.ExecutePost(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var item = JsonConvert.DeserializeObject<SamanKishVerifyResponseDto>(response.Content);
                            if (item.Success)
                            {
                                payment.IsSuccess = true;
                                payment.Description = TraceNo;
                            }
                            else
                            {
                                payment.IsSuccess = false;
                                payment.Description = $"{item.resultCode}-{item.ResultDescription}";

                            }
                        }
                        else
                        {
                            payment.IsSuccess = false;
                        }
                        _context.Payments.Update(payment);
                        await _context.SaveChangesAsync();

                    }
                }

                var paymentDto = mapper.Map<PaymentDto>(payment);
                return new BaseResultDto<PaymentDto>(isSuccess: payment.IsSuccess.Value, paymentDto);

            }
            catch
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Unsuccess);
            }

        }


    }
}
