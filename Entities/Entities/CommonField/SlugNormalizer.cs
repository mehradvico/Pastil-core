using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.Entities.CommonField
{
    public static class SlugNormalizer
    {
        public static string Normalize(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

            var slug = new StringBuilder(label.Length);
            var separatorPending = false;

            foreach (var character in label.Trim())
            {
                if (IsAsciiLetterOrDigit(character))
                {
                    if (separatorPending && slug.Length > 0)
                        slug.Append('-');

                    slug.Append(char.ToLowerInvariant(character));
                    separatorPending = false;
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    throw new ValidationException(
                        "Label فقط می‌تواند شامل حروف انگلیسی و اعداد باشد.");
                }

                // Whitespace, repeated separators and extra punctuation are
                // normalized to one hyphen instead of causing an error.
                separatorPending = slug.Length > 0;
            }

            if (slug.Length == 0)
            {
                throw new ValidationException(
                    "Label باید حداقل یک حرف انگلیسی یا عدد داشته باشد.");
            }

            return slug.ToString();
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return character is >= 'a' and <= 'z'
                or >= 'A' and <= 'Z'
                or >= '0' and <= '9';
        }
    }
}
