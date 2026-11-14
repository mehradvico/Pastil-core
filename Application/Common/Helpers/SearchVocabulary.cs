using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Common.Helpers
{
    /// <summary>نتیجه‌ی هوشمندسازی یک کلمه از عبارت جستجو.</summary>
    public sealed class SearchWordPlan
    {
        /// <summary>کلمه‌ی نرمال‌شده‌ی کاربر.</summary>
        public string Word { get; init; }

        /// <summary>هیچ معادلی (حتی با غلط املایی) در کاتالوگ ندارد.</summary>
        public bool Unmatched { get; init; }

        /// <summary>کلمه‌ی کاملِ کاتالوگ (یا ابتدای آن) است یا معادل واقعی دارد؛ نه فقط تکه‌ای اتفاقی وسط کلمه‌ای دیگر.</summary>
        public bool Strong { get; init; }

        /// <summary>اگر کلمه‌ی کاربر در کاتالوگ نبود و با کلمه‌ی دیگری جایگزین شد، همان جایگزین؛ وگرنه null.</summary>
        public string Corrected { get; init; }

        /// <summary>کلمه‌های نرمال‌شده‌ی معادل (غیر از خودِ Word) که واقعاً در کاتالوگ هستند.</summary>
        public IReadOnlyList<string> Tokens { get; init; } = Array.Empty<string>();

        /// <summary>
        /// عبارت‌های LIKE برای دیتابیس: «یا»ی بین آیتم‌ها و «و»ی بین اجزای هر آیتم. شکلِ خامِ ذخیره‌شده در دیتابیس
        /// (مثلاً «آرایش» با مد) را نگه می‌دارد تا به collation وابسته نباشیم.
        /// </summary>
        public IReadOnlyList<string[]> LikeTerms { get; init; } = Array.Empty<string[]>();
    }

    public sealed class SearchQueryPlan
    {
        public string NormalizedQuery { get; init; } = string.Empty;
        public IReadOnlyList<SearchWordPlan> Words { get; init; } = Array.Empty<SearchWordPlan>();

        /// <summary>
        /// کلمه‌هایی که هیچ معادلی در کاتالوگ ندارند و می‌شود کنارشان گذاشت تا نتیجه خالی نماند. فقط وقتی پر می‌شود که
        /// کلمه‌های باقی‌مانده «قوی» باشند؛ وگرنه «هپی کت» به هر چیزی که «کت» دارد تبدیل می‌شد.
        /// </summary>
        public IReadOnlyList<string> IgnoredWords { get; init; } = Array.Empty<string>();

        /// <summary>
        /// عبارتی که عملاً جستجو شد، فقط وقتی با نوشته‌ی کاربر فرق دارد (غلط املایی، کیبورد اشتباه یا کلمه‌ی کنارگذاشته)؛ وگرنه null.
        /// </summary>
        public string CorrectedQuery { get; init; }
    }

    /// <summary>
    /// واژگان واقعی کاتالوگ (کلمه‌های نام محصول/برند/مرکز...). هر کلمه‌ی جستجو فقط به کلمه‌هایی گسترش می‌یابد که واقعاً
    /// در کاتالوگ هستند؛ پس هم «مکسی» محصول «ماکسی» را می‌آورد و هم چیزی بی‌ربط به نتیجه اضافه نمی‌شود.
    /// </summary>
    public sealed class SearchVocabulary
    {
        private const int MaxWords = 8;
        private const int MaxVariants = 4;
        private const int MaxCorrections = 2;
        private const int MaxWordCache = 20_000;

        private static readonly Regex RawSplit = new(@"[^\p{L}\p{N}\p{M}]+", RegexOptions.Compiled);

        public static readonly SearchVocabulary Empty = Build(Array.Empty<string>());

        private readonly string[] _tokens;
        private readonly string[] _keys;
        private readonly int[] _counts;
        private readonly bool[] _latin;
        private readonly string[][] _raws;
        private readonly Dictionary<string, int> _index;
        private readonly Lazy<Dictionary<string, List<int>>> _byCode;
        private readonly ConcurrentDictionary<string, SearchWordPlan> _wordCache = new();

        public bool IsEmpty => _tokens.Length == 0;
        public int Count => _tokens.Length;

        private SearchVocabulary(List<(string Token, int Count, string[] Raws)> entries)
        {
            _tokens = new string[entries.Count];
            _keys = new string[entries.Count];
            _counts = new int[entries.Count];
            _latin = new bool[entries.Count];
            _raws = new string[entries.Count][];
            _index = new Dictionary<string, int>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                _tokens[i] = entries[i].Token;
                _keys[i] = SearchPhonetics.SoundKey(entries[i].Token);
                _counts[i] = entries[i].Count;
                _latin[i] = SearchPhonetics.IsLatinWord(entries[i].Token);
                _raws[i] = entries[i].Raws;
                _index[entries[i].Token] = i;
            }
            _byCode = new Lazy<Dictionary<string, List<int>>>(BuildCodeIndex);
        }

        public static SearchVocabulary Build(IEnumerable<string> texts)
        {
            var normalizedByRaw = new Dictionary<string, string>();
            var data = new Dictionary<string, (int Count, Dictionary<string, int> Raws)>();

            foreach (var text in texts)
            {
                if (string.IsNullOrWhiteSpace(text)) continue;
                foreach (var raw in RawSplit.Split(text))
                {
                    if (raw.Length is < 2 or > 40) continue;
                    if (!normalizedByRaw.TryGetValue(raw, out var token))
                    {
                        token = SearchNormalizeHelper.Normalize(raw);
                        normalizedByRaw[raw] = token;
                    }
                    if (token.Length < 2 || token.Contains(' ')) continue;

                    if (!data.TryGetValue(token, out var entry))
                        entry = (0, new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
                    var rawLike = ToLikeForm(raw);
                    entry.Raws[rawLike] = entry.Raws.GetValueOrDefault(rawLike) + 1;
                    data[token] = (entry.Count + 1, entry.Raws);
                }
            }

            var entries = data
                .Select(item => (item.Key, item.Value.Count,
                    item.Value.Raws.OrderByDescending(raw => raw.Value).Take(3).Select(raw => raw.Key).ToArray()))
                .OrderByDescending(item => item.Count)
                .ToList();
            return new SearchVocabulary(entries);
        }

        // همان تبدیلی که کوئری لیست محصولات روی ستون انجام می‌دهد (REPLACE ي/ك)
        private static string ToLikeForm(string raw) => raw.Replace('ي', 'ی').Replace('ك', 'ک');

        private Dictionary<string, List<int>> BuildCodeIndex()
        {
            var map = new Dictionary<string, List<int>>();
            for (var i = 0; i < _tokens.Length; i++)
            {
                if (_latin[i] || SearchPhonetics.HasDigit(_tokens[i])) continue;
                var code = SearchPhonetics.ConsonantCode(_keys[i]);
                if (code.Length < 3) continue;
                if (!map.TryGetValue(code, out var list)) map[code] = list = new List<int>();
                list.Add(i);
            }
            return map;
        }

        public SearchQueryPlan Plan(string rawQuery)
        {
            var normalized = SearchNormalizeHelper.Normalize(rawQuery);
            var words = SearchNormalizeHelper.AllWordTerms(rawQuery, MaxWords);
            if (IsEmpty || words.Length == 0)
                return Passthrough(normalized, words);

            var converted = false;
            if (!words.All(IsDirect))
            {
                var swapped = TryKeyboardSwap(rawQuery);
                if (swapped != null)
                {
                    words = swapped;
                    converted = true;
                }
            }

            var plans = words.Select(ExpandWord).ToList();

            var unmatched = plans.Where(plan => plan.Unmatched).ToList();
            var kept = plans.Where(plan => !plan.Unmatched).ToList();
            var canIgnore = unmatched.Count > 0
                && kept.Count >= unmatched.Count
                && kept.Any(plan => plan.Strong)
                && kept.Sum(plan => plan.Word.Length) >= 4;
            var ignored = canIgnore ? unmatched.Select(plan => plan.Word).ToList() : new List<string>();

            var effective = plans.Where(plan => !ignored.Contains(plan.Word)).Select(plan => plan.Corrected ?? plan.Word).ToList();
            var changed = converted || ignored.Count > 0 || plans.Any(plan => plan.Corrected != null);

            return new SearchQueryPlan
            {
                NormalizedQuery = converted ? string.Join(" ", words) : normalized,
                Words = plans,
                IgnoredWords = ignored,
                CorrectedQuery = changed && effective.Count > 0 ? string.Join(" ", effective) : null
            };
        }

        private static SearchQueryPlan Passthrough(string normalized, string[] words) => new()
        {
            NormalizedQuery = normalized,
            Words = words.Select(word => new SearchWordPlan { Word = word, LikeTerms = OwnLikeTerms(word) }).ToList()
        };

        private static List<string[]> OwnLikeTerms(string word)
        {
            var terms = new List<string[]> { new[] { word } };
            if (SearchPhonetics.HasDigit(word))
                terms.Add(new[] { SearchNormalizeHelper.ToPersianDigits(word) });
            return terms;
        }

        private bool IsDirect(string word)
        {
            if (_index.ContainsKey(word)) return true;
            foreach (var token in _tokens)
                if (token.Contains(word)) return true;
            return false;
        }

        private bool IsTokenStart(string word)
        {
            if (_index.ContainsKey(word)) return true;
            if (word.Length < 3) return false;
            foreach (var token in _tokens)
                if (token.StartsWith(word, StringComparison.Ordinal)) return true;
            return false;
        }

        // زبان کیبورد اشتباه بوده: فقط وقتی قبول می‌شود که «همه‌ی» کلمه‌های تبدیل‌شده واقعاً در کاتالوگ باشند
        private string[] TryKeyboardSwap(string rawQuery)
        {
            var hasLatin = rawQuery.Any(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z');
            var hasPersian = rawQuery.Any(c => c >= '؀' && c <= 'ۿ' && !char.IsDigit(c));
            if (hasLatin == hasPersian) return null;

            var swapped = hasLatin
                ? SearchPhonetics.EnglishKeyboardToPersian(rawQuery)
                : SearchPhonetics.PersianKeyboardToEnglish(rawQuery);
            var words = SearchNormalizeHelper.AllWordTerms(swapped, MaxWords);
            return words.Length > 0 && words.All(IsTokenStart) ? words : null;
        }

        private SearchWordPlan ExpandWord(string word)
        {
            if (_wordCache.TryGetValue(word, out var cached)) return cached;
            var plan = ExpandWordCore(word);
            if (_wordCache.Count < MaxWordCache) _wordCache[word] = plan;
            return plan;
        }

        private SearchWordPlan ExpandWordCore(string word)
        {
            var hasDigit = SearchPhonetics.HasDigit(word);
            var latin = SearchPhonetics.IsLatinWord(word);
            var key = SearchPhonetics.SoundKey(word);

            // ۱) خودِ کلمه در کاتالوگ هست؟ (و شکل‌های خامی که LIKE ساده پیدایشان نمی‌کند، مثل «آرایش» برای «ارایش»)
            var direct = false;
            var hiddenRaws = new List<string>();
            for (var i = 0; i < _tokens.Length; i++)
            {
                if (!_tokens[i].Contains(word)) continue;
                direct = true;
                if (hiddenRaws.Count >= 3) continue;
                foreach (var raw in _raws[i])
                {
                    if (raw.Contains(word, StringComparison.OrdinalIgnoreCase)) continue;
                    hiddenRaws.Add(raw);
                    break;
                }
            }

            // ۲) همان کلمه با «ا/و/ی» کمتر یا بیشتر: همیشه اضافه می‌شود، حتی وقتی خودِ کلمه هم نتیجه دارد
            var variants = !latin && !hasDigit ? VowelVariants(key, word) : new List<int>();

            var tokens = variants.Select(i => _tokens[i]).ToList();
            string[] split = null;
            string corrected = null;

            if (!direct)
            {
                if (tokens.Count > 0)
                {
                    corrected = tokens[0];
                }
                else if (!hasDigit)
                {
                    // ۳) کلمه در کاتالوگ نیست: فقط حالا سراغ حدس‌زدن می‌رویم
                    var guesses = latin ? GuessLatin(word, key) : GuessPersian(word, key, out split);
                    tokens = guesses.Select(i => _tokens[i]).ToList();
                    if (split != null) corrected = string.Join(" ", split);
                    else if (tokens.Count > 0) corrected = tokens[0];
                }
            }

            var likeTerms = OwnLikeTerms(word);
            likeTerms.AddRange(hiddenRaws.Select(raw => new[] { raw }));
            foreach (var token in tokens)
                likeTerms.AddRange(_raws[_index[token]].Take(2).Select(raw => new[] { raw }));
            if (split != null)
                likeTerms.Add(split.Select(part => _raws[_index[part]][0]).ToArray());

            var allTokens = split == null ? tokens : tokens.Concat(split).ToList();
            return new SearchWordPlan
            {
                Word = word,
                Unmatched = !direct && tokens.Count == 0 && split == null,
                Strong = tokens.Count > 0 || split != null || IsTokenStart(word),
                Corrected = corrected,
                Tokens = allTokens.Where(token => token != word).Distinct().ToList(),
                LikeTerms = Simplify(likeTerms)
            };
        }

        // عبارتِ تک‌جزئی که عبارت کوتاه‌تری را در خودش دارد اضافی است (LIKE '%ماکسی%' خودش «ماکسیمم» را هم می‌گیرد)
        private static List<string[]> Simplify(List<string[]> terms)
        {
            var singles = terms.Where(term => term.Length == 1).Select(term => term[0])
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(term => term.Length).ToList();
            var kept = new List<string>();
            foreach (var single in singles)
                if (!kept.Any(shorter => single.Contains(shorter, StringComparison.OrdinalIgnoreCase)))
                    kept.Add(single);

            var result = kept.Select(term => new[] { term }).ToList();
            foreach (var multi in terms.Where(term => term.Length > 1))
                if (!multi.Any(part => kept.Any(shorter => part.Contains(shorter, StringComparison.OrdinalIgnoreCase))))
                    result.Add(multi);
            return result;
        }

        private List<int> VowelVariants(string key, string word)
        {
            var found = new List<int>();
            if (key.Length < 4) return found;
            // هنگام تایپ («گاستر...») هم باید کار کند، ولی فقط برای کلمه‌ی به‌اندازه‌ی کافی بلند
            var allowPrefix = key.Length >= 5;
            var prefixes = new List<int>();
            for (var i = 0; i < _tokens.Length && found.Count < MaxVariants; i++)
            {
                var tokenKey = _keys[i];
                if (_latin[i] || tokenKey.Length < 4 || tokenKey[0] != key[0]) continue;
                if (_tokens[i].Contains(word)) continue;
                if (SearchPhonetics.IsVowelVariant(key, tokenKey, allowPrefix: false))
                    found.Add(i);
                else if (allowPrefix && prefixes.Count < MaxVariants && SearchPhonetics.IsVowelVariant(key, tokenKey, allowPrefix: true))
                    prefixes.Add(i);
            }
            // کلمه‌ی کامل («گاسترو») جلوتر از کلمه‌ای است که فقط ابتدایش جور است («گاسترواینتستینال»)
            return found.Concat(prefixes).Take(MaxVariants).ToList();
        }

        private List<int> GuessPersian(string word, string key, out string[] split)
        {
            split = SplitJoinedWord(word);
            return split != null ? new List<int>() : Typos(key, latin: false);
        }

        // «خوشخوراک» ← «خوش» + «خوراک» (نیم‌فاصله/فاصله‌ی جاافتاده)
        private string[] SplitJoinedWord(string word)
        {
            if (word.Length < 5) return null;
            string[] best = null;
            var bestScore = 0;
            for (var cut = 2; cut <= word.Length - 2; cut++)
            {
                var left = word[..cut];
                var right = word[cut..];
                if (!_index.TryGetValue(left, out var leftIndex) || !_index.TryGetValue(right, out var rightIndex)) continue;
                // تکه‌ی دوحرفی فقط وقتی قبول است که طرف دیگر کلمه‌ی کاملِ بلندی باشد
                if (Math.Min(left.Length, right.Length) < 3 && Math.Max(left.Length, right.Length) < 4) continue;
                var score = Math.Min(_counts[leftIndex], _counts[rightIndex]);
                if (score <= bestScore) continue;
                bestScore = score;
                best = new[] { left, right };
            }
            return best;
        }

        private List<int> Typos(string key, bool latin)
        {
            // کلمه‌ی کوتاه با یک حرف اختلاف معمولاً کلمه‌ی دیگری است («مینی»/«میشی»)؛ فقط از ۵ حرف به بالا حدس می‌زنیم
            // و برای ۵–۶ حرفی‌ها حرف اول هم باید یکی باشد.
            var found = new List<int>();
            if (key.Length < 5) return found;
            var max = key.Length >= 9 ? 2 : 1;
            var sameFirst = key.Length <= 6;
            var best = max + 1;
            for (var i = 0; i < _tokens.Length; i++)
            {
                var tokenKey = _keys[i];
                if (_latin[i] != latin || tokenKey.Length < 4 || SearchPhonetics.HasDigit(tokenKey)) continue;
                if (sameFirst && tokenKey[0] != key[0]) continue;
                if (Math.Abs(tokenKey.Length - key.Length) > max) continue;
                var distance = SearchPhonetics.EditDistance(key, tokenKey, Math.Min(max, best));
                if (distance > max || distance > best) continue;
                if (distance < best)
                {
                    best = distance;
                    found.Clear();
                }
                found.Add(i);
            }
            // واژگان به ترتیب پرتکرارترین مرتب است؛ هم‌حرفِ‌اول‌ها جلوتر می‌آیند
            return found
                .OrderByDescending(i => _keys[i][0] == key[0])
                .Take(MaxCorrections)
                .ToList();
        }

        private List<int> GuessLatin(string word, string key)
        {
            var persian = SearchPhonetics.SoundKey(SearchPhonetics.LatinToPersian(word));
            if (persian.Length >= 3)
            {
                // «maxi» ← «ماکسی»: خودِ نویسه‌گردانی یا همان با مصوت کمتر/بیشتر
                var exact = new List<int>();
                for (var i = 0; i < _tokens.Length && exact.Count < MaxCorrections; i++)
                    if (!_latin[i] && (_keys[i] == persian || (persian.Length >= 4 && _keys[i].StartsWith(persian, StringComparison.Ordinal))))
                        exact.Add(i);
                if (exact.Count > 0) return exact;

                var variants = VowelVariants(persian, persian);
                if (variants.Count > 0) return variants.Take(MaxCorrections).ToList();
            }

            var latinTypos = Typos(key, latin: true);
            if (latinTypos.Count > 0) return latinTypos;

            if (persian.Length >= 3)
            {
                var code = SearchPhonetics.ConsonantCode(persian);
                if (code.Length >= 3 && _byCode.Value.TryGetValue(code, out var sameCode))
                {
                    var sameStart = sameCode.Where(i => _keys[i][0] == persian[0]).Take(MaxCorrections).ToList();
                    if (sameStart.Count > 0) return sameStart;
                }

                return Typos(persian, latin: false);
            }
            return new List<int>();
        }
    }
}
