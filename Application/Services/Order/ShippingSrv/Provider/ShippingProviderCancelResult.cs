namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderCancelResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        public static ShippingProviderCancelResult Failed(string message) => new()
        {
            IsSuccess = false,
            ErrorMessage = message
        };

        public static ShippingProviderCancelResult Success() => new()
        {
            IsSuccess = true
        };
    }
}
