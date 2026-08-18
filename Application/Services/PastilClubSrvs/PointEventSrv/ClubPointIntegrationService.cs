using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Entities.Entities.PastilClubField;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointEventSrv
{
    public class ClubPointIntegrationService : IClubPointIntegrationService
    {
        private readonly IClubPointEventService _clubPointEventService;
        private readonly ILogger<ClubPointIntegrationService> _logger;

        public ClubPointIntegrationService(
            IClubPointEventService clubPointEventService,
            ILogger<ClubPointIntegrationService> logger)
        {
            _clubPointEventService = clubPointEventService;
            _logger = logger;
        }

        public Task ProductOrderCompletedAsync(long userId, string orderId, CancellationToken cancellationToken = default) =>
            AwardSafeAsync(Create(
                userId,
                ClubPointEventTypeEnum.ProductOrderCompleted,
                ClubPointSourceTypeEnum.ProductOrder,
                ParseId(orderId),
                orderId,
                $"امتیاز تکمیل سفارش {orderId}"), cancellationToken);

        public Task ProductOrderReversedAsync(long userId, string orderId, CancellationToken cancellationToken = default) =>
            ReverseSafeAsync(Create(
                userId,
                ClubPointEventTypeEnum.ProductOrderCompleted,
                ClubPointSourceTypeEnum.ProductOrder,
                ParseId(orderId),
                orderId,
                $"بازگشت امتیاز سفارش {orderId}"), cancellationToken);

        public Task CompanionReserveCompletedAsync(long userId, long reserveId, CancellationToken cancellationToken = default) =>
            AwardSafeAsync(Create(userId, ClubPointEventTypeEnum.CompanionReservationCompleted,
                ClubPointSourceTypeEnum.CompanionReservation, reserveId, reserveId.ToString(),
                $"امتیاز تکمیل رزرو خدمات {reserveId}"), cancellationToken);

        public Task CompanionReserveReversedAsync(long userId, long reserveId, CancellationToken cancellationToken = default) =>
            ReverseSafeAsync(Create(userId, ClubPointEventTypeEnum.CompanionReservationCompleted,
                ClubPointSourceTypeEnum.CompanionReservation, reserveId, reserveId.ToString(),
                $"بازگشت امتیاز رزرو خدمات {reserveId}"), cancellationToken);

        public Task PansionReserveCompletedAsync(long userId, long reserveId, CancellationToken cancellationToken = default) =>
            AwardSafeAsync(Create(userId, ClubPointEventTypeEnum.PansionReservationCompleted,
                ClubPointSourceTypeEnum.PansionReservation, reserveId, reserveId.ToString(),
                $"امتیاز تکمیل رزرو پانسیون {reserveId}"), cancellationToken);

        public Task PansionReserveReversedAsync(long userId, long reserveId, CancellationToken cancellationToken = default) =>
            ReverseSafeAsync(Create(userId, ClubPointEventTypeEnum.PansionReservationCompleted,
                ClubPointSourceTypeEnum.PansionReservation, reserveId, reserveId.ToString(),
                $"بازگشت امتیاز رزرو پانسیون {reserveId}"), cancellationToken);

        public Task PetProfileCompletedAsync(long userId, long userPetId, CancellationToken cancellationToken = default) =>
            AwardSafeAsync(Create(userId, ClubPointEventTypeEnum.PetProfileCompleted,
                ClubPointSourceTypeEnum.PetProfile, userPetId, userPetId.ToString(),
                $"امتیاز تکمیل پروفایل پت {userPetId}"), cancellationToken);

        public Task MemoryCreatedAsync(
            long userId,
            long memoryId,
            DateTimeOffset memoryDate,
            CancellationToken cancellationToken = default) =>
            AwardSafeAsync(Create(userId, ClubPointEventTypeEnum.MemoryCreated,
                ClubPointSourceTypeEnum.Memory, memoryId, ClubPointEventKeyFactory.BuildMemorySourceKey(userId, memoryDate),
                $"امتیاز ثبت خاطره روزانه {memoryId}"), cancellationToken);

        public Task MemoryReversedAsync(
            long userId,
            long memoryId,
            DateTimeOffset memoryDate,
            CancellationToken cancellationToken = default) =>
            ReverseSafeAsync(Create(userId, ClubPointEventTypeEnum.MemoryCreated,
                ClubPointSourceTypeEnum.Memory, memoryId, ClubPointEventKeyFactory.BuildMemorySourceKey(userId, memoryDate),
                $"بازگشت امتیاز خاطره روزانه {memoryId}"), cancellationToken);

        public async Task RegistrationReferralCompletedAsync(
            long newUserId,
            long referrerUserId,
            bool isBusinessReferral,
            CancellationToken cancellationToken = default)
        {
            if (newUserId <= 0 || referrerUserId <= 0 || newUserId == referrerUserId)
                return;

            var sourceType = isBusinessReferral
                ? ClubPointSourceTypeEnum.BusinessReferral
                : ClubPointSourceTypeEnum.UserReferral;
            var referralKind = isBusinessReferral ? "business" : "user";

            await AwardSafeAsync(Create(
                referrerUserId,
                ClubPointEventTypeEnum.UserReferralReferrer,
                sourceType,
                newUserId,
                $"registration:{newUserId}:{referralKind}:referrer",
                $"امتیاز معرفی کاربر {newUserId}"), cancellationToken);

            await AwardSafeAsync(Create(
                newUserId,
                isBusinessReferral
                    ? ClubPointEventTypeEnum.BusinessReferralUser
                    : ClubPointEventTypeEnum.UserReferralReferee,
                sourceType,
                newUserId,
                $"registration:{newUserId}:{referralKind}:referee",
                isBusinessReferral
                    ? "امتیاز ثبت‌نام با کد معرف کسب‌وکار"
                    : $"امتیاز ثبت‌نام با کد معرف کاربر {referrerUserId}"), cancellationToken);
        }

        private async Task AwardSafeAsync(ClubPointEventDto dto, CancellationToken cancellationToken)
        {
            await ExecuteSafeAsync(
                () => _clubPointEventService.AwardAsync(dto, cancellationToken),
                dto);
        }

        private async Task ReverseSafeAsync(ClubPointEventDto dto, CancellationToken cancellationToken)
        {
            await ExecuteSafeAsync(
                () => _clubPointEventService.ReverseAsync(dto, cancellationToken),
                dto);
        }

        private async Task ExecuteSafeAsync(
            Func<Task<BaseResultDto<ClubPointTransactionVDto>>> action,
            ClubPointEventDto dto)
        {
            try
            {
                var result = await action();
                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Pastil Club point event was rejected. EventType: {EventType}, SourceKey: {SourceKey}",
                        dto.EventType,
                        dto.SourceKey);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Pastil Club point event failed. EventType: {EventType}, SourceKey: {SourceKey}",
                    dto.EventType,
                    dto.SourceKey);
            }
        }

        private static ClubPointEventDto Create(
            long userId,
            ClubPointEventTypeEnum eventType,
            ClubPointSourceTypeEnum sourceType,
            long? sourceId,
            string sourceKey,
            string description)
        {
            return new ClubPointEventDto
            {
                UserId = userId,
                EventType = eventType,
                SourceType = sourceType,
                SourceId = sourceId,
                SourceKey = sourceKey,
                Description = description
            };
        }

        private static long? ParseId(string value) =>
            long.TryParse(value, out var id) ? id : null;

    }
}
