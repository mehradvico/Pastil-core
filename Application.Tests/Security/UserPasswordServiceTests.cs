using Application.Common.Security;
using Entities.Entities.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Application.Tests.Security;

public class UserPasswordServiceTests
{
    [Fact]
    public void HashPassword_CreatesSaltedHash_AndVerifiesCorrectPassword()
    {
        var service = CreateService();
        var user = new User { Id = 1 };

        var firstHash = service.HashPassword(user, "Test123!");
        var secondHash = service.HashPassword(user, "Test123!");
        var verification = service.VerifyPassword(
            user,
            firstHash,
            "Test123!");

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(verification.Succeeded);
        Assert.False(verification.NeedsRehash);
    }

    [Fact]
    public void VerifyPassword_RejectsIncorrectPassword()
    {
        var service = CreateService();
        var user = new User { Id = 2 };
        var hash = service.HashPassword(user, "Correct123!");

        var verification = service.VerifyPassword(
            user,
            hash,
            "Incorrect123!");

        Assert.False(verification.Succeeded);
        Assert.False(verification.NeedsRehash);
    }

    [Fact]
    public void VerifyPassword_AcceptsLegacySha256_AndRequestsRehash()
    {
        var service = CreateService();
        var user = new User { Id = 3 };
        var password = "Legacy123!";
        var legacyHash = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));

        var verification = service.VerifyPassword(
            user,
            legacyHash,
            password);

        Assert.True(verification.Succeeded);
        Assert.True(verification.NeedsRehash);
    }

    [Fact]
    public void VerifyPassword_WhenPepperIsEnabled_UpgradesUnpepperedHash()
    {
        var user = new User { Id = 4 };
        var password = "Pepper123!";
        var unpepperedHash = CreateService().HashPassword(user, password);
        var pepperedService = CreateService("server-only-secret");

        var verification = pepperedService.VerifyPassword(
            user,
            unpepperedHash,
            password);

        Assert.True(verification.Succeeded);
        Assert.True(verification.NeedsRehash);
    }

    private static UserPasswordService CreateService(string pepper = "")
    {
        var options = Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 10_000
        });
        var hasher = new PasswordHasher<User>(options);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Security:PasswordPepper"] = pepper
            })
            .Build();

        return new UserPasswordService(hasher, configuration);
    }
}
