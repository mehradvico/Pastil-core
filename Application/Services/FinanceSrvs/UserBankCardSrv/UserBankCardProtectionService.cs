using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using Persistence.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv
{
    public class UserBankCardProtectionService : IUserBankCardProtectionService
    {
        private const int BatchSize = 200;

        private readonly IDataBaseContext _context;
        private readonly ILogger<UserBankCardProtectionService> _logger;

        public UserBankCardProtectionService(IDataBaseContext context, ILogger<UserBankCardProtectionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> ProtectExistingAsync()
        {
            if (!SensitiveDataProtector.IsConfigured)
            {
                _logger.LogWarning("Bank card protection skipped: Security:BankCardEncryptionKey is not configured.");
                return 0;
            }

            var processed = 0;
            long lastId = 0;

            while (true)
            {
                // فقط رکوردهایی که شاخص کور ندارند (یعنی هنوز توسط این مسیر/ثبت جدید محافظت نشده‌اند)؛ با cursor روی Id تا رکورد
                // بدون شماره کارت باعث حلقه‌ی بی‌پایان نشود.
                var batch = await _context.UserBankCards
                    .AsTracking()
                    .Where(x => x.Id > lastId && x.CardNumberHash == null)
                    .OrderBy(x => x.Id)
                    .Take(BatchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                    break;

                foreach (var item in batch)
                {
                    lastId = item.Id;
                    if (string.IsNullOrWhiteSpace(item.CardNumber))
                        continue;

                    var digits = new string(item.CardNumber.Where(char.IsDigit).ToArray());
                    item.CardNumberHash = SensitiveDataProtector.BlindIndex(digits);

                    // مقدار در حافظه ساده است (converter موقع خواندن رمز را باز کرده)؛ علامت‌گذاری تغییر باعث می‌شود موقع ذخیره با converter رمز شود
                    var entry = ((DbContext)_context).Entry(item);
                    entry.Property(x => x.CardNumber).IsModified = true;
                    entry.Property(x => x.ShebaNumber).IsModified = true;
                    processed++;
                }

                await _context.SaveChangesAsync();
            }

            if (processed > 0)
                _logger.LogInformation("Protected {Count} legacy bank card records (encrypted card number/sheba + blind index).", processed);

            return processed;
        }
    }
}
