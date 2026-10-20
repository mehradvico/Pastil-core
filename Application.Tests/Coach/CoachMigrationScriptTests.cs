using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>اسکریپت انتقال مربی‌ها (scripts/MigrateCompanionCoachesToCoaches.sql) روی LocalDB اجرا و نتیجه‌اش بررسی می‌شود.</summary>
public class CoachMigrationScriptTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachMigrationScriptTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private static string ScriptText(string idsSql, bool deactivate = true)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !System.IO.File.Exists(Path.Combine(dir.FullName, "scripts", "MigrateCompanionCoachesToCoaches.sql")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        var text = System.IO.File.ReadAllText(Path.Combine(dir!.FullName, "scripts", "MigrateCompanionCoachesToCoaches.sql"))
            .Replace("DECLARE @Commit bit = 0;", "DECLARE @Commit bit = 1;")
            .Replace("DECLARE @DeactivateLegacyServices bit = 1;", "DECLARE @DeactivateLegacyServices bit = " + (deactivate ? "1" : "0") + ";");
        var marker = "-- INSERT INTO @CompanionIds (Id) VALUES (123), (456);";
        Assert.Contains(marker, text);
        return text.Replace(marker, idsSql);
    }

    private void RunScript(string idsSql)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = ScriptText(idsSql);
        command.CommandTimeout = 60;
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Script_MovesSelectedCompanions_ServicesPackagesModes_AndIsIdempotent()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        long personalId, otherId, visitId;
        using (var context = _fixture.NewContext())
        {
            var plain = new Assistance { Name = "Lesson", Active = true, IsGrooming = false };
            var grooming = new Assistance { Name = "Bath", Active = true, IsGrooming = true };
            context.Assistances.AddRange(plain, grooming);
            await context.SaveChangesAsync();

            var personal = new Companion { Name = "Trainer", ReferralCode = "RC1", OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", IsPersonal = true, Active = true, Approved = true };
            var other = new Companion { Name = "NotSelected", ReferralCode = "RC2", OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", Active = true, Approved = true };
            context.Companions.AddRange(personal, other);
            await context.SaveChangesAsync();
            personalId = personal.Id;
            otherId = other.Id;

            var visit = new CompanionAssistance { CompanionId = personal.Id, AssistanceId = plain.Id, Active = true, Approved = true, CompanionTypeId = 40, CommissionPercent = 9 };
            var bath = new CompanionAssistance { CompanionId = personal.Id, AssistanceId = grooming.Id, Active = true, Approved = true, CompanionTypeId = 40 };
            var notMine = new CompanionAssistance { CompanionId = other.Id, AssistanceId = plain.Id, Active = true, Approved = true, CompanionTypeId = 40 };
            context.CompanionAssistances.AddRange(visit, bath, notMine);
            await context.SaveChangesAsync();
            visitId = visit.Id;

            var simple = new CompanionAssistancePackage { Name = "Simple", Price = 300_000, PrePaymentPrice = 100_000, CompanionAssistanceId = visit.Id, Active = true };
            var modes = new CompanionAssistancePackage { Name = "Modes", Price = 120_000, PrePaymentPrice = 120_000, CompanionAssistanceId = visit.Id, Active = true };
            context.CompanionAssistancePackages.AddRange(simple, modes);
            await context.SaveChangesAsync();
            context.Set<CompanionAssistancePackageType>().AddRange(
                new CompanionAssistancePackageType { CompanionAssistancePackageId = modes.Id, CompanionAssistanceTypeId = 37, Price = 120_000, PrePaymentPrice = 120_000 },
                new CompanionAssistancePackageType { CompanionAssistancePackageId = modes.Id, CompanionAssistanceTypeId = 38, Price = 200_000, PrePaymentPrice = 40_000 });
            context.CompanionTimes.Add(new CompanionTime { CompanionId = personal.Id, WeekDayId = 1, StartTime = "9:00", EndTime = "17:00", Capacity = 2, Active = true });
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlRawAsync("INSERT INTO CodeCompanionAssistance (CodesId, CompanionAssistancesId) VALUES (38, {0}), (39, {0})", visitId);
        }

        var ids = $"INSERT INTO @CompanionIds (Id) VALUES ({personalId});";
        RunScript(ids);
        RunScript(ids); // idempotent

        using (var context = _fixture.NewContext())
        {
            var coaches = await context.Coaches.AsNoTracking().Where(c => c.LegacyCompanionId == personalId || c.LegacyCompanionId == otherId).ToListAsync();
            var coach = Assert.Single(coaches); // فقط انتخاب‌شده؛ دوباره‌اجرا کپی نمی‌سازد
            Assert.Equal(personalId, coach.LegacyCompanionId);
            Assert.True(coach.ServiceInPerson);
            Assert.True(coach.ServiceAtHome);
            Assert.True(coach.ServiceOnline);

            var service = Assert.Single(await context.CoachServices.AsNoTracking().Where(s => s.CoachId == coach.Id).ToListAsync()); // گرومینگ منتقل نشد
            Assert.Equal(9, service.CommissionPercent);
            var packages = await context.CoachServicePackages.AsNoTracking().Where(p => p.CoachServiceId == service.Id).ToListAsync();
            Assert.Equal(2, packages.Count);
            var simpleId = packages.Single(p => p.Name == "Simple").Id;
            var simpleModes = await context.CoachServicePackageTypes.AsNoTracking().Where(t => t.CoachServicePackageId == simpleId).OrderBy(t => t.Mode).ToListAsync();
            Assert.Equal(new[] { 38, 39 }, simpleModes.Select(t => t.Mode).ToArray());
            var times = await context.CoachTimes.AsNoTracking().Where(t => t.CoachId == coach.Id).ToListAsync();
            Assert.Equal(2, Assert.Single(times).Capacity);

            // خدمت منتقل‌شده در Companion غیرفعال شد؛ خدمت Companion انتخاب‌نشده دست‌نخورده است.
            Assert.False((await context.CompanionAssistances.AsNoTracking().FirstAsync(a => a.Id == visitId)).Active);
            Assert.True(await context.CompanionAssistances.AsNoTracking().AnyAsync(a => a.CompanionId == otherId && a.Active));
        }
    }

    [Fact]
    public void Script_WithNoSelection_DoesNothing()
    {
        if (!_fixture.Available) return;
        var before = CountCoaches();
        RunScript(""); // هیچ شناسه‌ای تعریف نشده
        Assert.Equal(before, CountCoaches());
    }

    private int CountCoaches()
    {
        using var context = _fixture.NewContext();
        return context.Coaches.Count();
    }
}
