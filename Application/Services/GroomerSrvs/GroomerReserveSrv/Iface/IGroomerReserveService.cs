using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerReserveSrv.Iface
{
    public interface IGroomerReserveService
    {
        /// <summary>قیمت بسته (پیش‌پرداخت و باقی‌مانده) قبل از رزرو.</summary>
        Task<BaseResultDto<GroomerReserveQuoteDto>> QuoteAsync(long packageId, bool pickupRequested);
        /// <summary>قیمت چند بسته‌ی یک رزرو (اولین = بسته‌ی اصلی).</summary>
        Task<BaseResultDto<GroomerReserveQuoteDto>> QuoteAsync(IReadOnlyList<long> packageIds, bool pickupRequested);

        /// <summary>زمان‌های آزاد یک روز برای بسته‌ی انتخابی (ظرفیت، هم‌پوشانی و گرومر انتخابی در نظر گرفته می‌شود).</summary>
        Task<BaseResultDto<GroomerAvailabilityDto>> GetAvailabilityAsync(long groomerId, DateTime date, long packageId, long? assigneeUserId, IReadOnlyList<long> extraPackageIds = null);

        Task<BaseResultDto<GroomerReserveVDto>> CreateAsync(GroomerReserveInputDto dto, long bookerId);

        GroomerReservePage Search(GroomerReserveSearchInputDto input);

        /// <summary>bookerId / ownerUserId (هر کدام پر باشد) دسترسی را به همان کاربر محدود می‌کند؛ هر دو خالی = ادمین.</summary>
        Task<BaseResultDto<GroomerReserveVDto>> FindAsync(long id, long? bookerId, long? ownerUserId);

        Task<BaseResultDto> SetRebateCodeAsync(long id, string rebateCode, long bookerId);
        Task<BaseResultDto> ClearRebateAsync(long id, long bookerId);
        Task<BaseResultDto> SetWalletAsync(long id, bool fromWallet, long bookerId);

        /// <summary>از PaymentService بعد از پرداخت موفق (یا پرداخت کامل با کیف پول) صدا زده می‌شود.</summary>
        Task<BaseResultDto> GroomerReservePaymentCallback(long? reserveId, bool fromWallet = false);

        /// <summary>تغییر زمان نوبت توسط مالک (ownerUserId) یا ادمین (null)؛ به کاربر پوش و پیامک می‌رود. مدت و بسته‌ها عوض نمی‌شود.</summary>
        Task<BaseResultDto<GroomerReserveVDto>> RescheduleAsync(long id, DateTime newDoDate, long? ownerUserId);

        /// <summary>تکمیل رزرو توسط مالک (ownerUserId) یا ادمین (null) بعد از زمان نوبت.</summary>
        /// <summary>تکمیل با بدنه‌ی اختیاری مبلغ نهایی/دریافتی در محل؛ userId = مالک یا پرسنل انتخاب‌شده، null = ادمین.</summary>
        Task<BaseResultDto<GroomerReserveVDto>> CompleteAsync(long id, long? userId, Application.Services.ReserveCompleteSrv.ReserveCompleteDto dto = null);
    }

    public interface IGroomerReserveApprovalService
    {
        Task<BaseResultDto> ApproveAsync(long reserveId, long? ownerUserId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> RejectAsync(long reserveId, string reason, long? ownerUserId, CancellationToken cancellationToken = default);
        /// <summary>لغو توسط خود کاربر: تا ۱۲ ساعت قبل از نوبت با برگشت کامل مبلغ به کیف پول، بعد از آن بدون برگشت.</summary>
        Task<BaseResultDto> CancelByBookerAsync(long reserveId, long bookerId, CancellationToken cancellationToken = default);
        /// <summary>لغو توسط ادمین؛ refund = true کل مبلغ پرداخت‌شده را به کیف پول کاربر برمی‌گرداند.</summary>
        Task<BaseResultDto> CancelByAdminAsync(long reserveId, string reason, bool refund, CancellationToken cancellationToken = default);
        /// <summary>کار زمان‌بندی‌شده: رزروهای بی‌پاسخ ماندگار را لغو و پولشان را برمی‌گرداند.</summary>
        Task ExpireOverdueAsync(CancellationToken cancellationToken = default);
    }
}
