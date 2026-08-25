using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Content.CargoSrv.Dto;
using Application.Services.TripSrv.TripSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripSrv.Iface
{
    public interface ITripService : ICommonSrv<Trip, TripDto>
    {

        TripSearchDto Search(TripInputDto baseSearchDto);
        Task<BaseResultDto<ManualPayTripDto>> ManualTripPaymentAsync(ManualPayTripDto dto);
        Task<BaseResultDto<TripVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> InsertOrUpdateAsync(TripDto dto);
        Task<BaseResultDto<TripVDto>> GetUserCurrentTrip(long userId);
        Task<BaseResultDto<TripVDto>> GetDriverCurrentTrip(long driverId);
        Task<BaseResultDto> TripPaymentCallback(long? tripId, bool fromWallet = false);
        Task<BaseResultDto<TripDriverChangeStatusDto>> UpdateTripDriverStatusAsync(TripDriverChangeStatusDto dto);
        Task<BaseResultDto<TripShareDto>> UpdateTripShareAsync(TripShareDto dto);
        Task<BaseResultDto<TripAdminChooseDriverDto>> ChooseDriverAsync(TripAdminChooseDriverDto dto);
        Task<BaseResultDto<TripChangeStatusDto>> TripChangeStatusAsync(TripChangeStatusDto dto);
        Task<BaseResultDto<TripUserChangeStatusDto>> UpdateTripUserStatusAsync(TripUserChangeStatusDto dto);
        Task<BaseResultDto> SetRebateCodeAsyncDto(TripSetRebateCodeDto dto);
        Task<BaseResultDto> SetWalletAsyncDto(TripSetWalletDto dto);
        Task<BaseResultDto> ClearRebateCodeAsync(long id);
        Task SyncDriverAcceptAsync();

        // پت‌رسان — مراحل ریز پیشرفت سفر
        Task<BaseResultDto<TripVDto>> AdvanceTripProgressAsync(long tripId, long driverId, TripProgressStageEnum targetStage);
        Task<BaseResultDto<TripLiveDto>> GetLiveForUserAsync(long tripId, long userId);
        Task<BaseResultDto<TripLiveDto>> GetLiveForDriverAsync(long tripId, long driverId);

        // پت‌رسان — حالت دو: سفر متصل به رزرو
        Task<BaseResultDto<TripDto>> CreateReservationLinkedTripAsync(TripReservationCreateDto dto, long userId);
        Task DispatchScheduledTripsAsync();

        // پت‌رسان — نقشه‌ی زنده‌ی ادمین
        Task<BaseResultDto<List<TripAdminLiveDto>>> GetActiveTripsForAdminAsync();

        // پت‌رسان — Broadcast سفر لحظه‌ای به همه‌ی راننده‌ها
        Task<BaseResultDto<List<TripVDto>>> GetAvailableTripsForDriverAsync();
        Task<BaseResultDto<TripVDto>> CancelByDriverAsync(long tripId, long driverId);
    }
}
