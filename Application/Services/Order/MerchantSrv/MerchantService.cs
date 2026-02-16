using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.MerchantSrv.Iface;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.MerchantSrv
{
    public class MerchantService : CommonSrv<Merchant, MerchantDto>, IMerchantService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentGatewayResolver _gatewayResolver;

        public MerchantService(IDataBaseContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IPaymentGatewayResolver gatewayResolver) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _gatewayResolver = gatewayResolver;
        }

        public BaseSearchDto<MerchantVDto> Search(BaseInputDto baseSearchDto)
        {
            var query = _context.Merchants.Include(s => s.Bank).ThenInclude(s => s.Picture).AsQueryable();

            if (!string.IsNullOrEmpty(baseSearchDto.Q))
                query = query.Where(s => s.Bank.Name.Contains(baseSearchDto.Q));

            if (baseSearchDto.Available.HasValue)
                query = query.Where(s => s.Active == baseSearchDto.Available);

            return new BaseSearchDto<Merchant, MerchantVDto>(baseSearchDto, query, mapper);
        }

        public async Task<BaseResultDto> StartAsync(PaymentStartDto dto)
        {
            var merchant = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == dto.MerchantId);

            if (merchant == null || !merchant.Active)
            {
                return new BaseResultDto<PaymentStartDto>(false, Resource.Notification.Unsuccess, dto);
            }

            var provider = (MerchantEnum)merchant.BankId;
            var gateway = _gatewayResolver.Resolve(provider);
            var res = await gateway.StartAsync(dto, merchant);

            if (!res.IsSuccess)
            {
                return new BaseResultDto<PaymentStartDto>(false, res.ErrorMessage, dto);
            }

            dto.PaymentIsLink = res.PaymentIsLink;
            dto.PaymentUrl = res.PaymentIsLink ? res.PaymentUrl : res.HtmlForm;

            return new BaseResultDto<PaymentStartDto>(true, dto);
        }

        public async Task<BaseResultDto> CallbackAsync(Payment payment, bool test)
        {
            var merchant = await _context.Merchants.Include(s => s.Bank).FirstOrDefaultAsync(s => s.Id == payment.MerchantId);
            if (merchant == null)
            {
                return new BaseResultDto(false, Resource.Notification.Unsuccess);           
            }

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
    }
}
