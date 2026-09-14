namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    // نتیجه‌ی پارس هر آیتم داخل RowsJson (Excel/سپیدار/دامپزشکیار)
    public class AiProductMatchRowInputDto
    {
        public string RowId { get; set; }
        public string Name { get; set; }
        public string ExternalCode { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public string Unit { get; set; }
    }
}
