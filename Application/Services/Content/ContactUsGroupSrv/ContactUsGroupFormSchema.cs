using Application.Services.Content.ContactUsGroupSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Content.ContactUsGroupSrv
{
    public static class ContactUsGroupLabels
    {
        public const string ContactPastil = "contact-pastil";
        public const string DriverRequest = "driver-request";
        public const string CompanionRequest = "companion-request";
        public const string PetShopRequest = "pet-shop-request";
        public const string SpecialProductRequest = "special-product-request";
        public const string AdvertisingRequest = "advertising-request";
    }

    public static class ContactUsGroupFormSchema
    {
        private static readonly IReadOnlyDictionary<string, List<ContactUsFormFieldDto>> Fields =
            new Dictionary<string, List<ContactUsFormFieldDto>>(StringComparer.OrdinalIgnoreCase)
            {
                [ContactUsGroupLabels.ContactPastil] = new List<ContactUsFormFieldDto>(),
                [ContactUsGroupLabels.DriverRequest] = new List<ContactUsFormFieldDto>
                {
                    Text("city", "شهر محل فعالیت", true, 1, 100, "مثلاً تهران"),
                    Select("vehicleType", "نوع وسیله نقلیه", true, 2,
                        Option("motorcycle", "موتورسیکلت"), Option("car", "خودرو سواری"), Option("van", "وانت یا ون")),
                    Select("hasDriverLicense", "گواهینامه معتبر دارید؟", true, 3,
                        Option("yes", "بله"), Option("no", "خیر")),
                    Number("experienceYears", "سابقه کاری (سال)", false, 4, "مثلاً ۲")
                },
                [ContactUsGroupLabels.CompanionRequest] = new List<ContactUsFormFieldDto>
                {
                    Text("province", "استان محل فعالیت", true, 1, 100, "استان"),
                    Text("city", "شهر محل فعالیت", true, 2, 100, "شهر"),
                    Select("requestedServiceArea", "حوزه همکاری", true, 3,
                        Option("veterinary", "کلینیک دامپزشکی"), Option("grooming", "آرایش و شست‌وشوی حیوانات"),
                        Option("training", "آموزش حیوانات"), Option("boarding", "پانسیون"),
                        Option("transport", "حمل‌ونقل حیوانات"), Option("other", "سایر")),
                    Select("hasRelatedBusiness", "کسب‌وکار مرتبط دارید؟", true, 4,
                        Option("yes", "بله"), Option("no", "خیر")),
                    Number("experienceYears", "سابقه فعالیت مرتبط (سال)", false, 5, "مثلاً ۳")
                },
                [ContactUsGroupLabels.PetShopRequest] = new List<ContactUsFormFieldDto>
                {
                    Text("storeName", "نام پت‌شاپ", true, 1, 150, "نام فروشگاه"),
                    Text("province", "استان", true, 2, 100, "استان"),
                    Text("city", "شهر", true, 3, 100, "شهر"),
                    Select("storeType", "نوع فعالیت", true, 4,
                        Option("physical", "حضوری"), Option("online", "آنلاین"), Option("both", "حضوری و آنلاین")),
                    Url("websiteOrSocial", "وب‌سایت یا شبکه اجتماعی", false, 5, 300, "https://...")
                },
                [ContactUsGroupLabels.SpecialProductRequest] = new List<ContactUsFormFieldDto>
                {
                    Text("productName", "نام محصول", true, 1, 200, "نام دقیق محصول"),
                    Text("brand", "برند محصول", false, 2, 150, "برند"),
                    Number("quantity", "تعداد موردنیاز", true, 3, "تعداد", 1),
                    Url("productLink", "لینک نمونه محصول", false, 4, 500, "https://...")
                },
                [ContactUsGroupLabels.AdvertisingRequest] = new List<ContactUsFormFieldDto>
                {
                    Text("businessName", "نام برند یا کسب‌وکار", true, 1, 150, "نام برند"),
                    Select("advertisingType", "نوع تبلیغات", true, 2,
                        Option("banner", "بنر در پاستیل"), Option("sponsored-post", "محتوای اسپانسری"),
                        Option("social-media", "شبکه‌های اجتماعی"), Option("campaign", "کمپین مشترک"),
                        Option("other", "سایر")),
                    Url("websiteOrSocial", "وب‌سایت یا شبکه اجتماعی", false, 3, 300, "https://..."),
                    Text("budgetRange", "بازه بودجه", false, 4, 100, "بازه تقریبی بودجه")
                }
            };

        public static List<ContactUsFormFieldDto> GetFields(string label)
        {
            return !string.IsNullOrWhiteSpace(label) && Fields.TryGetValue(label.Trim(), out var fields)
                ? fields
                : new List<ContactUsFormFieldDto>();
        }

        public static bool IsManaged(string label)
        {
            return !string.IsNullOrWhiteSpace(label) && Fields.ContainsKey(label.Trim());
        }

        private static ContactUsFormFieldDto Text(string key, string label, bool required, int priority, int maxLength, string placeholder)
            => Field(key, label, "text", required, priority, maxLength, placeholder);

        private static ContactUsFormFieldDto Number(string key, string label, bool required, int priority, string placeholder, decimal minValue = 0)
        {
            var field = Field(key, label, "number", required, priority, null, placeholder);
            field.MinValue = minValue;
            return field;
        }

        private static ContactUsFormFieldDto Url(string key, string label, bool required, int priority, int maxLength, string placeholder)
            => Field(key, label, "url", required, priority, maxLength, placeholder);

        private static ContactUsFormFieldDto Select(string key, string label, bool required, int priority, params ContactUsFormOptionDto[] options)
        {
            var field = Field(key, label, "select", required, priority, null, null);
            field.Options = new List<ContactUsFormOptionDto>(options);
            return field;
        }

        private static ContactUsFormFieldDto Field(string key, string label, string inputType, bool required, int priority, int? maxLength, string placeholder)
        {
            return new ContactUsFormFieldDto
            {
                Key = key,
                Label = label,
                InputType = inputType,
                Required = required,
                Priority = priority,
                MaxLength = maxLength,
                Placeholder = placeholder
            };
        }

        private static ContactUsFormOptionDto Option(string value, string label)
            => new ContactUsFormOptionDto { Value = value, Label = label };
    }
}
