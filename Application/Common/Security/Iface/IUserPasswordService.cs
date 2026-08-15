using Entities.Entities.Security;

namespace Application.Common.Security.Iface
{
    public interface IUserPasswordService
    {
        string HashPassword(User user, string password);

        UserPasswordVerificationResult VerifyPassword(
            User user,
            string storedHash,
            string providedPassword);
    }

    public readonly record struct UserPasswordVerificationResult(
        bool Succeeded,
        bool NeedsRehash);
}
