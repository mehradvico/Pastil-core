using Application.Services.Dto;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Application.Common.Privacy
{
    /// <summary>
    /// حذف اطلاعات شخصی کاربران از پاسخ‌های endpointهایی که احراز هویت لازم ندارند. لیست‌های عمومی (پزشکان، همکاران،
    /// نظرات، صاحب مرکز…) قبلاً کاربر را با <see cref="UserMinVDto"/>/<see cref="UserVDto"/> کامل برمی‌گرداندند: موبایل،
    /// ایمیل، کد معرف، موقعیت مکانی فعلی، نقش و … برای هر ناشناسی قابل خواندن و قابل جمع‌آوری انبوه بود.
    /// فقط نام و مشخصات نمایشی (نام، جنسیت، تخصص، تصویر) می‌ماند.
    /// </summary>
    public static class UserPiiScrubber
    {
        private const int MaxDepth = 12;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

        public static void Scrub(object root)
        {
            if (root == null)
                return;
            Walk(root, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);
        }

        private static void Walk(object node, HashSet<object> seen, int depth)
        {
            if (node == null || depth > MaxDepth)
                return;

            var type = node.GetType();
            if (type.IsValueType || node is string)
                return;
            if (!seen.Add(node))
                return;

            switch (node)
            {
                case UserMinVDto minimal:
                    minimal.Mobile = null;
                    minimal.Email = null;
                    minimal.ReferralCode = null;
                    return;
                case UserVDto full:
                    full.Mobile = null;
                    full.Email = null;
                    full.ReferralCode = null;
                    full.RegistrationReferralSource = default;
                    full.UsedReferralCode = null;
                    full.ReferredByUserId = null;
                    full.ReferredByCompanionId = null;
                    full.ReferredByStoreId = null;
                    full.CompanionId = null;
                    full.DriverId = null;
                    full.CreateDate = default;
                    full.RoleId = 0;
                    full.RoleName = null;
                    full.UserCurrentLocation = null;
                    return;
            }

            if (node is IDictionary dictionary)
            {
                foreach (var value in dictionary.Values)
                    Walk(value, seen, depth + 1);
                return;
            }

            if (node is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                    Walk(item, seen, depth + 1);
                return;
            }

            // فقط گراف DTOهای خود برنامه (نه نوع‌های فریمورک) پیمایش می‌شود
            if (type.Namespace == null || !type.Namespace.StartsWith("Application", StringComparison.Ordinal))
                return;

            foreach (var property in PropertyCache.GetOrAdd(type, GetWalkableProperties))
            {
                object value;
                try
                {
                    value = property.GetValue(node);
                }
                catch
                {
                    continue;
                }
                Walk(value, seen, depth + 1);
            }
        }

        private static PropertyInfo[] GetWalkableProperties(Type type) =>
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead
                                   && property.GetIndexParameters().Length == 0
                                   && !property.PropertyType.IsValueType
                                   && property.PropertyType != typeof(string))
                .ToArray();
    }
}
