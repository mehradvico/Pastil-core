using Application.Services.ProviderSrvs.ProviderSubscriptionSrv;
using Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv
{
    public class ProviderSubscriptionSyncService : IProviderSubscriptionSyncService
    {
        private readonly IDataBaseContext _context;

        public ProviderSubscriptionSyncService(IDataBaseContext _context)
        {
            this._context = _context;
        }

        public async Task SyncProviderAsync(long providerId)
        {
            var now = DateTime.Now;
            var current = await _context.ProviderSubscriptions.AsNoTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted && x.StartDate <= now && x.EndDate > now)
                .OrderByDescending(x => x.Plan.Level)
                .Select(x => new { x.Plan.Level, x.StartDate, x.EndDate })
                .FirstOrDefaultAsync();

            var (gold, silver, silverCreate) = current == null
                ? ProviderSubscriptionRules.ToLegacyAccountDates(null, now, now)
                : ProviderSubscriptionRules.ToLegacyAccountDates(current.Level, current.StartDate, current.EndDate);

            var companions = await _context.Companions
                .AsTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted)
                .ToListAsync();
            foreach (var companion in companions)
            {
                companion.GoldAccountDate = gold;
                companion.SilverAccountDate = silver;
                companion.SilverAccountCreateDate = silverCreate;
            }
            // آرایشگاه‌ها (Groomer) هم همین سطح را می‌گیرند؛ فقط تاریخ‌های طلایی/نقره‌ای دارند.
            var groomers = await _context.Groomers
                .AsTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted)
                .ToListAsync();
            foreach (var groomer in groomers)
            {
                groomer.GoldAccountDate = gold;
                groomer.SilverAccountDate = silver;
            }

            // کلینیک‌ها (Clinic) هم همین سطح را می‌گیرند؛ فقط تاریخ‌های طلایی/نقره‌ای دارند.
            var clinics = await _context.Clinics
                .AsTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted)
                .ToListAsync();
            foreach (var clinic in clinics)
            {
                clinic.GoldAccountDate = gold;
                clinic.SilverAccountDate = silver;
            }
            await _context.SaveChangesAsync();
        }
    }
}
