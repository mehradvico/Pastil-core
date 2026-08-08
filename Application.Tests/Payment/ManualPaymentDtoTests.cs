using Application.Common.Enumerable;
using Application.Services.Order.PaymentSrv.Dto;
using System.Text.Json;
using Xunit;

namespace Application.Tests.Payment;

public class ManualPaymentDtoTests
{
    [Fact]
    public void TargetType_DeserializesFromDocumentedStringValue()
    {
        var dto = JsonSerializer.Deserialize<ManualPaymentDto>(
            """{"targetType":"PastilAI","referenceId":"2","userId":1500}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(dto);
        Assert.Equal(PaymentCallbackTypeEnum.PastilAI, dto.TargetType);
        Assert.Equal("2", dto.ReferenceId);
        Assert.Equal(1500, dto.UserId);
    }

    [Fact]
    public void TargetType_SerializesAsStringValue()
    {
        var dto = new ManualPaymentVDto
        {
            PaymentId = 10,
            TargetType = PaymentCallbackTypeEnum.Wallet,
            UserId = 20,
            Amount = 10000,
            IsSuccess = true
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"targetType\":\"Wallet\"", json);
    }
}
