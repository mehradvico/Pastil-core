using Application.Services.ProductSrvs.ProductItemSrv;
using Xunit;

namespace Application.Tests.Reservation;

public class ProductVarietyRulesTests
{
    private static readonly IReadOnlySet<long> None = new HashSet<long>();

    private static IReadOnlySet<long> Set(params long[] ids) => new HashSet<long>(ids);

    [Fact]
    public void RowsAreValid_ProductWithoutVariety_OnlyAcceptsNullValues()
    {
        Assert.True(ProductVarietyRules.RowsAreValid(null, null, None, None, new (long?, long?)[] { (null, null) }));
        Assert.False(ProductVarietyRules.RowsAreValid(null, null, None, None, new (long?, long?)[] { (5, null) }));
        Assert.False(ProductVarietyRules.RowsAreValid(null, null, None, None, new (long?, long?)[] { (null, 5) }));
    }

    [Fact]
    public void RowsAreValid_ProductWithVariety_RequiresAValueFromThatVarietyOnly()
    {
        var valid1 = Set(10, 11, 12);

        Assert.True(ProductVarietyRules.RowsAreValid(1, null, valid1, None, new (long?, long?)[] { (10, null), (12, null) }));
        // مقدار تنوع دیگر
        Assert.False(ProductVarietyRules.RowsAreValid(1, null, valid1, None, new (long?, long?)[] { (99, null) }));
        // محصول دارای تنوع ولی سطر بدون مقدار (قفل قبلی را دور می‌زد و صفحه‌ی مشتری را می‌شکست)
        Assert.False(ProductVarietyRules.RowsAreValid(1, null, valid1, None, new (long?, long?)[] { (null, null) }));
        // مقدار برای تنوع دومی که محصول ندارد
        Assert.False(ProductVarietyRules.RowsAreValid(1, null, valid1, None, new (long?, long?)[] { (10, 20) }));
    }

    [Fact]
    public void RowsAreValid_TwoVarieties_BothSlotsMustBelongToTheirOwnVariety()
    {
        var valid1 = Set(10, 11);
        var valid2 = Set(20, 21);

        Assert.True(ProductVarietyRules.RowsAreValid(1, 2, valid1, valid2, new (long?, long?)[] { (10, 20), (11, 21) }));
        Assert.False(ProductVarietyRules.RowsAreValid(1, 2, valid1, valid2, new (long?, long?)[] { (10, null) }));
        Assert.False(ProductVarietyRules.RowsAreValid(1, 2, valid1, valid2, new (long?, long?)[] { (20, 10) })); // جابه‌جا
    }

    [Fact]
    public void RowsAreValid_OneBadRowRejectsTheWholeRequest()
    {
        var valid1 = Set(10);
        Assert.False(ProductVarietyRules.RowsAreValid(1, null, valid1, None, new (long?, long?)[] { (10, null), (77, null) }));
    }

    [Theory]
    [InlineData(null, null, 1, null, true, true)]   // افزودن تنوع اول وقتی آیتمی با مقدار وجود دارد
    [InlineData(1, null, 1, 2, true, true)]         // افزودن تنوع دوم روی آیتم‌های دارای مقدار
    [InlineData(1, null, null, null, true, true)]   // حذف تنوع
    [InlineData(1, null, 3, null, true, true)]      // جایگزینی تنوع
    [InlineData(1, 2, 2, 1, true, true)]            // جابه‌جایی ترتیب
    [InlineData(1, null, 3, null, false, false)]    // هیچ فروشنده‌ای آیتمی با مقدار ندارد ⇒ آزاد
    [InlineData(1, null, 1, null, true, false)]     // بدون تغییر ⇒ رد نمی‌شود
    public void ChangeIsBlocked_OnlyWhenStructureChangesWhileSellersUseValues(
        int? oldV1, int? oldV2, int? newV1, int? newV2, bool itemsUseValues, bool expectedBlocked)
    {
        Assert.Equal(expectedBlocked, ProductVarietyRules.ChangeIsBlocked(oldV1, oldV2, newV1, newV2, itemsUseValues));
    }
}
