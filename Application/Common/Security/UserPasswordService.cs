using Application.Common.Security.Iface;
using Entities.Entities.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Security
{
    public sealed class UserPasswordService : IUserPasswordService
    {
        private const string CurrentHashPrefix = "$pastil$pbkdf2$v1$";

        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly string _pepper;

        public UserPasswordService(
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _passwordHasher = passwordHasher;
            _pepper = configuration["Security:PasswordPepper"]?.Trim() ?? string.Empty;
        }

        public string HashPassword(User user, string password)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var preparedPassword = PreparePassword(password);
            return CurrentHashPrefix + _passwordHasher.HashPassword(user, preparedPassword);
        }

        public UserPasswordVerificationResult VerifyPassword(
            User user,
            string storedHash,
            string providedPassword)
        {
            if (user == null ||
                string.IsNullOrWhiteSpace(storedHash) ||
                string.IsNullOrEmpty(providedPassword))
            {
                return new UserPasswordVerificationResult(false, false);
            }

            if (storedHash.StartsWith(CurrentHashPrefix, StringComparison.Ordinal))
            {
                var identityHash = storedHash[CurrentHashPrefix.Length..];
                var currentResult = VerifyIdentityHash(
                    user,
                    identityHash,
                    PreparePassword(providedPassword));

                if (currentResult != PasswordVerificationResult.Failed)
                {
                    return new UserPasswordVerificationResult(
                        true,
                        currentResult == PasswordVerificationResult.SuccessRehashNeeded);
                }

                // Allows enabling a pepper after hashes have already been created.
                if (!string.IsNullOrEmpty(_pepper) &&
                    VerifyIdentityHash(user, identityHash, providedPassword) !=
                    PasswordVerificationResult.Failed)
                {
                    return new UserPasswordVerificationResult(true, true);
                }

                return new UserPasswordVerificationResult(false, false);
            }

            // Compatibility with any unprefixed ASP.NET Identity hashes.
            var unprefixedResult = VerifyIdentityHash(user, storedHash, providedPassword);
            if (unprefixedResult != PasswordVerificationResult.Failed)
            {
                return new UserPasswordVerificationResult(true, true);
            }

            // Compatibility with Pastil's previous unsalted SHA-256 hashes.
            return VerifyLegacySha256(storedHash, providedPassword)
                ? new UserPasswordVerificationResult(true, true)
                : new UserPasswordVerificationResult(false, false);
        }

        private string PreparePassword(string password)
        {
            if (string.IsNullOrEmpty(_pepper))
            {
                return password;
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_pepper));
            return Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private PasswordVerificationResult VerifyIdentityHash(
            User user,
            string identityHash,
            string password)
        {
            try
            {
                return _passwordHasher.VerifyHashedPassword(
                    user,
                    identityHash,
                    password);
            }
            catch (FormatException)
            {
                return PasswordVerificationResult.Failed;
            }
            catch (CryptographicException)
            {
                return PasswordVerificationResult.Failed;
            }
        }

        private static bool VerifyLegacySha256(
            string storedHash,
            string providedPassword)
        {
            try
            {
                var storedBytes = Convert.FromBase64String(storedHash);
                var providedBytes = SHA256.HashData(
                    Encoding.UTF8.GetBytes(providedPassword));

                return storedBytes.Length == providedBytes.Length &&
                       CryptographicOperations.FixedTimeEquals(
                           storedBytes,
                           providedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
