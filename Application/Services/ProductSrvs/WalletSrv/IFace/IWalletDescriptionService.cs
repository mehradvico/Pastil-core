using Application.Services.ProductSrvs.WalletSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.WalletSrv.IFace
{
    public interface IWalletDescriptionService
    {
        // Name هر ردیف را با شرح فارسی جایگزین می‌کند و مقدار خام (کلید دفتر) را در LedgerName نگه می‌دارد
        void Apply(IReadOnlyCollection<WalletVDto> rows);
        string Describe(WalletDto dto);
    }
}
