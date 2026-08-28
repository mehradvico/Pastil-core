using System;
using System.Security.Cryptography;

namespace Application.Services.Accounting.PetTagSrv
{
    public static class PetTagCodeGenerator
    {
        private const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Digits = "23456789";
        private const string Characters = Letters + Digits;

        public static string Create(int length = 24)
        {
            if (length < 8 || length > 40)
                throw new ArgumentOutOfRangeException(nameof(length));

            var characters = new char[length];
            characters[0] = Letters[RandomNumberGenerator.GetInt32(Letters.Length)];
            characters[1] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];

            for (var index = 2; index < characters.Length; index++)
                characters[index] = Characters[RandomNumberGenerator.GetInt32(Characters.Length)];

            for (var index = characters.Length - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
            }

            return new string(characters);
        }
    }
}
