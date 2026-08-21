using Application.Common.Enumerable;
using System.Text.Json.Serialization;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class ManualPaymentVDto
    {
        public long PaymentId { get; set; }
        public string PaymentCode { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentCallbackTypeEnum TargetType { get; set; }

        public string TargetReferenceId { get; set; }
        public string CallbackId { get; set; }
        public long UserId { get; set; }
        public double Amount { get; set; }
        public long? FileId { get; set; }
        public string RefNumber { get; set; }
        public string Description { get; set; }
        public bool IsSuccess { get; set; }
    }
}
