using Application.Common.Enumerable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Services.Order.PaymentSrv.Dto
{
    public class ManualPaymentDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentCallbackTypeEnum? TargetType { get; set; }

        public string ReferenceId { get; set; }
        public long? UserId { get; set; }
        public double? Amount { get; set; }
        public long? FileId { get; set; }

        [MaxLength(200)]
        public string RefNumber { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }
    }
}
