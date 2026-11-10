using Application.Common.Enumerable.Message;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Setting.MessageSenderSrv
{
    public enum VenueRegistrationKind { Store, Clinic, Coach, Pansion, School, Groomer }

    /// <summary>
    /// پیامک ثبت مرکز جدید به مالک (ادمین یا خود ارائه‌دهنده). خطا پرت نمی‌کند تا ثبت مرکز را خراب نکند.
    /// قالب کاوه‌نگار: VenueRegisteredForOwner؛ %token = نوع مرکز، %token2 = نام مرکز.
    /// </summary>
    public interface IVenueRegistrationSms
    {
        Task SendAsync(VenueRegistrationKind kind, string venueName, long? ownerUserId, long? providerId = null, long? companionId = null);
    }

    public class VenueRegistrationSms : IVenueRegistrationSms
    {
        private static readonly CultureInfo Persian = new CultureInfo("fa");

        private readonly IDataBaseContext _context;
        private readonly IMessageSenderService _messageSender;
        private readonly ILogger<VenueRegistrationSms> _logger;

        public VenueRegistrationSms(IDataBaseContext context, IMessageSenderService messageSender, ILogger<VenueRegistrationSms> logger)
        {
            _context = context;
            _messageSender = messageSender;
            _logger = logger;
        }

        /// <summary>پیامک همیشه فارسی است، مستقل از زبان درخواست.</summary>
        public static string KindToken(VenueRegistrationKind kind) =>
            Resource.Notification.ResourceManager.GetString($"VenueRegistrationKind{kind}", Persian);

        public async Task SendAsync(VenueRegistrationKind kind, string venueName, long? ownerUserId, long? providerId = null, long? companionId = null)
        {
            try
            {
                var mobile = await FindOwnerMobileAsync(ownerUserId, providerId, companionId);
                if (string.IsNullOrWhiteSpace(mobile))
                {
                    _logger.LogWarning("Venue registration SMS skipped: no owner mobile for {Kind} {VenueName}.", kind, venueName);
                    return;
                }
                await _messageSender.SendMessageAsync(
                    MessageTypeEnum.VenueRegisteredForOwner,
                    mobile,
                    null,
                    token1: KindToken(kind),
                    token2: venueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Venue registration SMS failed for {Kind} {VenueName}.", kind, venueName);
            }
        }

        private async Task<string> FindOwnerMobileAsync(long? ownerUserId, long? providerId, long? companionId)
        {
            if (ownerUserId is > 0)
                return await _context.Users.AsNoTracking().Where(u => u.Id == ownerUserId.Value).Select(u => u.Mobile).FirstOrDefaultAsync();
            if (providerId is > 0)
                return await _context.Providers.AsNoTracking().Where(p => p.Id == providerId.Value).Select(p => p.User.Mobile).FirstOrDefaultAsync();
            if (companionId is > 0)
                return await _context.Companions.AsNoTracking().Where(c => c.Id == companionId.Value).Select(c => c.Owner.Mobile).FirstOrDefaultAsync();
            return null;
        }
    }
}
