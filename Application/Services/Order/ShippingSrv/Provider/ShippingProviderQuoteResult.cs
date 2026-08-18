namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderQuoteResult
    {
        public bool IsSuccess { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; } = "IRR";
        public string ExternalQuoteId { get; set; }
        public string ErrorMessage { get; set; }

        public static ShippingProviderQuoteResult Failed(string message) => new()
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}
