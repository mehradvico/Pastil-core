using Application.Services.Order.PaymentGatewaySrv;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Application.Tests.Payment;

public class PaymentTestModeServiceTests
{
    [Fact]
    public void ConfigureStartResult_ReturnsSuccessAndFailureLinks()
    {
        var service = CreateService();
        var dto = new PaymentStartDto
        {
            CallbackUrl = "https://payment.pastil.pet/callback/125?callbackToken=secure-token"
        };

        service.ConfigureStartResult(dto);

        Assert.True(dto.IsTestMode);
        Assert.True(dto.PaymentIsLink);
        Assert.Equal(
            "https://payment.pastil.pet/callback/125?callbackToken=secure-token&testResult=success",
            dto.TestSuccessUrl);
        Assert.Equal(
            "https://payment.pastil.pet/callback/125?callbackToken=secure-token&testResult=failed",
            dto.TestFailureUrl);
        Assert.Equal(dto.TestSuccessUrl, dto.PaymentUrl);
    }

    [Fact]
    public void CreateCallbackResult_ReturnsFailed_WhenFailureIsRequested()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?testResult=failed");

        var result = service.CreateCallbackResult(new Entities.Entities.Payment { Id = 125 }, context.Request);

        Assert.False(result.IsSuccess);
        Assert.Equal("TEST_PAYMENT_FAILED", result.ErrorMessage);
        Assert.Equal("TEST_MODE_FAILED", result.Description);
    }

    [Fact]
    public void CreateCallbackResult_ReturnsSuccess_WhenSuccessIsRequested()
    {
        var service = CreateService();
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?testResult=success");

        var result = service.CreateCallbackResult(new Entities.Entities.Payment { Id = 125 }, context.Request);

        Assert.True(result.IsSuccess);
        Assert.Equal("TEST-125", result.RefNumber);
        Assert.Equal("TEST_MODE_SUCCESS", result.Description);
    }

    [Fact]
    public void LockedTestMode_IgnoresClientFailureOverride_AndDoesNotExposeQaLinks()
    {
        var service = CreateService(allowResultOverride: false);
        var dto = new PaymentStartDto
        {
            CallbackUrl = "https://payment.pastil.pet/callback/125?callbackToken=secure-token"
        };
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?testResult=failed");

        service.ConfigureStartResult(dto);
        var result = service.CreateCallbackResult(
            new Entities.Entities.Payment { Id = 125 },
            context.Request);

        Assert.Null(dto.TestSuccessUrl);
        Assert.Null(dto.TestFailureUrl);
        Assert.Equal(
            "https://payment.pastil.pet/callback/125?callbackToken=secure-token&testResult=success",
            dto.PaymentUrl);
        Assert.True(result.IsSuccess);
    }

    private static PaymentTestModeService CreateService(bool allowResultOverride = true)
    {
        var options = Options.Create(new PaymentTestModeOptions
        {
            Enabled = true,
            AllowResultOverride = allowResultOverride,
            DefaultResult = "Success"
        });

        return new PaymentTestModeService(options);
    }
}
