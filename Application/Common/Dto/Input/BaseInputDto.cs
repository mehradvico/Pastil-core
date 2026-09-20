using Application.Common.Enumerable;
using System;

namespace Application.Common.Dto.Input
{
    public class BaseInputDto
    {
        public BaseInputDto()
        {
            PageIndex = 1;
            PageSize = 20;
            SortBy = SortEnum.New;
        }
        /// <summary>
        /// شماره صفحه
        /// </summary>
        private int _pageIndex = 1;
        private int _pageSize = 20;

        public int PageIndex
        {
            get => _pageIndex;
            // Skip((PageIndex-1)*PageSize) با صفحه‌ی ۰/منفی خطای ۵۰۰ می‌داد و مقدار بسیار بزرگ سرریز می‌کرد
            set => _pageIndex = Math.Clamp(value, 1, 1_000_000);
        }
        /// <summary>
        /// تعداد در صفحه
        /// </summary>
        /// <summary>
        /// سقف ۱۰۰۰ (بزرگ‌ترین مقدار واقعی مصرف‌کننده‌ها: لیست برند/دسته در پنل). قبلاً بی‌سقف بود و هر endpoint عمومی را
        /// می‌شد با pageSize میلیونی یک‌جا خالی کرد.
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : Math.Min(value, 1000);
        }
        /// <summary>
        /// متن قابل جستجو
        /// </summary>
        public string Q { get; set; }
        /// <summary>
        /// مرتب سازی بر اساس 
        /// </summary>
        public SortEnum SortBy { get; set; }
        public bool? Available { get; set; }
    }
}
