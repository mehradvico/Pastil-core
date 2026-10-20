using Application.Maping;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.ClinicField;
using Entities.Entities.LocationField;
using Entities.Entities.ProviderField;
using Entities.Entities.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Tests.Clinic;

/// <summary>
/// دیتابیس موقت LocalDB برای تست رفتار واقعی سرویس‌های کلینیک (ExecuteUpdate، ایندکس یکتا، شمارنده‌ها).
/// اگر LocalDB روی ماشین نباشد <see cref="Available"/> برابر false است و تست‌ها بدون خطا رد می‌شوند.
/// هر نمونه یک دیتابیس یکتا می‌سازد و در Dispose پاک می‌کند؛ کلیدهای خارجی عمداً خاموش‌اند (تست منطق، نه کاتالوگ‌ها).
/// </summary>
public sealed class ClinicLocalDbFixture : IDisposable
{
    private const string Server = @"(localdb)\MSSQLLocalDB";
    private readonly string _database = "ClinicTests_" + Guid.NewGuid().ToString("N");

    public bool Available { get; }
    public string ConnectionString => $"Server={Server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";
    public IMapper Mapper { get; }

    public ClinicLocalDbFixture()
    {
        Mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new AllMap());
            cfg.AddProfile(new ClinicMap());
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

        try
        {
            using var context = NewContext();
            context.Database.EnsureCreated();
            context.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
            Available = true;
        }
        catch (SqlException)
        {
            Available = false;
        }
    }

    public DataBaseContext NewContext()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer($"Server={Server};Database={_database};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5",
                sql => sql.UseNetTopologySuite())
            .Options;
        return new DataBaseContext(options);
    }

    /// <summary>داده‌ی پایه: استان/شهر، کاربر مالک + ارائه‌دهنده‌ی تأییدشده، یک حیوان و دو عکس.</summary>
    public SeedData Seed()
    {
        using var context = NewContext();
        var tag = Random.Shared.NextInt64(1_000_000, 9_999_999).ToString();
        var state = new State { Name = "S" };
        context.States.Add(state);
        context.SaveChanges();
        var city = new City { Name = "C", StateId = state.Id };
        var owner = new User { FirstName = "Owner", LastName = "One", Mobile = "0911" + tag };
        var other = new User { FirstName = "Other", LastName = "Two", Mobile = "0912" + tag };
        var liker = new User { FirstName = "Like", LastName = "Three", Mobile = "0913" + tag };
        context.Cities.Add(city);
        context.Users.AddRange(owner, other, liker);
        context.Pets.Add(new Pet { Name = "Dog", Label = "dog" + Guid.NewGuid().ToString("N"), Active = true });
        context.Pictures.AddRange(new Picture { Name = "p1", Url = "u", CreateDate = DateTime.Now }, new Picture { Name = "p2", Url = "u", CreateDate = DateTime.Now });
        context.SaveChanges();

        var provider = new Provider { UserId = owner.Id, Active = true, Approved = true, CreateDate = DateTime.Now };
        context.Providers.Add(provider);
        context.SaveChanges();

        return new SeedData(state.Id, city.Id, owner.Id, other.Id, liker.Id, provider.Id, owner.Mobile, other.Mobile,
            context.Pets.Select(p => p.Id).First(), context.Pictures.Select(p => p.Id).OrderBy(id => id).ToList());
    }

    public void Dispose()
    {
        try
        {
            using var context = NewContext();
            context.Database.EnsureDeleted();
        }
        catch (SqlException)
        {
            // دیتابیس موقت؛ اگر پاک نشد LocalDB خودش بعداً مدیریت می‌کند.
        }
    }

    public sealed record SeedData(long StateId, long CityId, long OwnerUserId, long OtherUserId, long LikerUserId,
        long ProviderId, string OwnerMobile, string OtherMobile, long PetId, IReadOnlyList<long> PictureIds);
}
