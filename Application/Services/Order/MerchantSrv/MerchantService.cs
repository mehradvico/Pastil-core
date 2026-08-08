using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.MerchantSrv.Iface;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.MerchantSrv
{
    public class MerchantService : CommonSrv<Merchant, MerchantDto>, IMerchantService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentGatewayResolver _gatewayResolver;
        private readonly IPaymentTestModeService _paymentTestModeService;

        public MerchantService(
            IDataBaseContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IPaymentGatewayResolver gatewayResolver,
            IPaymentTestModeService paymentTestModeService)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _gatewayResolver = gatewayResolver;
            _paymentTestModeService = paymentTestModeService;
        }

        public BaseSearchDto<MerchantVDto> Search(BaseInputDto baseSearchDto)
        {
            var query = _context.Merchants
                .Include(s => s.Bank)
                .ThenInclude(s => s.Picture)
                .AsQueryable();

            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.Bank.Name.Contains(baseSearchDto.Q));
            }

            if (baseSearchDto.Available.HasValue)
            {
                query = query.Where(s => s.Active == baseSearchDto.Available);
            }

            return new BaseSearchDto<Merchant, MerchantVDto>(baseSearchDto, query, _mapper);
        }

        public async Task<BaseResultDto> StartAsync(PaymentStartDto dto)
        {
            var merchant = await _context.Merchants
                .Include(s => s.Bank)
                .FirstOrDefaultAsync(s => s.Id == dto.MerchantId);

            if (merchant == null || !merchant.Active)
            {
                return new BaseResultDto<PaymentStartDto>(
                    false,
                    Resource.Notification.Unsuccess,
                    dto);
            }

            if (_paymentTestModeService.IsEnabled)
            {
                _paymentTestModeService.ConfigureStartResult(dto);
                return new BaseResultDto<PaymentStartDto>(true, dto);
            }

            var gateway = _gatewayResolver.Resolve((MerchantEnum)merchant.BankId);
            var gatewayResult = await gateway.StartAsync(dto, merchant);

            if (!gatewayResult.IsSuccess)
            {
                return new BaseResultDto<PaymentStartDto>(
                    false,
                    gatewayResult.ErrorMessage,
                    dto);
            }

            dto.PaymentIsLink = gatewayResult.PaymentIsLink;
            dto.PaymentUrl = gatewayResult.PaymentIsLink
                ? gatewayResult.PaymentUrl
                : gatewayResult.HtmlForm;

            return new BaseResultDto<PaymentStartDto>(true, dto);
        }

        public async Task<BaseResultDto> CallbackAsync(Payment payment)
        {
            if (payment == null)
            {
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            }

            if (_paymentTestModeService.IsEnabled)
            {
                var testResult = _paymentTestModeService.CreateCallbackResult(payment, request);
                return await SaveCallbackResultAsync(payment, testResult, true);
            }

            var merchant = await _context.Merchants
                .Include(s => s.Bank)
                .FirstOrDefaultAsync(s => s.Id == payment.MerchantId);

            if (merchant == null || !merchant.Active)
            {
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            }

            var gateway = _gatewayResolver.Resolve((MerchantEnum)merchant.BankId);
            var gatewayResult = await gateway.CallbackAsync(payment, merchant, request);

            return await SaveCallbackResultAsync(payment, gatewayResult, false);
        }

        private async Task<BaseResultDto> SaveCallbackResultAsync(
            Payment payment,
            GatewayCallbackResultDto result,
            bool isTestMode)
        {
            payment.IsSuccess = result.IsSuccess;
            payment.RefNumber = result.RefNumber;
            payment.Token = result.Token;
            payment.GatewayStatus = isTestMode
                ? result.IsSuccess ? "TEST_SUCCESS" : "TEST_FAILED"
                : result.IsSuccess ? "SUCCESS" : "FAILED";
            payment.Description = result.Description ?? result.ErrorMessage;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return new BaseResultDto(result.IsSuccess, result.ErrorMessage);
        }
    }
}
