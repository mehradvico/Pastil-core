using Application.Common.Helpers;

namespace Application.Services.CommonSrv.SearchSrv.Iface
{
    public interface ISearchVocabularyService
    {
        /// <summary>کلمه‌های نام محصولاتِ قابل‌نمایش به مشتری؛ برای لیست/جستجوی محصولات.</summary>
        SearchVocabulary Products { get; }

        /// <summary>محصولات + برند، دسته، ویژگی، فروشگاه، مراکز، خدمات، دوره‌ها و بسته‌ها؛ برای جستجوی اصلی.</summary>
        SearchVocabulary Global { get; }
    }
}
