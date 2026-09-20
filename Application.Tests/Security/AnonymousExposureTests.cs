using System.Text.RegularExpressions;
using Api.Filters;
using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Privacy;
using Application.Common.Security;
using Application.Services.Accounting.UserPetSrv;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Application.Tests.Security;

public class AnonymousExposureTests
{
    // ---------- پاک‌سازی PII ----------

    private static UserMinVDto Person() => new()
    {
        Id = 7,
        Mobile = "09121234567",
        Email = "vet@example.com",
        ReferralCode = "REF123",
        FirstName = "سارا",
        LastName = "احمدی",
        FullName = "سارا احمدی",
        IsFemale = true,
        Expertise = "دامپزشک"
    };

    [Fact]
    public void Scrub_RemovesContactDetails_FromNestedSearchResults_ButKeepsDisplayFields()
    {
        var search = new BaseSearchDto<CompanionUserVDto>
        {
            TotalCount = 1,
            List = new List<CompanionUserVDto> { new() { Id = 1, User = Person() } }
        };

        UserPiiScrubber.Scrub(search);

        var user = search.List[0].User;
        Assert.Null(user.Mobile);
        Assert.Null(user.Email);
        Assert.Null(user.ReferralCode);
        Assert.Equal("سارا احمدی", user.FullName);
        Assert.Equal("دامپزشک", user.Expertise);
        Assert.Equal(7, user.Id);
    }

    [Fact]
    public void Scrub_RemovesLocationRoleAndReferralData_FromTheFullUserDto()
    {
        var user = new UserVDto
        {
            Id = 9,
            Mobile = "0912",
            Email = "a@b.c",
            ReferralCode = "R",
            UsedReferralCode = "U",
            ReferredByUserId = 3,
            DriverId = 4,
            CompanionId = 5,
            RoleId = 1,
            RoleName = "Admin",
            FullName = "علی رضایی",
            UserCurrentLocation = new Application.Services.LocationFields.UserCurrentLocationSrv.Dto.UserCurrentLocationVDto()
        };

        UserPiiScrubber.Scrub(new List<UserVDto> { user });

        Assert.Null(user.Mobile);
        Assert.Null(user.Email);
        Assert.Null(user.ReferralCode);
        Assert.Null(user.UsedReferralCode);
        Assert.Null(user.ReferredByUserId);
        Assert.Null(user.DriverId);
        Assert.Null(user.CompanionId);
        Assert.Equal(0, user.RoleId);
        Assert.Null(user.RoleName);
        Assert.Null(user.UserCurrentLocation);
        Assert.Equal("علی رضایی", user.FullName);
    }

    [Fact]
    public void Scrub_HandlesNullAndCyclicGraphs_WithoutThrowing()
    {
        UserPiiScrubber.Scrub(null);

        var node = new CompanionUserVDto { User = Person() };
        var list = new List<object>();
        list.Add(list); // self-reference
        list.Add(node);

        UserPiiScrubber.Scrub(list);

        Assert.Null(node.User.Mobile);
    }

    // ---------- فیلتر سراسری ----------

    private static ResultExecutingContext Context(object value, params object[] endpointMetadata)
    {
        var descriptor = new ActionDescriptor { EndpointMetadata = endpointMetadata.ToList() };
        var actionContext = new ActionContext(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new RouteData(), descriptor);
        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(value), new object());
    }

    private static Task Run(ResultExecutingContext context) =>
        new ScrubAnonymousUserPiiFilter().OnResultExecutionAsync(context, () => Task.FromResult<ResultExecutedContext>(null!));

    [Fact]
    public async Task Filter_ScrubsEndpointsWithoutAuthorization()
    {
        var person = Person();
        await Run(Context(person)); // بدون هیچ metadata مجوز = ناشناس

        Assert.Null(person.Mobile);
    }

    [Fact]
    public async Task Filter_ScrubsAllowAnonymousEndpoints_EvenWhenClassIsAuthorized()
    {
        var person = Person();
        await Run(Context(person, new AuthorizeAttribute(), new AllowAnonymousAttribute()));

        Assert.Null(person.Mobile);
    }

    [Fact]
    public async Task Filter_LeavesAuthorizedEndpointsUntouched()
    {
        var person = Person();
        await Run(Context(person, new AuthorizeAttribute()));

        Assert.Equal("09121234567", person.Mobile);
    }

    // ---------- پت ----------

    [Fact]
    public void PetPublicView_HidesMedicalIdentityAndOwnerDetails()
    {
        var pet = new UserPetVDto
        {
            Id = 3,
            Name = "لوسی",
            UserId = 99,
            MicroChipCode = "900123",
            SpecificDisease = "دیابت",
            SpecificMedicene = "انسولین",
            AddressValue = "تهران، خیابان…",
            User = Person(),
            UserPetRecords = new List<Application.Services.Accounting.UserPerRecordSrv.Dto.UserPetRecordMinVDto>(),
            Size = "Small"
        };

        UserPetPrivacy.ToPublicView(pet);

        Assert.Equal("لوسی", pet.Name);
        Assert.Equal("Small", pet.Size);
        Assert.Equal(0, pet.UserId);
        Assert.Null(pet.User);
        Assert.Null(pet.MicroChipCode);
        Assert.Null(pet.SpecificDisease);
        Assert.Null(pet.SpecificMedicene);
        Assert.Null(pet.AddressValue);
        Assert.Null(pet.UserPetRecords);
    }

    // ---------- سقف صفحه‌بندی ----------

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-5, 20)]
    [InlineData(50, 50)]
    [InlineData(1000, 1000)]
    [InlineData(1001, 1000)]
    [InlineData(int.MaxValue, 1000)]
    public void PageSize_IsClampedToASafeRange(int requested, int expected)
    {
        var dto = new BaseInputDto { PageSize = requested };
        Assert.Equal(expected, dto.PageSize);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(4, 4)]
    [InlineData(int.MaxValue, 1_000_000)]
    public void PageIndex_NeverProducesANegativeOrOverflowingSkip(int requested, int expected)
    {
        var dto = new BaseInputDto { PageIndex = requested };
        Assert.Equal(expected, dto.PageIndex);
        Assert.True((long)(dto.PageIndex - 1) * dto.PageSize <= int.MaxValue);
    }

    // ---------- کلید JWT ----------

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("short-key", true)]
    [InlineData("0123456789012345678901234567890", true)]   // ۳۱ بایت
    [InlineData("01234567890123456789012345678901", false)] // ۳۲ بایت
    public void JwtKeyPolicy_FlagsKeysShorterThan256Bits(string? key, bool weak)
    {
        Assert.Equal(weak, JwtKeyPolicy.IsWeak(key));
    }

    // ---------- endpointهای نوشتنیِ ناشناس ----------

    // هر action غیر-GET که بدون احراز هویت قابل‌فراخوانی است باید اینجا با دلیل ثبت شده باشد. endpoint جدیدی که تصادفاً
    // ناشناس بماند (مثل نظردهی که قبلاً UserId را از بدنه می‌پذیرفت) این تست را می‌شکند.
    private static readonly HashSet<string> AllowedAnonymousWrites = new(StringComparer.Ordinal)
    {
        "AccountController.*",                 // ورود/ثبت‌نام/OTP/بازیابی (rate-limit شده)
        "ContactUsController.Post",            // فرم تماس (rate-limit شده)
        "NewsletterController.Post",           // عضویت خبرنامه (rate-limit شده)
        "SearchController.Post",               // جستجو (rate-limit شده)
        "MapSearchController.Post",            // جستجوی مکان (rate-limit شده)
        "DayToDateController.Post",            // تبدیل تاریخ (بدون داده)
        "MiareWebhookController.Receive",      // وب‌هوک با کلید مشترک
        "PushController.Subscribe",            // ثبت اشتراک پوش قبل از لاگین
        "PushController.SubscribeFcm",
    };

    [Fact]
    public void Every_anonymous_write_endpoint_is_on_the_reviewed_allowlist()
    {
        var root = FindBackendRoot();
        var unexpected = new List<string>();

        foreach (var path in Directory.EnumerateFiles(Path.Combine(root, "Api"), "*Controller.cs", SearchOption.AllDirectories)
                     .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                                 && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")))
        {
            var source = File.ReadAllText(path);
            var header = Regex.Match(source, @"((?:\s*\[[^\]]+\]\s*)*)\s*public\s+(?:sealed\s+)?class\s+(\w+)\s*:\s*(?:ControllerBase|Controller)");
            if (!header.Success)
                continue;

            var classAttributes = header.Groups[1].Value;
            var className = header.Groups[2].Value;
            var classAuthorized = classAttributes.Contains("Authorize");
            var classAnonymous = classAttributes.Contains("AllowAnonymous");
            var body = source[header.Index..];

            foreach (Match action in Regex.Matches(body, @"((?:\s*\[[^\]]+\]\s*)+)\s*public\s+(?:async\s+)?[\w<>\[\], ?]+\s+(\w+)\("))
            {
                var attributes = action.Groups[1].Value;
                var verb = Regex.Match(attributes, @"\[Http(Get|Post|Put|Delete|Patch)");
                if (!verb.Success || verb.Groups[1].Value == "Get")
                    continue;

                var actionAuthorized = attributes.Contains("Authorize") && !attributes.Contains("AllowAnonymous");
                var actionAnonymous = attributes.Contains("AllowAnonymous");
                var anonymous = actionAnonymous || (!classAuthorized && !actionAuthorized) || (classAnonymous && !actionAuthorized);
                if (!anonymous)
                    continue;

                var name = $"{className}.{action.Groups[2].Value}";
                if (!AllowedAnonymousWrites.Contains(name) && !AllowedAnonymousWrites.Contains($"{className}.*"))
                    unexpected.Add(name);
            }
        }

        Assert.True(unexpected.Count == 0,
            "Anonymous non-GET endpoints that are not on the reviewed allowlist: " + string.Join(", ", unexpected.Distinct()));
    }

    private static string FindBackendRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found above the test output directory.");
    }
}
