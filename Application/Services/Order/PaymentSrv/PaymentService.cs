using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Application.Services.Content.CargoSrv.Iface;
using Application.Services.Order.MerchantSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentSrv
{
    public class PaymentService : CommonSrv<Payment, PaymentDto>, IPaymentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMerchantService _merchantService;
        private readonly IProductOrderService _productOrderService;
        private readonly ICompanionReserveService _companionReserveService;
        private readonly IWalletService _walletService;
        private readonly ICodeService _codeService;
        private readonly ITripService _tripService;
        private readonly ICargoService _cargoService;
        private readonly ICompanionInsurancePackageSaleService _companionInsurance;
        private readonly IPansionReserveService _pansionReserve;

        public PaymentService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, IWalletService walletService, IMerchantService merchantService, IPansionReserveService pansionReserve, IProductOrderService productOrderService, ICompanionReserveService companionReserveService, ITripService tripService, ICargoService cargoService, ICompanionInsurancePackageSaleService companionInsurance) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _merchantService = merchantService;
            _productOrderService = productOrderService;
            _walletService = walletService;
            _codeService = codeService;
            _companionReserveService = companionReserveService;
            _tripService = tripService;
            _cargoService = cargoService;
            _companionInsurance = companionInsurance;
            _pansionReserve = pansionReserve;

            


        }

        public async Task<BaseResultDto> InsertWalletPaymentAsyncDto(PaymentStartDto dto)
        {
            if (dto.Amount < 10000)
            {
                dto.Amount = 10000;
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            var paymentTypeWallet = await _codeService.GetByLabelAsync(PaymentTypeEnum.PaymentType_Wallet.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = paymentTypeWallet.Id;
            return await StartPayment(dto);

        }

        public async Task<BaseResultDto> StartPayment(PaymentStartDto dto)
        {
            try
            {
                if (dto.Amount < 10000)
                {
                    return new BaseResultDto(false, string.Format(Resource.Pattern.AmountsLessT1CannotPaid, 10000));

                }
                var item = mapper.Map<Payment>(dto);
                DateTime justNow = DateTime.Now;
                item.CreateDate = justNow;
                item.IsOnline = true;
                await _context.Payments.AddAsync(item);
                await _context.SaveChangesAsync();
                dto.PaymentId = item.Id;
                var initPayment = await _merchantService.StartAsync(dto);
                if (!initPayment.IsSuccess)
                {
                    item.IsSuccess = false;
                    item.Description = string.Format("{0}:{1}", Resource.Notification.ErrorOnStartPayment, initPayment.Messages);
                    _context.Payments.Update(item);
                    await _context.SaveChangesAsync(true);
                }
                return initPayment;
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public async Task<BaseResultDto<PaymentDto>> CallbackPayment(long paymentId, bool test = false)
        {
            try
            {
                var payment = await FindAsync(paymentId);
                if (payment == null)
                {
                    return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                }
                if (payment.IsSuccess != null)
                {
                    return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                }
                else
                {
                    var callback = await _merchantService.CallbackAsync(payment, test);
                    if (callback.IsSuccess)
                    {
                        if (payment.Type.Label == PaymentTypeEnum.PaymentType_ProductOrder.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.ProductOrder.ToString())
                            {
                                var productPaymentCallback = await _productOrderService.ProductPaymentCallback(payment.ProductOrderId, fromWallet: true);
                                if (!productPaymentCallback.IsSuccess)
                                {
                                    return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);

                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_CompanionReserve.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.CompanionReserve.ToString())
                            {
                                if (long.TryParse(payment.CallBackId, out var reserveId))
                                {
                                    var productPaymentCallback = await _companionReserveService.CompanionReservePaymentCallback(reserveId, fromWallet: true);
                                    if (!productPaymentCallback.IsSuccess)
                                    {
                                        return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                                    }
                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_PansionReserve.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.PansionReserve.ToString())
                            {
                                if (long.TryParse(payment.CallBackId, out var reserveId))
                                {
                                    var productPaymentCallback = await _pansionReserve.PansionReservePaymentCallback(reserveId, fromWallet: true);
                                    if (!productPaymentCallback.IsSuccess)
                                    {
                                        return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                                    }
                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_Trip.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Trip.ToString())
                            {
                                if (long.TryParse(payment.CallBackId, out var tripId))
                                {
                                    var productPaymentCallback = await _tripService.TripPaymentCallback(tripId, fromWallet: true);
                                    if (!productPaymentCallback.IsSuccess)
                                    {
                                        return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                                    }
                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_Cargo.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Cargo.ToString())
                            {
                                if (long.TryParse(payment.CallBackId, out var cargoId))
                                {
                                    var productPaymentCallback = await _cargoService.CargoPaymentCallback(cargoId, fromWallet: true);
                                    if (!productPaymentCallback.IsSuccess)
                                    {
                                        return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                                    }
                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_Insurance.ToString())
                        {
                            if (payment.CallBackTypeLabel == PaymentCallbackTypeEnum.Insurance.ToString())
                            {
                                if (long.TryParse(payment.CallBackId, out var insuranceId))
                                {
                                    var productPaymentCallback = await _companionInsurance.CompanionInsurancePackageSalePaymentCallback(insuranceId, fromWallet: true);
                                    if (!productPaymentCallback.IsSuccess)
                                    {
                                        return new BaseResultDto<PaymentDto>(isSuccess: false, val: Resource.Notification.Unsuccess, null);
                                    }
                                }
                            }
                        }
                        else if (payment.Type.Label == PaymentTypeEnum.PaymentType_Wallet.ToString())
                        {
                            await _walletService.WalletPaymentCallback(payment);
                        }

                    }
                    else
                    {
                        if (payment.Type.Label == PaymentTypeEnum.PaymentType_ProductOrder.ToString())
                        {
                            await _productOrderService.UpdateWalletAsync(payment.ProductOrderId, false);

                        }

                    }
                    return new BaseResultDto<PaymentDto>(isSuccess: callback.IsSuccess, data: mapper.Map<PaymentDto>(payment));

                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PaymentDto>(isSuccess: false, val: ex.Message, null);
            }
        }
        public BaseSearchDto<PaymentVDto> Search(PaymentInputDto baseSearchDto)
        {
            var query = _context.Payments.Include(s => s.Merchant).ThenInclude(s => s.Bank).Include(s => s.File).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.ProductOrderId))
            {
                query = query.Where(s => s.ProductOrderId == baseSearchDto.ProductOrderId);
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
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        query = query.OrderByDescending(s => s.Amount);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Amount);
                        break;
                    }
                default:
                    break;
            }

            return new BaseSearchDto<Payment, PaymentVDto>(baseSearchDto, query, mapper);
        }
        public async Task<BaseResultDto> InsertReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.CompanionReserves.FirstOrDefaultAsync(s => s.Id == dto.CompanionReserveId);

            dto.Amount = reservedetail.PrePaymentPrice;

            if (dto.Amount < 1)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(reservedetail.BookerId);

                if (walletAmount >= reservedetail.PrePaymentPrice)
                {
                    await _companionReserveService.CompanionReservePaymentCallback(reservedetail.Id, true);
                    return new BaseResultDto<PaymentStartDto>(true, dto);
                }
                else
                {
                    dto.Amount = reservedetail.PrePaymentPrice - walletAmount;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    return await InsertWalletPaymentAsyncDto(dto);

                }
            }
            var PaymentType_AgencyReserve = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_CompanionReserve.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_AgencyReserve;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.CompanionReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertPansionReservePaymentAsyncDto(PaymentStartDto dto)
        {
            var reservedetail = await _context.PansionReserves.FirstOrDefaultAsync(s => s.Id == dto.PansionReserveId);

            dto.Amount = reservedetail.PaymentPrice;

            if (dto.Amount < 1)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (reservedetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(reservedetail.BookerId);

                if (walletAmount >= reservedetail.PaymentPrice)
                {
                    await _pansionReserve.PansionReservePaymentCallback(reservedetail.Id, true);
                    return new BaseResultDto<PaymentStartDto>(true, dto);
                }
                else
                {
                    dto.Amount = reservedetail.PaymentPrice - walletAmount;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.PansionReserve.ToString();
                    dto.CallBackId = reservedetail.Id.ToString();
                    return await InsertWalletPaymentAsyncDto(dto);

                }
            }
            var PaymentType_AgencyReserve = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_PansionReserve.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_AgencyReserve;
            dto.CallBackTypeLabel = PaymentCallbackTypeEnum.PansionReserve.ToString();
            dto.CallBackId = reservedetail.Id.ToString();
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertTripPaymentAsyncDto(PaymentStartDto dto)
        {
            var tripdetail = await _context.Trips.FirstOrDefaultAsync(s => s.Id == dto.TripId);
            dto.Amount = tripdetail.PaymentPrice;

            if (tripdetail.TripStatusId != (long)TripStatusEnum.TripStatus_Accepted)
            {
                return new BaseResultDto(false, Resource.Notification.YourCargoRequestIsNotAccepted);
            }

            if (dto.Amount < 1)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (tripdetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(tripdetail.UserPet.UserId);

                if (walletAmount >= tripdetail.PaymentPrice)
                {
                    await _tripService.TripPaymentCallback(tripdetail.Id, true);
                    return new BaseResultDto<PaymentStartDto>(true, dto);
                }
                else
                {
                    dto.Amount = tripdetail.PaymentPrice - walletAmount;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Trip.ToString();
                    dto.CallBackId = tripdetail.Id.ToString();
                    return await InsertWalletPaymentAsyncDto(dto);

                }
            }
            var PaymentType_trip = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Trip.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_trip;
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertCargoPaymentAsyncDto(PaymentStartDto dto)
        {
            var cargodetail = await _context.Cargoes.Include(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == dto.CargoId);
            dto.Amount = cargodetail.PaymentPrice;

            if (cargodetail.StatusId != (long)CargoStatusEnum.CargoStatus_Accepted)
            {
                return new BaseResultDto(false, Resource.Notification.YourCargoRequestIsNotAccepted);
            }
            if (dto.Amount < 1)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (cargodetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(cargodetail.UserPet.UserId);

                if (walletAmount >= cargodetail.PaymentPrice)
                {
                    await _cargoService.CargoPaymentCallback(cargodetail.Id, true);
                    return new BaseResultDto<PaymentStartDto>(true, dto);
                }
                else
                {
                    dto.Amount = cargodetail.PaymentPrice - walletAmount;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Cargo.ToString();
                    dto.CallBackId = cargodetail.Id.ToString();
                    return await InsertWalletPaymentAsyncDto(dto);

                }
            }
            var PaymentType_cargo = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Cargo.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_cargo;
            return await StartPayment(dto);
        }

        public async Task<BaseResultDto> InsertCompanionInsurancePackageSalePaymentAsyncDto(PaymentStartDto dto)
        {
            var insuranceDetail = await _context.CompanionInsurancePackageSales.FirstOrDefaultAsync(s => s.Id == dto.CompanionInsurancePackageSaleId);
            dto.Amount = insuranceDetail.PaymentPrice;

            insuranceDetail.PaymentPrice = insuranceDetail.Price - insuranceDetail.RebatePrice;
            if (insuranceDetail.PaymentPrice < 0)
            {
                insuranceDetail.PaymentPrice = 0;
            }

            dto.Amount = insuranceDetail.PaymentPrice;

            if (dto.Amount < 1)
            {
                return new BaseResultDto(false, Resource.Notification.AmountNotCorrect);
            }
            else if (dto.MerchantId == null)
            {
                return new BaseResultDto(false, Resource.Notification.PleaseSelectTheMerchant);

            }
            if (insuranceDetail.FromWallet)
            {
                var walletAmount = await _walletService.GetAmountValueAsync(insuranceDetail.UserPet.UserId);

                if (walletAmount >= insuranceDetail.PaymentPrice)
                {
                    await _companionInsurance.CompanionInsurancePackageSalePaymentCallback(insuranceDetail.Id, true);
                    return new BaseResultDto<PaymentStartDto>(true, dto);
                }
                else
                {
                    dto.Amount = insuranceDetail.PaymentPrice - walletAmount;
                    dto.ProductOrderId = null;
                    dto.CallBackTypeLabel = PaymentCallbackTypeEnum.Insurance.ToString();
                    dto.CallBackId = insuranceDetail.Id.ToString();
                    return await InsertWalletPaymentAsyncDto(dto);

                }
            }
            var PaymentType_insurance = await _codeService.GetIdByLabelAsync(PaymentTypeEnum.PaymentType_Insurance.ToString());
            dto.IsOnline = true;
            dto.ProductOrderId = null;
            dto.TypeId = PaymentType_insurance;
            return await StartPayment(dto);
        }

        public async Task<Payment> FindAsync(long id)
        {
            return await _context.Payments.Include(s => s.User).Include(s => s.Type).Include(s => s.ProductOrder).Include(s => s.Merchant).ThenInclude(s => s.Bank).AsTracking().SingleOrDefaultAsync(s => s.Id == id);
        }
    }
}
