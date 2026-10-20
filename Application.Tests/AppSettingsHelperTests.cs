using Application.Common.Helpers;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Xunit;

namespace Application.Tests;

// AppSettingsHelper فقط در Payment مقداردهی می‌شد؛ خواندن آن در Api (مثلاً ساخت تصویر پیک میاره) NullReferenceException می‌داد.
[Collection("AppSettingsHelper")]
public class AppSettingsHelperTests
{
    private static readonly FieldInfo ConfigurationField =
        typeof(AppSettingsHelper).GetField("_configuration", BindingFlags.NonPublic | BindingFlags.Static)!;

    [Fact]
    public void UrlGetters_WhenNotInitialized_ReturnNullInsteadOfThrowing()
    {
        var previous = ConfigurationField.GetValue(null);
        try
        {
            ConfigurationField.SetValue(null, null);
            Assert.Null(AppSettingsHelper.FileBaseUrl);
            Assert.Null(AppSettingsHelper.BaseUrl);
            Assert.Null(AppSettingsHelper.ApiBaseUrl);
            Assert.False(AppSettingsHelper.PaymentStartFromApi);
        }
        finally
        {
            ConfigurationField.SetValue(null, previous);
        }
    }

    [Fact]
    public void UrlGetters_ReadConfigurationWhenInitialized()
    {
        var previous = ConfigurationField.GetValue(null);
        try
        {
            AppSettingsHelper.Initialize(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:FileBaseUrl"] = "https://file.example",
                ["Urls:PaymentStartFromApi"] = "true"
            }).Build());
            Assert.Equal("https://file.example", AppSettingsHelper.FileBaseUrl);
            Assert.True(AppSettingsHelper.PaymentStartFromApi);
        }
        finally
        {
            ConfigurationField.SetValue(null, previous);
        }
    }
}
