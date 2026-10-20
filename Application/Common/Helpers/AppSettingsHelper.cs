using Microsoft.Extensions.Configuration;
using System;

namespace Application.Common.Helpers
{
    public static class AppSettingsHelper
    {
        private static IConfiguration _configuration;
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static string ApiBaseUrl => _configuration?["Urls:ApiBaseUrl"];
        public static string FileBaseUrl => _configuration?["Urls:FileBaseUrl"];
        public static string PaymentBaseUrl => _configuration?["Urls:PaymentBaseUrl"];
        public static string PaymentStartBaseUrl => _configuration?["Urls:PaymentStartBaseUrl"];
        public static bool PaymentStartFromApi => bool.TryParse(_configuration?["Urls:PaymentStartFromApi"], out var fromApi) && fromApi;
        public static string BaseUrl => _configuration?["Urls:BaseUrl"];
        public static string PaymentOrderUrl(long id) => $"{BaseUrl}{_configuration["Urls:OrderUrl"].Replace("id", id.ToString())}";
        public static string PaymentReserveUrl(long id) => $"{BaseUrl}{_configuration["Urls:ReserveUrl"].Replace("id", id.ToString())}";
        public static string PaymentTripUrl(long id) => $"{BaseUrl}{_configuration["Urls:TripUrl"].Replace("id", id.ToString())}";
        public static string PaymentCargoUrl(long id) => $"{BaseUrl}{_configuration["Urls:CargoUrl"].Replace("id", id.ToString())}";
        public static string PaymentInsuranceUrl(long id) => $"{BaseUrl}{_configuration["Urls:InsuranceUrl"].Replace("id", id.ToString())}";
        public static string PaymentWalletUrl(long id) => $"{BaseUrl}{_configuration["Urls:WalletUrl"].Replace("Id", id.ToString())}";
        public static bool IsOwner
        {
            get
            {
                var stringValue = _configuration?["AppSettings:IsOwner"];
                if (bool.TryParse(stringValue, out var result))
                    return result;
                return false;
            }
        }
        public static int OtpTryCount
        {
            get
            {
                var stringValue = _configuration?["AppSettings:Otp:TryCount"];
                if (int.TryParse(stringValue, out var result))
                    return result;
                return 3;
            }
        }
        public static int OtpWaitMinutes
        {
            get
            {
                var stringValue = _configuration?["AppSettings:Otp:WaitMinutes"];
                if (int.TryParse(stringValue, out var result))
                    return result;
                return 5;
            }
        }
        public static int OtpDigitsLenth
        {
            get
            {
                var stringValue = _configuration?["AppSettings:Otp:DigitsLenth"];
                if (int.TryParse(stringValue, out var result))
                    return result;
                return 4;
            }
        }
        public static int OtpValidityMinutes
        {
            get
            {
                var stringValue = _configuration?["AppSettings:Otp:ValidityMinutes"];
                if (int.TryParse(stringValue, out var result))
                    return result;
                return 5;
            }
        }
        public static bool OtpChangeCode
        {
            get
            {
                var stringValue = _configuration?["AppSettings:Otp:ChangeCode"];
                if (bool.TryParse(stringValue, out var result))
                    return result;
                return false;
            }
        }

        public static bool NationalCodeChecker
        {
            get
            {
                var stringValue = _configuration?["AppSettings:CheckNationalCode"];
                if (bool.TryParse(stringValue, out var result))
                    return result;
                return false;
            }
        }
    }
}
