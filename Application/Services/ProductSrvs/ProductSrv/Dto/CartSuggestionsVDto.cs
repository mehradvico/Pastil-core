using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    /// <summary>«چیزی را فراموش نکردید؟»: کالاهای مکمل کل سبد + دلیل هر پیشنهاد.</summary>
    public class CartSuggestionsVDto
    {
        public List<ProductVDto> List { get; set; } = new List<ProductVDto>();
        public int TotalCount { get; set; }
        /// <summary>هم‌ترتیب با <c>List</c>؛ نوع کالا و کالایی از سبد که پیشنهاد به‌خاطر آن آمده.</summary>
        public List<CartSuggestionReasonVDto> Reasons { get; set; } = new List<CartSuggestionReasonVDto>();
    }

    public class CartSuggestionReasonVDto
    {
        public long ProductId { get; set; }
        /// <summary>کد نوع: bowl, litter_box, litter_scoop, litter_mat, pad, treat, toy, leash, collar, ...</summary>
        public string Kind { get; set; }
        /// <summary>شناسه‌ی محصولِ سبد که این پیشنهاد مکمل آن است.</summary>
        public long ForProductId { get; set; }
        public string ForProductName { get; set; }
    }
}
