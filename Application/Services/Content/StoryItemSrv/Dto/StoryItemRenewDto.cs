namespace Application.Services.Content.StoryItemSrv.Dto
{
    public class StoryItemRenewDto
    {
        public long Id { get; set; }
        // اگر مشخص نشود، همان DayCount فعلی آیتم دوباره از همین لحظه اعمال می‌شود.
        public int? DayCount { get; set; }
    }
}
