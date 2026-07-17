using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.WalletSrv
{
    public class WalletService : IWalletService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ISmsService _smsService;
        private readonly string connectionString;
        private readonly INoticeService _notificationService;

        public WalletService(IDataBaseContext _context, INoticeService notificationService, IConfiguration config, IMapper mapper, ISmsService smsService)
        {
            this._context = _context;
            this.mapper = mapper;
            this._smsService = smsService;
            this.connectionString = config.GetValue<string>("connection");
            this._notificationService = notificationService;


        }

        public async Task<BaseResultDto> DeleteAsync(long id)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.Id == id && s.Painding);
            if (item != null)
            {
                item.Deleted = true;
                _context.Wallets.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            return new BaseResultDto(false);
        }

        public async Task<BaseResultDto<WalletVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Wallets.Include(s => s.User).Include(s => s.ProductOrder).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<WalletVDto>(true, mapper.Map<WalletVDto>(item));
            }
            return new BaseResultDto<WalletVDto>(false, null);
        }

        public async Task<double> GetAmountValueAsync(long userId)
        {
            string sqlQuery = $@"SELECT ISNULL(SUM(CASE WHEN IsIncrease = 1 THEN Amount ELSE 0 END), 0) - ISNULL(SUM(CASE WHEN IsIncrease = 0 THEN Amount ELSE 0 END), 0) FROM Wallets WHERE userid = {userId} AND Deleted = 0";
            var connection = new SqlConnection(connectionString);
            double sum = await connection.ExecuteScalarAsync<double>(sqlQuery);
            sum = sum < 1 ? 0 : sum;
            return sum;
        }
        public async Task<BaseResultDto<double>> GetAmountAsync(long userId)
        {
            var amount = await GetAmountValueAsync(userId);
            return new BaseResultDto<double>(true, amount);
        }

        public async Task<BaseResultDto<WalletDto>> InsertAsyncDto(WalletDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<WalletDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.Amount == 0)
                    {
                        return new BaseResultDto<WalletDto>(false, Resource.Notification.PleaseEnterTheAmount, dto);
                    }
                    if (dto.Amount < 0)
                    {
                        dto.Amount = dto.Amount * -1;
                    }

                    var currentAmount = await GetAmountValueAsync(dto.UserId);
                    if (!dto.IsIncrease && dto.Amount > currentAmount)
                    {
                        return new BaseResultDto<WalletDto>(false, Resource.Notification.InsufficientFunds, dto);
                    }

                    var item = mapper.Map<Wallet>(dto);

                    item.CreateDate = DateTime.Now;
                    await _context.Wallets.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<WalletDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }


        public async Task<BaseResultDto<WalletDto>> InsertUpdateProductOrderAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.ProductOrderId == dto.ProductOrderId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateCargoAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.CargoId == dto.CargoId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateTripAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.TripId == dto.TripId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateReserveAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.CompanionReserveId == dto.CompanionReserveId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdatePansionReserveAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.PansionReserveId == dto.PansionReserveId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public async Task<BaseResultDto<WalletDto>> InsertUpdateInsuranceAsync(WalletDto dto, bool complete)
        {
            var item = await _context.Wallets.FirstOrDefaultAsync(s => s.CompanionInsurancePackageSaleId == dto.CompanionInsurancePackageSaleId);
            if (item != null)
            {

                if (complete)
                {
                    item.Painding = false;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));
                }
                else
                {
                    item.Deleted = true;
                    _context.Wallets.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto<WalletDto>(true, mapper.Map<WalletDto>(item));

                }
            }
            else
            {
                dto.IsIncrease = false;
                if (complete)
                {
                    dto.Painding = false;
                }
                else
                {
                    dto.Painding = true;
                }

                return await InsertAsyncDto(dto);
            }
        }
        public WalletSearchDto Search(WalletInputDto baseSearchDto)
        {
            var query = _context.Wallets.Include(s => s.User).Where(s => !s.Deleted).AsQueryable();

            if (baseSearchDto.UserId.HasValue)
            {
                query = query.Where(s => s.UserId == baseSearchDto.UserId);
            }
            if (baseSearchDto.IsIncrease.HasValue)
            {
                query = query.Where(s => s.IsIncrease == baseSearchDto.IsIncrease);
            }
            if (baseSearchDto.DateFrom.HasValue)
            {
                query = query.Where(s => s.CreateDate >= baseSearchDto.DateFrom);
            }
            if (baseSearchDto.DateTo.HasValue)
            {
                query = query.Where(s => s.CreateDate <= baseSearchDto.DateTo);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(baseSearchDto.Q) || s.ProductOrderId == baseSearchDto.Q);
            }

            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }

                default:
                    break;
            }
            return new WalletSearchDto(baseSearchDto, query, mapper);
        }

        public async Task<BaseResultDto> WalletPaymentCallback(Payment payment)
        {
            var walletDto = new WalletDto()
            {
                ProductOrderId = null,
                Amount = payment.Amount,
                IsIncrease = true,
                UserId = payment.UserId,
                PaymentId = payment.Id,
                Painding = false,
                Name = Resource.Lang.OnlinePayment,
            };
            var result = await InsertAsyncDto(walletDto);
            if (result.IsSuccess == true)
            {
                await _smsService.SendSmsAsync(smsType: Common.Enumerable.Message.MessageTypeEnum.IncreaseWallet, payment.User.Mobile, token1: payment.User.FirstName, token2: payment.Amount.ToString()/*, sendDate: DateTime.Now*/);
            }
            return result;
        }
    }
}
