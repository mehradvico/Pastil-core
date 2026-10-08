namespace Application.Services.PansionSrvs.PansionReserveSrv.Dto
{
    // تأیید یا رد رزرو توسط مرکز (یا ادمین). Reason فقط برای رد الزامی است.
    public class PansionReserveDecisionDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
}
