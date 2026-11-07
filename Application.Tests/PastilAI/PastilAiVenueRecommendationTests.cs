using System.Reflection;
using Application.Services.PastilAISrv;
using Application.Services.PastilAISrv.Dto;
using Application.Services.PastilAISrv.Provider;
using Application.Tests.Coach;
using GroomerEntity = Entities.Entities.GroomerField.Groomer;
using Entities.Entities.SchoolField;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Xunit;

namespace Application.Tests.PastilAi;

/// <summary>
/// PastilAI باید درخواست خدمت (آرایشگاه/کلینیک/مربی/پانسیون/مدرسه) را از درخواست کالا تشخیص دهد و
/// نمایندگی‌ها را با ترتیب «نزدیک → پیشنهاد پاستیل → امتیاز» از دیتابیس واقعی برگرداند.
/// </summary>
public class PastilAiVenueRecommendationTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _db;

    public PastilAiVenueRecommendationTests(CoachLocalDbFixture db) => _db = db;

    private static List<string> Intents(string question) =>
        (List<string>)typeof(PastilAiChatService)
            .GetMethod("GetVenueIntents", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { question })!;

    [Theory]
    [InlineData("برای سگم آرایش و اصلاح میخوام", "groomer")]
    [InlineData("برای سگم خدمات آرایشگاه میخوام", "groomer")]
    [InlineData("یه دامپزشک خوب برای واکسن گربه‌ام", "clinic")]
    [InlineData("سگم خیلی پارس میکنه مربی میخوام", "coach")]
    [InlineData("دارم میرم مسافرت سگمو کجا بسپارم", "pansion")]
    [InlineData("کلاس تربیت توله سگ", "school")]
    public void Detects_service_intent(string question, string expectedKind) =>
        Assert.Contains(expectedKind, Intents(question));

    [Fact]
    public void Behaviour_correction_is_coach_not_groomer()
    {
        var kinds = Intents("اصلاح رفتار سگم");
        Assert.Contains("coach", kinds);
        Assert.DoesNotContain("groomer", kinds);
    }

    [Fact]
    public void Plain_product_question_has_no_venue_intent() =>
        Assert.Empty(Intents("بهترین غذای خشک برای سگ بالغ چیست؟"));

    [Fact]
    public async Task Ranks_groomers_near_then_suggested_then_rating_and_skips_hidden()
    {
        if (!_db.Available) return;
        var seed = _db.Seed();
        var user = new Point(51.40, 35.70) { SRID = 4326 };
        var tag = Guid.NewGuid().ToString("N")[..6];

        GroomerEntity G(string name, double rate, bool suggested, Point? at, bool visible = true) => new()
        {
            Name = $"{name}-{tag}", OwnerId = seed.OwnerUserId, CityId = seed.CityId, StateId = seed.StateId,
            Active = true, Approved = visible, ShowToSite = visible, Deleted = false, Suggested = suggested,
            RateAvg = rate, RateCount = 10, Location = at, CreateDate = DateTime.Now
        };

        using (var ctx = _db.NewContext())
        {
            ctx.Groomers.AddRange(
                G("far-top", 5.0, true, new Point(59.60, 36.30) { SRID = 4326 }),   // مشهد: دور ولی پیشنهادی و ۵ ستاره
                G("near-low", 3.0, false, new Point(51.41, 35.71) { SRID = 4326 }),
                G("near-suggested", 4.0, true, new Point(51.42, 35.70) { SRID = 4326 }),
                G("near-high", 4.9, false, new Point(51.39, 35.69) { SRID = 4326 }),
                G("hidden", 5.0, true, new Point(51.40, 35.70) { SRID = 4326 }, visible: false));
            ctx.Schools.Add(new School
            {
                Name = $"school-{tag}", CityId = seed.CityId, StateId = seed.StateId, ProviderId = seed.ProviderId,
                Active = true, Approve = true, ShowToSite = true, RateAvg = 4.5, RateCount = 3
            });
            await ctx.SaveChangesAsync();
        }

        using var context = _db.NewContext();
        var service = new PastilAiChatService(context, null!, Options.Create(new PastilAiProviderOptions()), null!);
        var find = typeof(PastilAiChatService).GetMethod("FindVenuesAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var withLocation = await (Task<List<PastilAiRecommendedVenueDto>>)find.Invoke(service,
            new object[] { new List<string> { "groomer", "school" }, user, CancellationToken.None })!;
        var groomers = withLocation.Where(v => v.Kind == "groomer" && v.Name.EndsWith(tag)).Select(v => v.Name.Split('-')[0] + "-" + v.Name.Split('-')[1]).ToList();
        Assert.Equal(new[] { "near-suggested", "near-high", "near-low" }, groomers);
        Assert.All(withLocation.Where(v => v.Kind == "groomer"), v => Assert.True(v.DistanceMeters < 25_000));
        Assert.Contains(withLocation, v => v.Kind == "school" && v.Name == $"school-{tag}");
        Assert.DoesNotContain(withLocation, v => v.Name.StartsWith("hidden"));

        var noLocation = await (Task<List<PastilAiRecommendedVenueDto>>)find.Invoke(service,
            new object[] { new List<string> { "groomer" }, null!, CancellationToken.None })!;
        Assert.Equal($"far-top-{tag}", noLocation.First().Name);
        Assert.All(noLocation, v => Assert.Null(v.DistanceMeters));
    }
}
