using Application.Services.Content.StoryItemSrv.Dto;

namespace Application.Services.Content.StoryGroupSrv.Dto
{
    // یک گروه استوری صفحه‌ی اصلی با تعداد استوری‌های زنده و اولین استوری‌اش (به‌جای ۱ + N درخواست story/items?PageSize=1)
    public class StoryGroupHomeVDto
    {
        public StoryGroupVDto Group { get; set; }
        public int ItemCount { get; set; }
        public StoryItemVDto FirstItem { get; set; }
    }
}
