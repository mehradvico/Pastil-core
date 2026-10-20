using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Clinic;

/// <summary>اسکریپت انتقال کلینیک‌های قدیمی (scripts/MigrateCompanionClinicsToClinics.sql) روی LocalDB اجرا و نتیجه‌اش بررسی می‌شود.</summary>
public class ClinicMigrationScriptTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicMigrationScriptTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private static string ScriptText()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !System.IO.File.Exists(Path.Combine(dir.FullName, "scripts", "MigrateCompanionClinicsToClinics.sql")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        return System.IO.File.ReadAllText(Path.Combine(dir!.FullName, "scripts", "MigrateCompanionClinicsToClinics.sql"))
            .Replace("DECLARE @Commit bit = 0;", "DECLARE @Commit bit = 1;");
    }

    private void RunScript()
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = ScriptText();
        command.CommandTimeout = 60;
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Script_MovesClinicsServicesPackagesAndModes_AndIsIdempotent()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        long companionId, personalId, clinicAssistanceId;
        using (var context = _fixture.NewContext())
        {
            var plain = new Assistance { Name = "Visit", Active = true, IsGrooming = false };
            var grooming = new Assistance { Name = "Bath", Active = true, IsGrooming = true };
            context.Assistances.AddRange(plain, grooming);
            await context.SaveChangesAsync();

            var companion = new Companion { Name = "Vet", ReferralCode = "RV1", OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", Active = true, Approved = true };
            var personal = new Companion { Name = "Solo", ReferralCode = "RS1", OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a", IsPersonal = true, Active = true, Approved = true };
            context.Companions.AddRange(companion, personal);
            await context.SaveChangesAsync();
            companionId = companion.Id;
            personalId = personal.Id;
            context.Set<CompanionType>().AddRange(
                new CompanionType { CompanionId = companion.Id, TypeId = 40 },
                new CompanionType { CompanionId = companion.Id, TypeId = 44 },
                new CompanionType { CompanionId = personal.Id, TypeId = 40 });

            var visit = new CompanionAssistance { CompanionId = companion.Id, AssistanceId = plain.Id, Active = true, Approved = true, CompanionTypeId = 40, CommissionPercent = 7 };
            var bath = new CompanionAssistance { CompanionId = companion.Id, AssistanceId = grooming.Id, Active = true, Approved = true, CompanionTypeId = 40 };
            context.CompanionAssistances.AddRange(visit, bath);
            await context.SaveChangesAsync();
            clinicAssistanceId = visit.Id;

            var noTypes = new CompanionAssistancePackage { Name = "Simple", Price = 300_000, PrePaymentPrice = 100_000, CompanionAssistanceId = visit.Id, Active = true };
            var withTypes = new CompanionAssistancePackage { Name = "Modes", Price = 120_000, PrePaymentPrice = 120_000, CompanionAssistanceId = visit.Id, Active = true };
            var groomPackage = new CompanionAssistancePackage { Name = "Groom", Price = 500_000, CompanionAssistanceId = bath.Id, Active = true };
            context.CompanionAssistancePackages.AddRange(noTypes, withTypes, groomPackage);
            await context.SaveChangesAsync();
            context.Set<CompanionAssistancePackageType>().AddRange(
                new CompanionAssistancePackageType { CompanionAssistancePackageId = withTypes.Id, CompanionAssistanceTypeId = 37, Price = 120_000, PrePaymentPrice = 120_000 },
                new CompanionAssistancePackageType { CompanionAssistancePackageId = withTypes.Id, CompanionAssistanceTypeId = 38, Price = 200_000, PrePaymentPrice = 40_000 });
            context.CompanionTimes.Add(new CompanionTime { CompanionId = companion.Id, WeekDayId = 1, StartTime = "9:00", EndTime = "17:00", Capacity = 3, Active = true });
            await context.SaveChangesAsync();
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO CodeCompanionAssistance (CodesId, CompanionAssistancesId) VALUES (38, {0}), (39, {0})", clinicAssistanceId);
        }

        RunScript();
        RunScript(); // idempotent

        using (var context = _fixture.NewContext())
        {
            var clinics = await context.Clinics.AsNoTracking().Where(c => c.LegacyCompanionId == companionId || c.LegacyCompanionId == personalId).ToListAsync();
            var clinic = Assert.Single(clinics); // companion شخصی منتقل نمی‌شود، دوباره‌اجرا هم کپی نمی‌سازد
            Assert.Equal(companionId, clinic.LegacyCompanionId);
            Assert.True(clinic.ServiceInPerson);
            Assert.True(clinic.ServiceAtHome);
            Assert.True(clinic.ServiceOnline); // بسته‌ی Modes حالت آنلاین دارد

            Assert.Equal(2, await context.ClinicTypes.CountAsync(t => t.ClinicId == clinic.Id));
            var services = await context.ClinicServices.AsNoTracking().Where(s => s.ClinicId == clinic.Id).ToListAsync();
            var service = Assert.Single(services); // خدمت گرومینگ منتقل نشد
            Assert.Equal(7, service.CommissionPercent);

            var packages = await context.ClinicServicePackages.AsNoTracking().Where(p => p.ClinicServiceId == service.Id).OrderBy(p => p.Price).ToListAsync();
            Assert.Equal(2, packages.Count);
            var simple = packages.Single(p => p.Name == "Simple");
            var simpleModes = await context.ClinicServicePackageTypes.AsNoTracking().Where(t => t.ClinicServicePackageId == simple.Id).OrderBy(t => t.Mode).ToListAsync();
            Assert.Equal(new[] { 38, 39 }, simpleModes.Select(t => t.Mode).ToArray()); // حالت‌های خدمت با قیمت پایه
            Assert.All(simpleModes, t => Assert.Equal(300_000, t.Price));
            var modes = packages.Single(p => p.Name == "Modes");
            var modeRows = await context.ClinicServicePackageTypes.AsNoTracking().Where(t => t.ClinicServicePackageId == modes.Id).OrderBy(t => t.Mode).ToListAsync();
            Assert.Equal(new[] { 37, 38 }, modeRows.Select(t => t.Mode).ToArray());
            Assert.Equal(40_000, modeRows.Single(t => t.Mode == 38).PrePaymentPrice);

            var times = await context.ClinicTimes.AsNoTracking().Where(t => t.ClinicId == clinic.Id).ToListAsync();
            Assert.Equal(3, Assert.Single(times).Capacity);
        }
    }
}
