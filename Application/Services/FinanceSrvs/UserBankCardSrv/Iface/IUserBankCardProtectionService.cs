using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.UserBankCardSrv.Iface
{
    public interface IUserBankCardProtectionService
    {
        /// <summary>
        /// رکوردهای قدیمیِ کارت بانکی (شماره کارت/شبا بدون رمز و بدون شاخص کور) را رمز می‌کند. idempotent است و فقط وقتی
        /// کلید Security:BankCardEncryptionKey تنظیم باشد کاری می‌کند. تعداد رکوردهای پردازش‌شده را برمی‌گرداند.
        /// </summary>
        Task<int> ProtectExistingAsync();
    }
}
