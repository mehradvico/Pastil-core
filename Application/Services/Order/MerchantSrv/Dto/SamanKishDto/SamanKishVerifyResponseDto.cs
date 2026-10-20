namespace Application.Services.Order.MerchantSrv.Dto.SamanKishDto
{
    public class SamanKishVerifyResponseDto
    {

        public int resultCode { get; set; }
        public string ResultDescription { get; set; }
        public bool Success { get; set; }
        // جزئیات تراکنش verifyشده؛ برای تطبیق مبلغ و ترمینال با همین پرداخت (نام OrginalAmount عیناً نام فیلد SEP است)
        public SamanKishTransactionDetailDto TransactionDetail { get; set; }

    }

    public class SamanKishTransactionDetailDto
    {
        public string RRN { get; set; }
        public string RefNum { get; set; }
        public long TerminalNumber { get; set; }
        public long OrginalAmount { get; set; }
        public long AffectiveAmount { get; set; }
    }
}
