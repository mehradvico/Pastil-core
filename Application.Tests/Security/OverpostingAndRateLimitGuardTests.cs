using System.Text.RegularExpressions;
using Xunit;

namespace Application.Tests.Security;

/// <summary>
/// نگهبان‌های ساختاری برای مسیرهای over-posting و rate-limit. این‌ها منطق را اجرا نمی‌کنند؛ جلوی برگشت تصادفی
/// قفل‌هایی را می‌گیرند که در بازبینی امنیتی اضافه شده‌اند (پروفایل، محصول فروشنده، بار، OTP و ...).
/// </summary>
public class OverpostingAndRateLimitGuardTests
{
    private static string Backend => FindBackendRoot();

    private static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(Backend, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    [Fact]
    public void ProfilePut_cannot_change_mobile_or_password_through_the_generic_update()
    {
        var source = Read("Api/Areas/EndUser/Controllers/UserController.cs");

        Assert.Contains("userDto.Mobile = _currentUserHelper.CurrentUser.Mobile;", source);
        Assert.Contains("userDto.Password = null;", source);
    }

    [Fact]
    public void UserService_update_keeps_the_original_CreateDate()
    {
        var source = Read("Application/Services/Accounting/UserSrv/UserService.cs");

        Assert.Matches(new Regex(@"var createDate = item\.CreateDate;\s+mapper\.Map\(dto, item\);\s+item\.Password = passwordHash;\s+item\.CreateDate = createDate;"), source);
    }

    [Fact]
    public void Cargo_insert_never_trusts_client_prices()
    {
        var source = Read("Application/Services/Content/CargoSrv/CargoService.cs");

        Assert.DoesNotContain("item.PaymentPrice = item.Price;\r\n\r\n                await _context.Cargoes.AddAsync", source);
        Assert.Matches(new Regex(@"item\.Price = 0;[\s\S]{0,400}item\.PaymentPrice = 0;"), source);
    }

    [Fact]
    public void Seller_product_post_always_starts_as_draft_with_zeroed_statistics()
    {
        var source = Read("Api/Areas/Seller/Controllers/ProductAdminController.cs");

        Assert.Contains("productDto.StatusId = (long)Application.Common.Enumerable.ProductStatusEnum.ProductStatus_Draft;", source);
        Assert.Contains("productDto.SellCount = 0;", source);
        Assert.Contains("productDto.AdminDescription = null;", source);
    }

    [Fact]
    public void Seller_product_update_preserves_status_ownership_and_statistics()
    {
        var source = Read("Application/Services/ProductSrvs/ProductSrv/ProductService.cs");

        foreach (var field in new[] { "StoreId", "StatusId", "SellCount", "VisitCount", "RateAvg", "RateCount", "AdminDescription" })
            Assert.Contains($"item.{field} = keep{field};", source);
    }

    [Fact]
    public void Order_status_and_state_changes_are_limited_to_the_defined_codes()
    {
        var source = Read("Application/Services/Order/ProductOrderSrv/ProductOrderService.cs");

        Assert.Contains("Enum.GetNames(typeof(ProductOrderStatusEnum))", source);
        Assert.Contains("Enum.GetNames(typeof(ProductOrderStateEnum))", source);
    }

    [Fact]
    public void Every_EnableRateLimiting_attribute_refers_to_a_registered_policy()
    {
        var program = Read("Api/Program.cs");
        var registered = Regex.Matches(program, @"options\.AddPolicy\(""(\w+)""")
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        var missing = new List<string>();
        foreach (var path in Directory.EnumerateFiles(Path.Combine(Backend, "Api"), "*.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                                 && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")))
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(path), @"EnableRateLimiting\(""(\w+)""\)"))
            {
                if (!registered.Contains(m.Groups[1].Value))
                    missing.Add($"{Path.GetFileName(path)}: {m.Groups[1].Value}");
            }
        }

        Assert.True(missing.Count == 0, "Rate-limit policies used but not registered: " + string.Join(", ", missing));
    }

    [Theory]
    [InlineData("Api/Areas/EndUser/Controllers/ChangeMobileController.cs", "changemobilerequest", "OtpSend")]
    [InlineData("Api/Areas/EndUser/Controllers/ChangeMobileController.cs", "changemobile\"", "OtpVerify")]
    [InlineData("Api/Areas/EndUser/Controllers/ChangeEmailController.cs", "changeemailrequest", "OtpSend")]
    [InlineData("Api/Areas/EndUser/Controllers/ChangeEmailController.cs", "changeemail\"", "OtpVerify")]
    public void Otp_sending_and_confirming_endpoints_are_rate_limited(string file, string route, string policy)
    {
        var source = Read(file);
        var index = source.IndexOf($"[Route(\"{route.TrimEnd('"')}\")]", StringComparison.Ordinal);
        Assert.True(index >= 0, $"route {route} not found in {file}");

        var window = source.Substring(index, Math.Min(220, source.Length - index));
        Assert.Contains($"EnableRateLimiting(\"{policy}\")", window);
    }

    [Fact]
    public void Public_pet_tag_lookup_is_rate_limited()
    {
        Assert.Contains("EnableRateLimiting(\"PetTagLookup\")", Read("Api/Controllers/PetTagPublicController.cs"));
    }

    [Fact]
    public void ChangeMobileRequest_validates_the_target_with_the_mobile_pattern_not_the_email_one()
    {
        var source = Read("Application/Services/Accounting/UserSrv/UserService.cs");
        var start = source.IndexOf("ChangeMobileRequestAsync(ChangeMobileDto dto)", StringComparison.Ordinal);
        var body = source.Substring(start, 900);

        Assert.Contains("RegixHelper.IsMobileAsync(dto.Mobile)", body);
        Assert.DoesNotContain("IsEmailAsync(dto.Mobile)", body);
    }

    [Fact]
    public void Localhost_cors_origins_are_only_allowed_in_development()
    {
        var program = Read("Api/Program.cs");

        Assert.Contains("builder.Environment.IsDevelopment()", program);
        Assert.DoesNotContain("                \"http://localhost:3000\",\r\n                \"http://localhost:3001\",\r\n                \"https://panel.pastil.pet\"", program);
    }

    [Fact]
    public void Excel_import_has_an_explicit_body_size_limit()
    {
        Assert.Contains("[RequestSizeLimit(", Read("Api/Areas/Admin/Controllers/ProductsExcelController.cs"));
    }

    private static string FindBackendRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found above the test output directory.");
    }
}
