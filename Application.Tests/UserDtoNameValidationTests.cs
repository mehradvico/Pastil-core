using System.ComponentModel.DataAnnotations;
using System.Linq;
using Application.Services.Dto;
using Xunit;

namespace Application.Tests;

public class UserDtoNameValidationTests
{
    private static UserDto BuildDto(string firstName, string lastName) => new()
    {
        Mobile = "09120000000",
        FirstName = firstName,
        LastName = lastName,
        TwoFactorEnabled = false,
        Locked = false,
        RoleId = 1,
    };

    private static bool IsFirstNameValid(string firstName)
    {
        var dto = BuildDto(firstName, "موسوی");
        var results = new System.Collections.Generic.List<ValidationResult>();
        Validator.TryValidateProperty(
            dto.FirstName,
            new ValidationContext(dto) { MemberName = nameof(UserDto.FirstName) },
            results);
        return !results.Any();
    }

    [Theory]
    [InlineData("رضا")]
    [InlineData("محمد علی")]
    public void FirstName_AcceptsPersianLetters(string firstName)
    {
        Assert.True(IsFirstNameValid(firstName));
    }

    [Theory]
    [InlineData("�/9D�")] // بایت‌های مخدوش/mojibake که هرگز نباید به‌عنوان نام ذخیره شوند
    [InlineData("Reza")] // نام انگلیسی دیگر مجاز نیست - کاربر فقط باید بتواند فارسی وارد کند
    [InlineData("Ali Reza")]
    [InlineData("E-E")]
    [InlineData("12345")]
    [InlineData("a")] // کوتاه‌تر از حداقل مجاز
    [InlineData("😀😀")]
    public void FirstName_RejectsNonPersianOrGarbledInput(string firstName)
    {
        Assert.False(IsFirstNameValid(firstName));
    }
}
