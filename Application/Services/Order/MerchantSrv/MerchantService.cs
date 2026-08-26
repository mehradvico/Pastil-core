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
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        private readonly byte[] _encryptionKey;
        private const string EncryptedPrefix = "enc:v1:";

        public MerchantService(
            IDataBaseContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IPaymentGatewayResolver gatewayResolver,
            IPaymentTestModeService paymentTestModeService,
            IConfiguration configuration)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _gatewayResolver = gatewayResolver;
            _paymentTestModeService = paymentTestModeService;
            _encryptionKey = ParseEncryptionKey(configuration["Security:MerchantEncryptionKey"]);
        }

        public override async Task<BaseResultDto<MerchantDto>> InsertAsyncDto(MerchantDto dto)
        {
            if (_encryptionKey == null)
                return new BaseResultDto<MerchantDto>(false, Resource.Notification.MerchantGatewayEncryptionKeyNotConfigured, null);

            var protectedDto = CopyDto(dto);
            ProtectDto(protectedDto);
            var result = await base.InsertAsyncDto(protectedDto);
            return result.IsSuccess
                ? new BaseResultDto<MerchantDto>(true, MaskDto(dto, result.Data?.Id ?? 0))
                : new BaseResultDto<MerchantDto>(false, result.Messages, null);
        }

        public async Task<BaseResultDto> UpdateSecureAsyncDto(MerchantDto dto)
        {
            if (_encryptionKey == null)
                return new BaseResultDto(false, Resource.Notification.MerchantGatewayEncryptionKeyNotConfigured);

            var merchant = await _context.Merchants.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (merchant == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            merchant.BankId = dto.BankId;
            merchant.Active = dto.Active;
            merchant.Username = ProtectIfProvided(dto.Username, merchant.Username);
            merchant.Password = ProtectIfProvided(dto.Password, merchant.Password);
            merchant.PrivateKey = ProtectIfProvided(dto.PrivateKey, merchant.PrivateKey);
            merchant.TerminalKey = ProtectIfProvided(dto.TerminalKey, merchant.TerminalKey);
            merchant.MerchantNo = ProtectIfProvided(dto.MerchantNo, merchant.MerchantNo);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
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
                .AsTracking()
                .Include(s => s.Bank)
                .FirstOrDefaultAsync(s => s.Id == dto.MerchantId);

            if (merchant == null || !merchant.Active)
            {
                return new BaseResultDto<PaymentStartDto>(
                    false,
                    Resource.Notification.Unsuccess,
                    dto);
            }

            await ProtectLegacySecretsAsync(merchant);

            if (_paymentTestModeService.IsEnabled)
            {
                _paymentTestModeService.ConfigureStartResult(dto);
                return new BaseResultDto<PaymentStartDto>(true, dto);
            }

            var gateway = _gatewayResolver.Resolve((MerchantEnum)merchant.BankId);
            var gatewayMerchant = TryCreateGatewayMerchant(merchant);
            if (gatewayMerchant == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            var gatewayResult = await gateway.StartAsync(dto, gatewayMerchant);

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
                .AsTracking()
                .Include(s => s.Bank)
                .FirstOrDefaultAsync(s => s.Id == payment.MerchantId);

            if (merchant == null || !merchant.Active)
            {
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            }

            await ProtectLegacySecretsAsync(merchant);

            var gateway = _gatewayResolver.Resolve((MerchantEnum)merchant.BankId);
            var gatewayMerchant = TryCreateGatewayMerchant(merchant);
            if (gatewayMerchant == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            var gatewayResult = await gateway.CallbackAsync(payment, gatewayMerchant, request);

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

        private static byte[] ParseEncryptionKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            try
            {
                var key = Convert.FromBase64String(value.Trim());
                return key.Length == 32 ? key : null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private string ProtectIfProvided(string value, string currentValue) =>
            string.IsNullOrWhiteSpace(value) ? currentValue : Protect(value.Trim());

        private async Task ProtectLegacySecretsAsync(Merchant merchant)
        {
            if (_encryptionKey == null || merchant == null)
                return;

            var values = new[]
            {
                merchant.Username,
                merchant.Password,
                merchant.PrivateKey,
                merchant.TerminalKey,
                merchant.MerchantNo
            };
            if (!values.Any(value => !string.IsNullOrWhiteSpace(value) &&
                                     !value.StartsWith(EncryptedPrefix, StringComparison.Ordinal)))
                return;

            merchant.Username = Protect(merchant.Username);
            merchant.Password = Protect(merchant.Password);
            merchant.PrivateKey = Protect(merchant.PrivateKey);
            merchant.TerminalKey = Protect(merchant.TerminalKey);
            merchant.MerchantNo = Protect(merchant.MerchantNo);
            await _context.SaveChangesAsync();
        }

        private void ProtectDto(MerchantDto dto)
        {
            dto.Username = ProtectIfProvided(dto.Username, null);
            dto.Password = ProtectIfProvided(dto.Password, null);
            dto.PrivateKey = ProtectIfProvided(dto.PrivateKey, null);
            dto.TerminalKey = ProtectIfProvided(dto.TerminalKey, null);
            dto.MerchantNo = ProtectIfProvided(dto.MerchantNo, null);
        }

        private string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
                return value;
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var plain = Encoding.UTF8.GetBytes(value);
            var cipher = new byte[plain.Length];
            using var aes = new AesGcm(_encryptionKey, 16);
            aes.Encrypt(nonce, plain, cipher, tag);
            var payload = new byte[nonce.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);
            return EncryptedPrefix + Convert.ToBase64String(payload);
        }

        private string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
                return value;
            var payload = Convert.FromBase64String(value[EncryptedPrefix.Length..]);
            if (payload.Length < 29)
                throw new CryptographicException();
            var nonce = payload[..12];
            var tag = payload[12..28];
            var cipher = payload[28..];
            var plain = new byte[cipher.Length];
            using var aes = new AesGcm(_encryptionKey, 16);
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        private Merchant TryCreateGatewayMerchant(Merchant source)
        {
            if (_encryptionKey == null)
                return null;
            try
            {
                return new Merchant
                {
                    Id = source.Id,
                    BankId = source.BankId,
                    Active = source.Active,
                    Bank = source.Bank,
                    Username = Unprotect(source.Username),
                    Password = Unprotect(source.Password),
                    PrivateKey = Unprotect(source.PrivateKey),
                    TerminalKey = Unprotect(source.TerminalKey),
                    MerchantNo = Unprotect(source.MerchantNo)
                };
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        private static MerchantDto CopyDto(MerchantDto dto) => new()
        {
            Id = dto.Id,
            BankId = dto.BankId,
            Active = dto.Active,
            Username = dto.Username,
            Password = dto.Password,
            PrivateKey = dto.PrivateKey,
            TerminalKey = dto.TerminalKey,
            MerchantNo = dto.MerchantNo
        };

        private static MerchantDto MaskDto(MerchantDto dto, long id) => new()
        {
            Id = id,
            BankId = dto.BankId,
            Active = dto.Active
        };
    }
}
