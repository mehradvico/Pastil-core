using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Security;
using System.Security.Cryptography;
using Xunit;

namespace Application.Tests.Security;

[Collection("SensitiveDataProtector")] // کلید ایستا است؛ تست‌ها نباید هم‌زمان آن را عوض کنند
public class BankCardEncryptionTests
{
    private static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    [Fact]
    public void Protect_then_unprotect_round_trips_and_never_stores_plaintext()
    {
        Assert.True(SensitiveDataProtector.Configure(NewKey()));

        var cipher = SensitiveDataProtector.Protect("6037991234567890");

        Assert.StartsWith("enc:v1:", cipher);
        Assert.DoesNotContain("6037991234567890", cipher);
        Assert.Equal("6037991234567890", SensitiveDataProtector.Unprotect(cipher));
    }

    [Fact]
    public void Each_encryption_uses_a_fresh_nonce_but_the_blind_index_is_stable()
    {
        SensitiveDataProtector.Configure(NewKey());

        Assert.NotEqual(SensitiveDataProtector.Protect("6037991234567890"), SensitiveDataProtector.Protect("6037991234567890"));
        Assert.Equal(SensitiveDataProtector.BlindIndex("6037991234567890"), SensitiveDataProtector.BlindIndex("6037991234567890"));
        Assert.NotEqual(SensitiveDataProtector.BlindIndex("6037991234567890"), SensitiveDataProtector.BlindIndex("6037991234567891"));
        Assert.Equal(64, SensitiveDataProtector.BlindIndex("6037991234567890")!.Length);
    }

    [Fact]
    public void Blind_index_depends_on_the_key_and_is_not_a_plain_hash()
    {
        SensitiveDataProtector.Configure(NewKey());
        var first = SensitiveDataProtector.BlindIndex("6037991234567890");
        SensitiveDataProtector.Configure(NewKey());
        var second = SensitiveDataProtector.BlindIndex("6037991234567890");

        Assert.NotEqual(first, second);
        Assert.NotEqual(Convert.ToHexString(SHA256.HashData("6037991234567890"u8.ToArray())), first);
    }

    [Fact]
    public void Legacy_plaintext_values_are_read_unchanged_and_protect_is_idempotent()
    {
        SensitiveDataProtector.Configure(NewKey());

        Assert.Equal("6037991234567890", SensitiveDataProtector.Unprotect("6037991234567890"));
        var once = SensitiveDataProtector.Protect("IR120170000000123456789012");
        Assert.Equal(once, SensitiveDataProtector.Protect(once));
    }

    [Fact]
    public void Tampered_ciphertext_is_rejected()
    {
        SensitiveDataProtector.Configure(NewKey());
        var cipher = SensitiveDataProtector.Protect("6037991234567890");
        var payload = Convert.FromBase64String(cipher["enc:v1:".Length..]);
        payload[^1] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => SensitiveDataProtector.Unprotect("enc:v1:" + Convert.ToBase64String(payload)));
    }

    [Fact]
    public void Ciphertext_cannot_be_read_with_another_key()
    {
        SensitiveDataProtector.Configure(NewKey());
        var cipher = SensitiveDataProtector.Protect("6037991234567890");
        SensitiveDataProtector.Configure(NewKey());

        Assert.ThrowsAny<CryptographicException>(() => SensitiveDataProtector.Unprotect(cipher));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64!!")]
    [InlineData("c2hvcnQ=")] // فقط ۵ بایت
    public void Invalid_or_missing_key_disables_encryption_without_throwing(string? key)
    {
        Assert.False(SensitiveDataProtector.Configure(key!));
        Assert.False(SensitiveDataProtector.IsConfigured);
        Assert.Equal("6037991234567890", SensitiveDataProtector.Protect("6037991234567890"));
        Assert.Null(SensitiveDataProtector.BlindIndex("6037991234567890"));
    }

    [Fact]
    public void Reading_an_encrypted_value_without_a_key_fails_loudly()
    {
        SensitiveDataProtector.Configure(NewKey());
        var cipher = SensitiveDataProtector.Protect("6037991234567890");
        SensitiveDataProtector.Configure(null!);

        Assert.Throws<InvalidOperationException>(() => SensitiveDataProtector.Unprotect(cipher));
    }

    [Fact]
    public void DbContext_model_encrypts_card_number_and_sheba_and_indexes_the_hash()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True", x => x.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);

        var entity = context.Model.FindEntityType(typeof(UserBankCard))!;

        Assert.NotNull(entity.FindProperty(nameof(UserBankCard.CardNumber))!.GetValueConverter());
        Assert.NotNull(entity.FindProperty(nameof(UserBankCard.ShebaNumber))!.GetValueConverter());
        Assert.Equal(64, entity.FindProperty(nameof(UserBankCard.CardNumberHash))!.GetMaxLength());
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Any(p => p.Name == nameof(UserBankCard.CardNumberHash)));
    }

    [Fact]
    public void Card_number_converter_writes_ciphertext_and_reads_plaintext()
    {
        SensitiveDataProtector.Configure(NewKey());
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseSqlServer("Server=.;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True", x => x.UseNetTopologySuite())
            .Options;
        using var context = new DataBaseContext(options);
        var converter = context.Model.FindEntityType(typeof(UserBankCard))!.FindProperty(nameof(UserBankCard.CardNumber))!.GetValueConverter()!;

        var stored = (string)converter.ConvertToProvider("6037991234567890")!;

        Assert.StartsWith("enc:v1:", stored);
        Assert.Equal("6037991234567890", (string)converter.ConvertFromProvider(stored)!);
        Assert.Equal("6037991234567890", (string)converter.ConvertFromProvider("6037991234567890")!); // ردیف قدیمی
    }
}

[CollectionDefinition("SensitiveDataProtector", DisableParallelization = true)]
public class SensitiveDataProtectorCollection { }
