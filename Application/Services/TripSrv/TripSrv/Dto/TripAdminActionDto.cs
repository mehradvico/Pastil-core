using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// اقدام دستی ادمین روی یک سفر پت‌رسان (لغو / تکمیل). Detail همان توضیح ثبت‌شده‌ی ادمین است
    /// (برای لغو در CancelReasonDetail سفر ذخیره می‌شود).
    /// </summary>
    public class TripAdminActionDto : Id_FieldDto
    {
        [MaxLength(500)]
        public string Detail { get; set; }
    }
}
