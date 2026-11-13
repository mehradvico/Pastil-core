using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Services.ProductSrvs.ProductSrv
{
    /// <summary>ردیف سبک یک محصول برای رتبه‌بندی «کالاهای مشابه» و «در کنارش خریداری شده».</summary>
    public sealed class ProductCandidate
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long Price { get; set; }
        public long StatusId { get; set; }
        public int SellCount { get; set; }
        public double RateAvg { get; set; }
        /// <summary>نام دسته‌ی برگ (مثل «بهداشت، مراقبت و نگهداری حیوانات»).</summary>
        public string LeafName { get; set; }
        public long? ParentCategoryId { get; set; }
        /// <summary>نام دسته‌ی والد = نوع حیوان (سگ / گربه / پرنده ...).</summary>
        public string ParentName { get; set; }
    }

    [Flags]
    public enum PetSpecies
    {
        None = 0,
        Dog = 1,
        Cat = 2,
        Bird = 4,
        Fish = 8,
        Small = 16,
        Reptile = 32,
    }

    public enum LifeStage { None = 0, Junior = 1, Adult = 2, Senior = 3 }

    /// <summary>ویژگی‌های استخراج‌شده از نام/دسته‌ی یک محصول.</summary>
    public sealed class ProductProfile
    {
        /// <summary>نوع کالا (litter / bed / pad / treat ...) یا null وقتی با اطمینان تشخیص داده نشد.</summary>
        public string Kind { get; set; }
        public PetSpecies Species { get; set; }
        public LifeStage Stage { get; set; }
        public HashSet<string> Tokens { get; set; } = new HashSet<string>();
    }

    /// <summary>
    /// موتور خالص (بدون دیتابیس) «کالاهای مشابه» و «در کنارش خریداری شده».
    /// اصل طراحی: اگر مطمئن نیستیم چیزی نشان نمی‌دهیم؛ نشان‌دادن کالای بی‌ربط بدتر از نشان‌ندادن است.
    ///  - مشابه = هم‌نوعِ دقیق (خاک گربه ↔ خاک گربه) با حیوان سازگار؛ هیچ‌وقت نوع یا حیوان متفاوت.
    ///  - در کنارش = نوع‌های «مکمل» طبق جدول (خاک گربه ← ظرف خاک، بیل، مت، بوگیر)، همان حیوان، فقط موجود.
    /// </summary>
    public static class ProductRelationEngine
    {
        public const long AvailableStatusId = 8; // ProductStatus_Available

        // ───────────── نرمال‌سازی و توکن ─────────────

        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var sb = new StringBuilder(input.Length);
            foreach (var raw in input)
            {
                var ch = raw;
                if (ch == 'ي' || ch == 'ى') ch = 'ی';
                else if (ch == 'ك') ch = 'ک';
                else if (ch == 'ة' || ch == 'ۀ') ch = 'ه';
                else if (ch == 'أ' || ch == 'إ' || ch == 'ٱ') ch = 'ا';
                else if (ch == '‌' || ch == '‏' || ch == '‎') { sb.Append(' '); continue; }
                else if (ch == 'ـ' || (ch >= 'ً' && ch <= 'ٟ')) continue;
                else if (ch >= '٠' && ch <= '٩') ch = (char)('0' + (ch - '٠'));
                else if (ch >= '۰' && ch <= '۹') ch = (char)('0' + (ch - '۰'));
                if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
                else sb.Append(' ');
            }
            return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string[] Tokenize(string input) =>
            Normalize(input).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // ───────────── حیوان ─────────────

        private static readonly Dictionary<string, PetSpecies> SpeciesWords = BuildSpeciesWords();

        private static Dictionary<string, PetSpecies> BuildSpeciesWords()
        {
            var map = new Dictionary<string, PetSpecies>();
            void Add(PetSpecies s, params string[] words) { foreach (var w in words) map[Normalize(w)] = s; }
            Add(PetSpecies.Cat, "گربه", "گربگان", "کیتن", "kitten", "cat", "cats");
            Add(PetSpecies.Dog, "سگ", "سگان", "توله", "puppy", "dog", "dogs");
            Add(PetSpecies.Bird, "پرنده", "پرندگان", "طوطی", "طوطیسانان", "قناری", "کاکادو", "فنچ", "کبوتر", "bird", "parrot", "budgie");
            Add(PetSpecies.Fish, "ماهی", "ماهیان", "آکواریوم", "fish", "aquarium");
            Add(PetSpecies.Small, "جونده", "جوندگان", "همستر", "خوکچه", "خرگوش", "rabbit", "hamster");
            Add(PetSpecies.Reptile, "خزنده", "خزندگان", "لاکپشت", "مارمولک");
            return map;
        }

        private static readonly HashSet<string> FlavorMarkers = new HashSet<string>(
            new[] { "طعم", "رایحه", "عصاره", "حاوی" }.Select(Normalize));

        private static PetSpecies SpeciesOf(string text)
        {
            var result = PetSpecies.None;
            var skip = 0;
            foreach (var token in Tokenize(text))
            {
                // «غذای سگ با طعم ماهی/پرندگان»: حیواناتِ بعد از واژه‌ی طعم، حیوان مقصد نیستند
                if (FlavorMarkers.Contains(token)) { skip = 4; continue; }
                if (skip > 0) { skip--; continue; }
                if (SpeciesWords.TryGetValue(token, out var s)) result |= s;
            }
            // «ماهی» تقریباً همیشه طعم/ماده‌ی غذایی است؛ فقط وقتی تنها حیوان است معتبر می‌داند
            if ((result & PetSpecies.Fish) != 0 && (result & ~PetSpecies.Fish) != 0 && !HasToken(text, "آکواریوم", "ماهیان"))
                result &= ~PetSpecies.Fish;
            // «خرگوش» در اسم اسباب‌بازی‌های سگ/گربه فقط شکل عروسک است، نه حیوانِ مقصد
            if ((result & PetSpecies.Small) != 0 && (result & (PetSpecies.Dog | PetSpecies.Cat)) != 0 && !HasToken(text, "جونده", "جوندگان", "همستر", "خوکچه"))
                result &= ~PetSpecies.Small;
            return result;
        }

        private static bool HasToken(string text, params string[] words)
        {
            var tokens = Tokenize(text);
            return words.Any(w => tokens.Contains(Normalize(w)));
        }

        public static IReadOnlyList<string> SpeciesLeafWords(PetSpecies species)
        {
            var words = new List<string>();
            if ((species & PetSpecies.Cat) != 0) words.Add("گربه");
            if ((species & PetSpecies.Dog) != 0) words.Add("سگ");
            if ((species & PetSpecies.Bird) != 0) words.Add("پرنده");
            if ((species & PetSpecies.Fish) != 0) words.Add("ماهی");
            if ((species & PetSpecies.Small) != 0) words.Add("جونده");
            return words;
        }

        public static bool SpeciesCompatible(PetSpecies a, PetSpecies b) =>
            a == PetSpecies.None || b == PetSpecies.None || (a & b) != 0;

        // ───────────── مرحله‌ی سنی ─────────────

        private static LifeStage StageOf(string text)
        {
            var tokens = Tokenize(text);
            bool Has(params string[] w) => w.Any(x => tokens.Contains(Normalize(x)));
            if (Has("توله", "کیتن", "kitten", "puppy", "نابالغ", "junior", "بچه")) return LifeStage.Junior;
            if (Has("سالمند", "سنیور", "senior")) return LifeStage.Senior;
            if (Has("بالغ", "ادالت", "adult")) return LifeStage.Adult;
            return LifeStage.None;
        }

        // ───────────── نوع کالا ─────────────

        private sealed class KindRule
        {
            public string Kind;
            public string[][] Phrases;
        }

        private static KindRule Rule(string kind, params string[] phrases) => new KindRule
        {
            Kind = kind,
            Phrases = phrases.Select(p => Normalize(p).Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToArray()
        };

        // ترتیب مهم است: اولین قاعده‌ی منطبق برنده است (مثلاً «مت ظرف خاک» باید قبل از «ظرف خاک» و «خاک» بیاید).
        private static readonly KindRule[] KindRules =
        {
            Rule("milk", "شیر خشک", "شیرخشک"),
            Rule("bottle", "قمقمه"),
            // خاک گربه و لوازمش — ترتیب مهم است
            Rule("litter_mat", "مت ظرف خاک", "مت خاک", "مت توالت", "زیرانداز خاک", "زیر انداز خاک", "زیرانداز ظرف خاک", "زیر انداز ظرف خاک", "مت دستشویی"),
            Rule("litter_deodorizer", "بوگیر خاک", "بو گیر خاک", "خوشبو کننده خاک", "ضد بو خاک"),
            Rule("litter_box", "ظرف خاک", "ظرف توالت", "توالت گربه", "توالت", "جعبه خاک", "لیتر باکس", "litter box", "litterbox", "دستشویی گربه", "ظرف لیتر", "جای خاک", "سینی خاک", "لگن"),
            Rule("litter_scoop", "بیل خاک", "بیلچه", "بیل", "اسکوپ", "scoop", "خاک انداز", "خاکانداز"),
            Rule("litter", "خاک گربه", "خاک", "ماسه گربه", "ماسه", "litter", "بنتونیت", "بنتونایت"),
            Rule("odor", "بوگیر", "بو گیر", "لکه بر"),
            Rule("pad", "پد بهداشتی", "پد توالت", "پد زیرانداز", "پد زیر انداز", "زیرانداز بهداشتی", "زیر انداز بهداشتی", "پد سگ", "پد آموزشی", "ترینینگ پد", "toilet pad", "training pad"),
            Rule("fountain", "فواره", "آبخوری اتوماتیک", "آب خوری اتوماتیک", "آبخوری برقی", "دیسپنسر آب", "آبریز"),
            Rule("scratcher", "خراشگر", "اسکرچر", "اسکراچر", "خراش", "درخت گربه", "scratcher", "scratching"),
            Rule("poop_bag", "کیسه مدفوع", "کیسه جمع", "پوپ", "poop", "جمع کننده مدفوع"),
            Rule("wipes", "دستمال مرطوب", "دستمال", "وایپ", "wipes", "wipe", "پد مرطوب", "پد پاک کننده", "پد تمیز کننده", "پد انگشتی"),
            Rule("nail", "ناخن گیر", "ناخنگیر", "سوهان", "ناخن"),
            Rule("feeding_bottle", "شیشه شیر", "شیشه‌شیر"),
            Rule("syringe", "سرنگ"),
            Rule("toy", "اسباب بازی", "عروسک"),
            Rule("treat", "تشویقی", "تشویفی"),
            Rule("flea", "ضد کک", "ضدکک", "کک", "کنه", "ضد انگل", "ضدانگل", "انگل"),
            Rule("supplement", "مکمل", "ویتامین", "مولتی", "کلسیم", "روغن ماهی", "پروبیوتیک", "ملاتونین", "مالت", "اسید چرب"),
            Rule("medicine", "قطره", "محلول", "قرص", "شربت", "پماد", "ژل", "براوکتو", "داروی", "دارو", "اسپری"),
            Rule("shampoo", "شامپو", "نرم کننده", "ماسک مو", "اسپری شستشو", "حمام"),
            Rule("toy", "اسباب بازی", "اسباب", "عروسک", "توپ", "دندانی", "فریزبی", "طناب", "جغجغه", "toy", "تشک بازی"),
            Rule("treat", "تشویقی", "تشویفی", "بیسکویت", "بیسکوئیت", "اسنک", "استیک", "ژرکی", "جرکی", "استخوان", "پنیر سگ", "پودینگ", "بستنی", "treat", "treats", "snack"),
            Rule("dental", "مسواک", "خمیر دندان", "بهداشت دهان", "دهان شویه"),
            Rule("brush", "برس", "شانه", "شونه", "فرمیناتور", "furminator", "ماشین اصلاح", "قیچی", "کلیپر"),
            Rule("training", "کلیکر", "سوت", "آموزش دهنده", "ترینینگ", "training"),
            Rule("harness", "هارنس", "harness"),
            Rule("tag", "پلاک"),
            Rule("collar", "قلاده", "گردنبند", "طوق", "collar"),
            Rule("leash", "بند هدایت", "بند قلاده", "افسار", "لیش", "leash"),
            Rule("clothes", "لباس", "پوشاک", "تیشرت", "تی شرت", "سرهمی", "جلیقه", "پیراهن", "بلوز", "تاپ", "پالتو", "کاپشن", "بارانی", "بارونی", "دورس", "نیم تنه", "حوله تنی", "هودی", "کفش", "بوت", "جوراب", "کلاه", "شلوار", "پلیور", "سویشرت", "ست لباس", "پافر"),
            Rule("bowl", "ظرف", "ظرب", "ظروف", "کاسه", "آبخوری", "آب خوری", "غذاخوری", "غذا خوری", "مت آب و غذا", "مت غذا", "مت ظرف", "قمقمه", "bowl"),
            Rule("bed", "جای خواب", "جایخواب", "تخت", "تختخواب", "بستر", "پتو", "کوسن", "تشک", "سرپناه", "لانه سگ", "لانه گربه", "خانه سگ", "خانه گربه"),
            Rule("carrier", "کیف حمل", "باکس حمل", "قفس حمل", "کیف", "ساک", "ترنسپورت", "کوله پشتی", "کوله حمل", "کوله فضایی", "باکس"),
            // پرنده/جونده
            Rule("nest", "لانه"),
            Rule("hideout", "مخفیگاه", "پناهگاه"),
            Rule("playground", "پارک بازی", "پارک"),
            Rule("cage_cover", "کاور قفس", "کاور"),
            Rule("bird_hanging", "آویز"),
            Rule("perch", "نشیمنگاه", "نردبان", "تاب", "میله", "پرچ", "چوب قفس"),
            Rule("cage", "قفس", "آکواریوم", "تراریوم", "کلبه"),
            Rule("wet_food", "کنسرو", "پوچ", "غذای مرطوب", "غذا مرطوب", "پاته", "ژله", "pouch"),
        };

        private static bool ContainsPhrase(string[] tokens, string[] phrase)
        {
            if (phrase.Length == 0 || tokens.Length < phrase.Length) return false;
            for (var i = 0; i <= tokens.Length - phrase.Length; i++)
            {
                var ok = true;
                for (var j = 0; j < phrase.Length; j++)
                    if (tokens[i + j] != phrase[j]) { ok = false; break; }
                if (ok) return true;
            }
            return false;
        }

        public static string KindOfText(string text)
        {
            var tokens = Tokenize(text);
            if (tokens.Length == 0) return null;
            foreach (var rule in KindRules)
                if (rule.Phrases.Any(p => ContainsPhrase(tokens, p)))
                    return rule.Kind;
            // «پد shawn 60*90» : واژه‌ی پد + ابعاد زیرانداز (و نه پد گوش/دندان/انگشتی که بالاتر گرفته شد)
            if (tokens.Contains("پد") && tokens.Any(t => t == "60" || t == "90" || t == "45" || t == "33"))
                return "pad";
            // غذای خشک: «غذا/غذای» + «خشک» (نه شیر خشک که بالاتر گرفته شد)
            var hasFood = tokens.Contains("غذا") || tokens.Contains("غذای");
            if (hasFood && tokens.Contains("خشک")) return "dry_food";
            if (hasFood) return "food";
            return null;
        }

        // فقط وقتی «دسته‌ی برگ» یک نوع کالای یکدست است از آن به‌عنوان نوع پیش‌فرض استفاده می‌شود
        // (برگ‌های «بهداشت، مراقبت و نگهداری» و «لانه و قفس» آشغال‌دانی‌اند و استفاده نمی‌شوند).
        private static readonly HashSet<string> SafeLeafKinds = new HashSet<string>
        {
            "dry_food", "wet_food", "treat", "toy", "pad", "shampoo", "clothes", "food", "supplement", "flea"
        };

        public static ProductProfile Profile(ProductCandidate p)
        {
            var name = ((p.Name ?? "") + " " + (p.SecondName ?? "")).Trim();
            var kind = KindOfText(name);
            if (kind == null)
            {
                var leafKind = KindOfText(p.LeafName);
                if (leafKind != null && SafeLeafKinds.Contains(leafKind)) kind = leafKind;
            }
            var species = SpeciesOf(name);
            if (species == PetSpecies.None) species = SpeciesOf(p.LeafName);
            if (species == PetSpecies.None) species = SpeciesOf(p.ParentName);
            var profile = new ProductProfile { Kind = kind, Species = species, Stage = StageOf(name) };
            foreach (var t in ContentTokens(name)) profile.Tokens.Add(t);
            return profile;
        }

        private static readonly HashSet<string> StopWords = new HashSet<string>(
            new[] { "مدل", "کد", "وزن", "کیلوگرم", "کیلو", "گرم", "عددی", "عدد", "مخصوص", "برای", "و", "با", "از", "در", "طرح", "رنگ", "سایز", "بسته", "پک",
                    "مناسب", "ها", "های", "یک", "کوچک", "بزرگ", "متوسط", "جثه", "نژاد", "kg", "gr", "ml", "سری", "نوع", "حیوانات", "خانگی", "pet", "پت", "کیلوگرمی", "گرمی" }
                .Select(Normalize));

        private static IEnumerable<string> ContentTokens(string text) =>
            Tokenize(text).Where(t => t.Length >= 2 && !StopWords.Contains(t) && !SpeciesWords.ContainsKey(t) && !t.Any(char.IsDigit)
                && t != "غذا" && t != "غذای");

        // ───────────── سازگاری نوع ─────────────

        private static readonly HashSet<string> FoodFamily = new HashSet<string> { "food", "dry_food", "wet_food" };

        public static bool KindsCompatible(string a, string b)
        {
            if (a == b) return true;
            // «food» عمومی با غذای خشک/مرطوب سازگار است اما خشک با مرطوب نه
            if (a == "food" && FoodFamily.Contains(b)) return true;
            if (b == "food" && FoodFamily.Contains(a)) return true;
            return false;
        }

        // ───────────── کالاهای مکمل (در کنارش خریداری شده) ─────────────

        private static readonly Dictionary<string, (string Kind, int Weight)[]> ComplementMap = new Dictionary<string, (string, int)[]>
        {
            ["litter"] = new[] { ("litter_box", 10), ("litter_scoop", 8), ("litter_mat", 6), ("litter_deodorizer", 6) },
            ["litter_box"] = new[] { ("litter", 10), ("litter_scoop", 8), ("litter_mat", 6), ("litter_deodorizer", 6) },
            ["litter_scoop"] = new[] { ("litter", 10), ("litter_box", 8), ("litter_deodorizer", 5) },
            ["litter_mat"] = new[] { ("litter_box", 9), ("litter", 8) },
            ["litter_deodorizer"] = new[] { ("litter", 10), ("litter_box", 7), ("litter_scoop", 6) },
            ["pad"] = new[] { ("bed", 8), ("treat", 8), ("wipes", 6), ("training", 5) },
            ["bed"] = new[] { ("pad", 8), ("treat", 7), ("toy", 6) },
            ["treat"] = new[] { ("toy", 8), ("training", 6), ("bowl", 5) },
            ["dry_food"] = new[] { ("bowl", 9), ("treat", 8), ("supplement", 6), ("wet_food", 5) },
            ["wet_food"] = new[] { ("bowl", 8), ("dry_food", 7), ("treat", 6) },
            ["food"] = new[] { ("bowl", 8), ("treat", 7), ("supplement", 5) },
            ["bowl"] = new[] { ("dry_food", 8), ("wet_food", 7), ("treat", 5), ("fountain", 4) },
            ["fountain"] = new[] { ("dry_food", 7), ("wet_food", 6) },
            ["toy"] = new[] { ("treat", 8), ("bed", 5) },
            ["collar"] = new[] { ("leash", 9), ("tag", 8), ("harness", 6), ("poop_bag", 4) },
            ["leash"] = new[] { ("collar", 9), ("harness", 7), ("poop_bag", 7), ("tag", 4) },
            ["harness"] = new[] { ("leash", 9), ("collar", 5), ("poop_bag", 5) },
            ["tag"] = new[] { ("collar", 9), ("leash", 5) },
            ["carrier"] = new[] { ("pad", 7), ("bowl", 6), ("bed", 5) },
            ["scratcher"] = new[] { ("toy", 7), ("treat", 6) },
            ["brush"] = new[] { ("shampoo", 8), ("nail", 6), ("wipes", 5), ("dental", 4) },
            ["shampoo"] = new[] { ("brush", 9), ("wipes", 6), ("nail", 5) },
            ["nail"] = new[] { ("brush", 8), ("shampoo", 5) },
            ["dental"] = new[] { ("treat", 8), ("toy", 5) },
            ["flea"] = new[] { ("shampoo", 8), ("brush", 6) },
            ["supplement"] = new[] { ("dry_food", 7), ("treat", 7) },
            ["clothes"] = new[] { ("harness", 8), ("bed", 5) },
            ["training"] = new[] { ("treat", 9), ("toy", 6), ("leash", 5) },
            ["poop_bag"] = new[] { ("leash", 9), ("collar", 5) },
            ["wipes"] = new[] { ("shampoo", 7), ("pad", 6) },
            ["cage"] = new[] { ("bowl", 8), ("perch", 7), ("toy", 7), ("nest", 5) },
            ["nest"] = new[] { ("cage", 7), ("perch", 6), ("toy", 5) },
            ["playground"] = new[] { ("toy", 8), ("perch", 6) },
            ["bird_hanging"] = new[] { ("toy", 6), ("perch", 6) },
            ["perch"] = new[] { ("toy", 8), ("hideout", 6), ("cage", 5) },
            ["hideout"] = new[] { ("perch", 7), ("toy", 7) },
        };

        // نوع‌هایی که فقط برای بعضی حیوان‌ها معنی دارند (پد توالت برای گربه/پرنده پیشنهاد نمی‌شود)
        private static readonly Dictionary<string, PetSpecies> KindSpecies = new Dictionary<string, PetSpecies>
        {
            ["pad"] = PetSpecies.Dog | PetSpecies.Small,
            ["scratcher"] = PetSpecies.Cat,
            ["litter"] = PetSpecies.Cat | PetSpecies.Small,
            ["litter_box"] = PetSpecies.Cat | PetSpecies.Small,
            ["litter_scoop"] = PetSpecies.Cat | PetSpecies.Small,
            ["litter_mat"] = PetSpecies.Cat | PetSpecies.Small,
            ["litter_deodorizer"] = PetSpecies.Cat | PetSpecies.Small,
            ["leash"] = PetSpecies.Dog | PetSpecies.Cat,
            ["harness"] = PetSpecies.Dog | PetSpecies.Cat | PetSpecies.Bird,
            ["poop_bag"] = PetSpecies.Dog,
        };

        private static bool KindFitsSpecies(string kind, PetSpecies species)
        {
            if (!KindSpecies.TryGetValue(kind, out var allowed)) return true;
            return species != PetSpecies.None && (species & allowed) != 0;
        }

        public static IReadOnlyList<(string Kind, int Weight)> Complements(string kind) =>
            kind != null && ComplementMap.TryGetValue(kind, out var list) ? list : Array.Empty<(string, int)>();

        // ───────────── کمکی‌های امتیازدهی ─────────────

        private static bool IsAvailable(ProductCandidate c) => c.StatusId == AvailableStatusId && c.Price > 0;

        private static double Dice(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0;
            var inter = a.Count(b.Contains);
            return 2.0 * inter / (a.Count + b.Count);
        }

        private static double Popularity(ProductCandidate c) =>
            Math.Min(4, Math.Log(1 + Math.Max(0, c.SellCount))) + Math.Max(0, Math.Min(5, c.RateAvg)) / 5.0 * 2;

        // برند «نامشخص/عمومی» (همان برندی که اکثر کاتالوگ را پر کرده) نشانه‌ی شباهت نیست
        private static HashSet<long> CommonBrands(IReadOnlyList<ProductCandidate> pool)
        {
            var common = new HashSet<long>();
            if (pool.Count < 20) return common;
            foreach (var g in pool.Where(p => p.BrandId.HasValue).GroupBy(p => p.BrandId.Value))
                if (g.Count() > pool.Count * 0.25) common.Add(g.Key);
            return common;
        }

        // ───────────── مشابه ─────────────

        public static List<long> Similar(ProductCandidate source, IReadOnlyList<ProductCandidate> pool, int take)
        {
            if (source == null || pool == null || take <= 0) return new List<long>();
            var src = Profile(source);
            var commonBrands = CommonBrands(pool);
            var scored = new List<(ProductCandidate Item, double Score, bool Available)>();

            foreach (var cand in pool)
            {
                if (cand.Id == source.Id) continue;
                var cp = Profile(cand);
                if (!SpeciesCompatible(src.Species, cp.Species)) continue;

                var sameLeaf = source.CategoryId.HasValue && cand.CategoryId == source.CategoryId;
                var sameParent = source.ParentCategoryId.HasValue && cand.ParentCategoryId == source.ParentCategoryId;
                if (!sameLeaf && !sameParent) continue;

                var sim = Dice(src.Tokens, cp.Tokens);
                double score = 0;

                if (src.Kind != null && cp.Kind != null)
                {
                    if (!KindsCompatible(src.Kind, cp.Kind)) continue;          // خاک گربه ≠ پد سگ، قطعی
                    score += 30;
                }
                else if (src.Kind != null || cp.Kind != null)
                {
                    // یکی‌شان نوعش مشخص نیست: فقط با شباهت نامِ بالا قبول می‌کنیم
                    if (sim < 0.4) continue;
                    score += 5;
                }
                else
                {
                    // هیچ‌کدام نوع نداشتند (مثلاً غذای برندی در برگِ غذا): دسته‌ی برگ یکی باشد و کمی شباهت نام
                    if (!sameLeaf || sim < 0.15) continue;
                    score += 12;
                }

                score += sameLeaf ? 20 : 6;
                score += sim * 25;
                if (source.BrandId.HasValue && cand.BrandId == source.BrandId && !commonBrands.Contains(source.BrandId.Value)) score += 8;
                if (source.Price > 0 && cand.Price > 0)
                {
                    var ratio = Math.Max(source.Price, cand.Price) / (double)Math.Min(source.Price, cand.Price);
                    score += 8 * Math.Max(0, 1 - Math.Log(ratio) / Math.Log(4));
                }
                if (src.Stage != LifeStage.None && cp.Stage != LifeStage.None)
                    score += src.Stage == cp.Stage ? 4 : -8;
                var available = IsAvailable(cand);
                if (available) score += 25;
                score += Popularity(cand);
                scored.Add((cand, score, available));
            }

            return scored
                .OrderByDescending(x => x.Available).ThenByDescending(x => x.Score).ThenBy(x => x.Item.Id)
                .Take(take).Select(x => x.Item.Id).ToList();
        }

        // ───────────── در کنارش خریداری شده ─────────────

        /// <param name="coPurchase">productId → تعداد سفارش‌های پرداخت‌شده‌ای که هم‌زمان با محصول مبدأ خریده شده</param>
        public static List<long> BoughtTogether(ProductCandidate source, IReadOnlyList<ProductCandidate> pool,
            IReadOnlyDictionary<long, int> coPurchase, int take)
        {
            if (source == null || pool == null || take <= 0) return new List<long>();
            coPurchase ??= new Dictionary<long, int>();
            var src = Profile(source);
            var complements = Complements(src.Kind).ToDictionary(c => c.Kind, c => c.Weight);
            var commonBrands = CommonBrands(pool);
            var byKind = new Dictionary<string, List<(ProductCandidate Item, double Score)>>();

            foreach (var cand in pool)
            {
                if (cand.Id == source.Id || !IsAvailable(cand)) continue;     // کالای ناموجود پیشنهاد خرید نیست
                var cp = Profile(cand);
                if (cp.Kind == null) continue;                                  // نوع نامشخص = پیشنهاد نمی‌دهیم
                if (!SpeciesCompatible(src.Species, cp.Species)) continue;
                if (!KindFitsSpecies(cp.Kind, cp.Species)) continue;
                if (src.Kind != null && KindsCompatible(src.Kind, cp.Kind)) continue; // هم‌نوع = «مشابه» است نه «مکمل»

                coPurchase.TryGetValue(cand.Id, out var support);
                var hasWeight = complements.TryGetValue(cp.Kind, out var weight);
                if (!hasWeight)
                {
                    // کالایی که در جدول مکمل نیست، فقط با شواهد واقعی خرید هم‌زمان (حداقل ۳ سفارش) و فقط وقتی نوع مبدأ مشخص نیست
                    if (src.Kind != null || support < 3) continue;
                    weight = 3;
                }

                var score = weight * 3 + Math.Min(15, support * 5) + Popularity(cand);
                if (source.BrandId.HasValue && cand.BrandId == source.BrandId && !commonBrands.Contains(source.BrandId.Value)) score += 2;
                if (src.Stage != LifeStage.None && cp.Stage != LifeStage.None && src.Stage != cp.Stage) score -= 6;

                if (!byKind.TryGetValue(cp.Kind, out var list)) byKind[cp.Kind] = list = new List<(ProductCandidate, double)>();
                list.Add((cand, score));
            }

            // هر نوع حداکثر ۲ کالا (۱ اگر جای کم است)؛ نوع‌ها به ترتیب وزن، دور اول بهترینِ هر نوع، دور دوم دومی‌ها
            var order = byKind.Keys.OrderByDescending(k => complements.TryGetValue(k, out var w) ? w : 0).ThenBy(k => k, StringComparer.Ordinal).ToList();
            foreach (var k in order) byKind[k] = byKind[k].OrderByDescending(x => x.Score).ThenBy(x => x.Item.Id).ToList();
            var result = new List<long>();
            var rounds = take <= 4 ? 1 : 2;
            for (var round = 0; round < rounds && result.Count < take; round++)
                foreach (var k in order)
                {
                    if (byKind[k].Count > round && result.Count < take)
                        result.Add(byKind[k][round].Item.Id);
                }
            return result;
        }

        // ───────────── «چیزی را فراموش نکردید؟» (پیشنهاد مکمل برای کل سبد) ─────────────

        public sealed class CartSuggestion
        {
            public long ProductId { get; set; }
            /// <summary>نوع کالای پیشنهادی (bowl / litter_box / pad ...) برای برچسب «ظرف غذا» در UI.</summary>
            public string Kind { get; set; }
            /// <summary>کالایی از سبد که این پیشنهاد به‌خاطرش آمده (برای متن «برای ...»).</summary>
            public long ForProductId { get; set; }
        }

        /// <summary>
        /// پیشنهاد مکمل برای کل سبد: نوع‌هایی که مشتری هنوز ندارد، با حیوانِ همان کالا، موجود، با قیمت معقول نسبت به سبد.
        /// کالایی که چند قلم سبد به آن نیاز دارند امتیاز بیشتری می‌گیرد. هر نوع حداکثر ۲ کالا.
        /// </summary>
        public static List<CartSuggestion> CartSuggestions(IReadOnlyList<ProductCandidate> cart, IReadOnlyList<ProductCandidate> pool,
            IReadOnlyDictionary<long, int> coPurchase, int take, IReadOnlyList<ProductCandidate> alsoInCart = null)
        {
            var result = new List<CartSuggestion>();
            if (cart == null || cart.Count == 0 || pool == null || take <= 0) return result;
            coPurchase ??= new Dictionary<long, int>();
            // alsoInCart: بقیه‌ی اقلام سبد (فروشگاه‌های دیگر) — فقط برای «دوباره پیشنهاد نده»؛ نامزدکننده نیستند
            var everything = alsoInCart == null ? cart : cart.Concat(alsoInCart).GroupBy(c => c.Id).Select(g => g.First()).ToList();
            var cartIds = new HashSet<long>(everything.Select(c => c.Id));
            var items = cart.Select(c => (Item: c, Profile: Profile(c))).ToList();
            var haveKinds = everything.Select(c => Profile(c).Kind).Where(k => k != null).Distinct().ToList();
            var maxCartPrice = cart.Max(c => c.Price);
            var byKind = new Dictionary<string, List<(ProductCandidate Item, double Score, long ForId)>>();

            foreach (var cand in pool)
            {
                if (cartIds.Contains(cand.Id) || !IsAvailable(cand)) continue;
                var cp = Profile(cand);
                if (cp.Kind == null) continue;
                if (!KindFitsSpecies(cp.Kind, cp.Species)) continue;
                if (haveKinds.Any(k => KindsCompatible(k, cp.Kind))) continue;       // این نوع را الان در سبد دارد
                if (maxCartPrice > 0 && cand.Price > 4.0 * maxCartPrice) continue;     // پیشنهادِ «وسوسه‌ای» نباید از کل سبد گران‌تر باشد

                double score = 0; var nominators = 0; var bestWeight = -1; long forId = 0;
                foreach (var (item, profile) in items)
                {
                    if (profile.Kind == null || !SpeciesCompatible(profile.Species, cp.Species)) continue;
                    var weight = Complements(profile.Kind).FirstOrDefault(c => c.Kind == cp.Kind).Weight;
                    if (weight <= 0) continue;
                    nominators++;
                    score += weight * 3;
                    if (weight > bestWeight) { bestWeight = weight; forId = item.Id; }
                }
                coPurchase.TryGetValue(cand.Id, out var support);
                if (nominators == 0)
                {
                    // فقط با شواهد قوی خرید هم‌زمان (حداقل ۴ سفارش) و حیوان سازگار با دست‌کم یکی از اقلام سبد
                    if (support < 4 || !items.Any(i => SpeciesCompatible(i.Profile.Species, cp.Species))) continue;
                    score += 6; forId = cart[0].Id;
                }
                score += Math.Min(15, support * 5) + (nominators > 1 ? (nominators - 1) * 6 : 0) + Popularity(cand);
                if (maxCartPrice > 0 && cand.Price > maxCartPrice) score -= 3 * Math.Log(cand.Price / (double)maxCartPrice);

                if (!byKind.TryGetValue(cp.Kind, out var list)) byKind[cp.Kind] = list = new List<(ProductCandidate, double, long)>();
                list.Add((cand, score, forId));
            }

            foreach (var k in byKind.Keys.ToList()) byKind[k] = byKind[k].OrderByDescending(x => x.Score).ThenBy(x => x.Item.Id).ToList();
            var order = byKind.Keys.OrderByDescending(k => byKind[k][0].Score).ThenBy(k => k, StringComparer.Ordinal).ToList();
            for (var round = 0; round < 2 && result.Count < take; round++)
                foreach (var k in order)
                    if (byKind[k].Count > round && result.Count < take)
                        result.Add(new CartSuggestion { ProductId = byKind[k][round].Item.Id, Kind = k, ForProductId = byKind[k][round].ForId });
            return result;
        }
    }
}
