using System;
using System.Text;

namespace Application.Common.Helpers
{
    /// <summary>
    /// ابزارهای «شنیداری» جستجو: کاربر اسم خارجی را هر جور که می‌شنود می‌نویسد («مکسی» به‌جای «ماکسی»، «گسترو» به‌جای
    /// «گاسترو»، «maxi» به‌جای «ماکسی»). همه‌ی ورودی‌ها باید از قبل با <see cref="SearchNormalizeHelper.Normalize"/> نرمال شده باشند.
    /// </summary>
    public static class SearchPhonetics
    {
        public static bool IsVowel(char c) => c is 'ا' or 'و' or 'ی';

        /// <summary>حرف‌های هم‌صدا یکی می‌شوند (ص/ث←س، ط←ت، ذ/ض/ظ←ز، ح←ه، غ←ق، ع←ا، ژ←ج) و حرف تکراری پشت‌سرهم یک‌بار می‌آید.</summary>
        public static string SoundKey(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;
            var sb = new StringBuilder(word.Length);
            foreach (var raw in word)
            {
                var c = raw switch
                {
                    'ص' or 'ث' => 'س',
                    'ط' => 'ت',
                    'ذ' or 'ض' or 'ظ' => 'ز',
                    'ح' => 'ه',
                    'غ' => 'ق',
                    'ژ' => 'ج',
                    'ع' or 'ء' => 'ا',
                    'ئ' => 'ی',
                    'ـ' => '\0',
                    _ => raw
                };
                if (c == '\0') continue;
                if (sb.Length > 0 && sb[^1] == c) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>اسکلت بی‌صدا: همان SoundKey بدون «ا/و/ی». فقط برای تطبیق کلمه‌ی لاتین با معادل فارسی‌اش.</summary>
        public static string ConsonantCode(string soundKey)
        {
            var sb = new StringBuilder(soundKey.Length);
            foreach (var c in soundKey)
            {
                if (IsVowel(c)) continue;
                if (sb.Length > 0 && sb[^1] == c) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// آیا <paramref name="word"/> همان <paramref name="token"/> (یا ابتدای آن، وقتی <paramref name="allowPrefix"/>) است با این
        /// تفاوت که یکی از دو طرف ۱ یا ۲ «ا/و/ی» میانی بیشتر دارد؟ عمداً سخت‌گیر است تا کلمه‌ی دیگری جا نزند:
        /// حرف اول و آخر باید یکی باشند، جابه‌جایی مصوت قبول نیست («کاسه» ≠ «کیسه») و مصوت اضافه فقط در یک طرف است («سالم» ≠ «سلام»).
        /// </summary>
        public static bool IsVowelVariant(string word, string token, bool allowPrefix)
        {
            return ExtraVowels(word, token, allowPrefix, extraInToken: true)
                || ExtraVowels(word, token, allowPrefix, extraInToken: false);
        }

        private static bool ExtraVowels(string word, string token, bool allowPrefix, bool extraInToken)
        {
            int i = 0, j = 0, skips = 0;
            while (i < word.Length && j < token.Length)
            {
                if (word[i] == token[j]) { i++; j++; continue; }
                if (i == 0 || skips == 2) return false;
                if (extraInToken)
                {
                    if (!IsVowel(token[j])) return false;
                    j++;
                }
                else
                {
                    // حرف آخرِ کلمه‌ی کاربر حذف‌شدنی نیست
                    if (!IsVowel(word[i]) || i == word.Length - 1) return false;
                    i++;
                }
                skips++;
            }
            if (i < word.Length || skips == 0) return false;
            return allowPrefix || j == token.Length;
        }

        /// <summary>فاصله‌ی ویرایشی (با جابه‌جایی دو حرف کنار هم = ۱). اگر از <paramref name="max"/> بیشتر شود max+1 برمی‌گرداند.</summary>
        public static int EditDistance(string a, string b, int max)
        {
            if (Math.Abs(a.Length - b.Length) > max) return max + 1;
            var prev2 = new int[b.Length + 1];
            var prev = new int[b.Length + 1];
            var cur = new int[b.Length + 1];
            for (var j = 0; j <= b.Length; j++) prev[j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                cur[0] = i;
                var rowMin = cur[0];
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    var value = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                        value = Math.Min(value, prev2[j - 2] + 1);
                    cur[j] = value;
                    if (value < rowMin) rowMin = value;
                }
                if (rowMin > max) return max + 1;
                (prev2, prev, cur) = (prev, cur, prev2);
            }
            return prev[b.Length];
        }

        public static bool IsLatinWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            foreach (var c in word)
                if (c is < 'a' or > 'z') return false;
            return true;
        }

        public static bool HasDigit(string word)
        {
            foreach (var c in word)
                if (char.IsDigit(c)) return true;
            return false;
        }

        /// <summary>نویسه‌گردانی تقریبی لاتین ← فارسی («maxi» ← «ماکسی»، «royal» ← «رویال»). دقیق نیست؛ خروجی‌اش بعداً با واژگان واقعی تطبیق داده می‌شود.</summary>
        public static string LatinToPersian(string word)
        {
            var sb = new StringBuilder(word.Length + 2);
            for (var i = 0; i < word.Length; i++)
            {
                var c = word[i];
                var next = i + 1 < word.Length ? word[i + 1] : '\0';
                var two = next == '\0' ? null : string.Concat(c, next);
                var digraph = two switch
                {
                    "sh" => "ش",
                    "ch" => "چ",
                    "kh" => "خ",
                    "gh" => "ق",
                    "ph" => "ف",
                    "th" => "ت",
                    "ck" => "ک",
                    "oo" or "ou" => "و",
                    "ee" or "ea" => "ی",
                    _ => null
                };
                if (digraph != null)
                {
                    if (i == 0 && IsVowel(digraph[0])) sb.Append('ا');
                    sb.Append(digraph);
                    i++;
                    continue;
                }

                if (i == 0 && (c is 'a' or 'e' or 'i' or 'o' or 'u'))
                {
                    sb.Append(c switch { 'i' => "ای", 'u' => "او", 'o' => "او", _ => "ا" });
                    continue;
                }

                sb.Append(c switch
                {
                    'a' => "ا",
                    'b' => "ب",
                    'c' => next is 'e' or 'i' or 'y' ? "س" : "ک",
                    'd' => "د",
                    'e' => "",
                    'f' => "ف",
                    'g' => "گ",
                    'h' => "ه",
                    'i' => "ی",
                    'j' => "ج",
                    'k' => "ک",
                    'l' => "ل",
                    'm' => "م",
                    'n' => "ن",
                    'o' => "و",
                    'p' => "پ",
                    'q' => "ک",
                    'r' => "ر",
                    's' => "س",
                    't' => "ت",
                    'u' => "و",
                    'v' => "و",
                    'w' => "و",
                    'x' => "کس",
                    'y' => "ی",
                    'z' => "ز",
                    _ => ""
                });
            }
            return sb.ToString();
        }

        private const string EnglishKeys = "qwertyuiop[]asdfghjkl;'zxcvbnm,";
        private const string PersianKeys = "ضصثقفغعهخحجچشسیبلاتنمکگظطزرذدئو";

        /// <summary>کاربر یادش رفته زبان کیبورد را عوض کند: «vd,dhg» ← «رویال».</summary>
        public static string EnglishKeyboardToPersian(string value) => MapKeys(value, EnglishKeys, PersianKeys);

        /// <summary>برعکس: «قخغشم» ← «royal».</summary>
        public static string PersianKeyboardToEnglish(string value) => MapKeys(value, PersianKeys, EnglishKeys);

        private static string MapKeys(string value, string from, string to)
        {
            var sb = new StringBuilder(value.Length);
            foreach (var c in value.ToLowerInvariant())
            {
                var index = from.IndexOf(c);
                sb.Append(index >= 0 ? to[index] : c);
            }
            return sb.ToString();
        }
    }
}
