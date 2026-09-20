using Entities.Entities.CommonField;

namespace Entities.Entities
{
    // تصاویر یک درخواست «محصول ثبت‌نشده» (تا ۵ تا). دقیقاً هم‌شکل ProductPicture، چون بعد از تأیید
    // ادمین همین‌ها یک‌به‌یک به ProductPictureهای محصول کاتالوگ تبدیل می‌شوند.
    // MissingProduct.PictureId همچنان «کاور» است و همیشه با تصویر SortOrder=0 یکی نگه داشته می‌شود.
    public class MissingProductPicture : Id_Field
    {
        public long MissingProductId { get; set; }
        public long PictureId { get; set; }
        public int SortOrder { get; set; }

        public MissingProduct MissingProduct { get; set; }
        public Picture Picture { get; set; }
    }
}
