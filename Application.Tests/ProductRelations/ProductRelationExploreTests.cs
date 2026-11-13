using Application.Services.ProductSrvs.ProductSrv;
using System.Text;
using Xunit;

namespace Application.Tests.ProductRelations;

// ابزار اکتشافی: فقط وقتی PASTIL_PRODUCTS_TSV ست باشد اجرا می‌شود (خروجی گزارش در PASTIL_RELATION_REPORT).
public class ProductRelationExploreTests
{
    private static List<ProductCandidate> Load(string path) =>
        File.ReadAllLines(path).Select(l => l.Split('|')).Where(a => a.Length >= 12 && long.TryParse(a[0], out _)).Select(a => new ProductCandidate
        {
            Id = long.Parse(a[0]), Name = a[1], SecondName = a[2],
            CategoryId = long.TryParse(a[3], out var c) ? c : null,
            BrandId = long.TryParse(a[4], out var b) ? b : null,
            Price = long.TryParse(a[5], out var pr) ? pr : 0,
            StatusId = long.TryParse(a[6], out var st) ? st : 0,
            SellCount = int.TryParse(a[7], out var sc) ? sc : 0,
            RateAvg = double.TryParse(a[8], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0,
            LeafName = a[9], ParentCategoryId = long.TryParse(a[10], out var pid) ? pid : null, ParentName = a[11],
        }).ToList();

    [Fact]
    public void Explore()
    {
        var path = Environment.GetEnvironmentVariable("PASTIL_PRODUCTS_TSV");
        var report = Environment.GetEnvironmentVariable("PASTIL_RELATION_REPORT");
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(report) || !File.Exists(path)) return;
        var all = Load(path);
        var sb = new StringBuilder();
        var profiles = all.ToDictionary(p => p.Id, ProductRelationEngine.Profile);

        sb.AppendLine("== kind distribution");
        foreach (var g in profiles.GroupBy(p => p.Value.Kind ?? "(null)").OrderByDescending(g => g.Count()))
            sb.AppendLine($"{g.Key}\t{g.Count()}");
        sb.AppendLine("== species distribution");
        foreach (var g in profiles.GroupBy(p => p.Value.Species).OrderByDescending(g => g.Count()))
            sb.AppendLine($"{g.Key}\t{g.Count()}");

        sb.AppendLine("== sample per kind (up to 12 each)");
        foreach (var g in profiles.GroupBy(p => p.Value.Kind ?? "(null)").OrderBy(g => g.Key))
        {
            sb.AppendLine($"-- {g.Key}");
            foreach (var p in g.OrderBy(x => x.Key).Where((_, i) => i % Math.Max(1, g.Count() / 12) == 0).Take(12))
            {
                var c = all.First(x => x.Id == p.Key);
                sb.AppendLine($"   [{p.Value.Species}] {c.Name}  <{c.LeafName}>");
            }
        }

        var rnd = new Random(7);
        var samples = new List<ProductCandidate>();
        foreach (var kw in new[] { "خاک گربه", "پد", "جای خواب", "تشویقی", "قلاده", "غذای خشک", "کنسرو", "کیف حمل", "شامپو", "اسباب بازی", "ظرف" })
            samples.AddRange(all.Where(p => p.Name.Contains(kw) && p.StatusId == 8).OrderBy(_ => rnd.Next()).Take(3));
        samples.AddRange(all.Where(p => p.StatusId == 8).OrderBy(_ => rnd.Next()).Take(15));
        foreach (var s in samples)
        {
            var pool = all.Where(p => p.CategoryId == s.CategoryId || (s.ParentCategoryId.HasValue && p.ParentCategoryId == s.ParentCategoryId) ||
                ProductRelationEngine.SpeciesLeafWords(profiles[s.Id].Species).Any(w => (p.LeafName ?? "").Contains(w))).ToList();
            sb.AppendLine($"#### SOURCE {s.Id} [{profiles[s.Id].Kind}/{profiles[s.Id].Species}] {s.Name} <{s.LeafName}>");
            sb.AppendLine("  similar:");
            foreach (var id in ProductRelationEngine.Similar(s, pool, 8)) { var c = all.First(x => x.Id == id); sb.AppendLine($"    - [{profiles[id].Kind}/{profiles[id].Species}] {c.Name} <{c.LeafName}> {(c.StatusId == 8 ? "" : "(ناموجود)")}"); }
            sb.AppendLine("  together:");
            foreach (var id in ProductRelationEngine.BoughtTogether(s, pool, new Dictionary<long, int>(), 8)) { var c = all.First(x => x.Id == id); sb.AppendLine($"    - [{profiles[id].Kind}/{profiles[id].Species}] {c.Name} <{c.LeafName}>"); }
        }

        sb.AppendLine("==== CART SCENARIOS");
        var carts = new[] { new[] { "خاک گربه", "غذای خشک گربه" }, new[] { "جای خواب مخصوص سگ", "تشویقی سگ" }, new[] { "قلاده", "شامپو سگ" }, new[] { "قفس پرندگان" }, new[] { "غذای خشک سگ" }, new[] { "کیف حمل" }, new[] { "اسکرچر" } };
        foreach (var kws in carts)
        {
            var cart = kws.Select(k => all.Where(p => p.Name.Contains(k) && p.StatusId == 8).FirstOrDefault() ?? all.First(p => p.Name.Contains(k))).ToList();
            var sp = cart.Aggregate(PetSpecies.None, (a, c) => a | profiles[c.Id].Species);
            var words = ProductRelationEngine.SpeciesLeafWords(sp);
            var parents = cart.Where(c => c.ParentCategoryId.HasValue).Select(c => c.ParentCategoryId.Value).ToHashSet();
            var pool = all.Where(p => cart.All(c => c.Id != p.Id) && ((p.ParentCategoryId.HasValue && parents.Contains(p.ParentCategoryId.Value)) || words.Any(w => (p.LeafName ?? "").Contains(w)))).ToList();
            sb.AppendLine("CART: " + string.Join(" + ", cart.Select(c => c.Name)));
            foreach (var x in ProductRelationEngine.CartSuggestions(cart, pool, new Dictionary<long, int>(), 8))
            { var c = all.First(y => y.Id == x.ProductId); sb.AppendLine($"    - [{x.Kind}] {c.Name}  (for {all.First(y => y.Id == x.ForProductId).Name})"); }
        }
        File.WriteAllText(report, sb.ToString());
    }
}
