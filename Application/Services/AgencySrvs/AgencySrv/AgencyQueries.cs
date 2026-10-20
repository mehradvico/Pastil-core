using Application.Common.Enumerable;
using Application.Services.AgencySrvs.AgencySrv.Dto;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.AgencySrvs.AgencySrv
{
    /// <summary>
    /// کوئری‌های یکپارچه‌ی لیست نمایندگی‌ها. جدا شده‌اند تا ترجمه‌ی آن‌ها به SQL بدون دیتابیس تست شود (ToQueryString).
    /// هر نوع فیلتر خودش را اعمال می‌کند و بعد به یک شکل مشترک (AgencyVDto) برمی‌گردد تا Concat شود.
    /// </summary>
    public static class AgencyQueries
    {
        public static IEnumerable<IQueryable<AgencyRow>> Sources(IDataBaseContext context, AgencyInputDto input)
        {
            var companionOnly = input.IsPersonal.HasValue || input.GoldAccount == true || input.SilverAccount == true;
            bool Wants(AgencyKindEnum kind) => input.Kind == null || input.Kind == kind;

            if (Wants(AgencyKindEnum.Companion)) yield return Companions(context, input);
            if (Wants(AgencyKindEnum.Groomer)) yield return Groomers(context, input);
            if (Wants(AgencyKindEnum.Clinic)) yield return Clinics(context, input);
            if (Wants(AgencyKindEnum.Coach)) yield return Coaches(context, input);
            if (!companionOnly)
            {
                if (Wants(AgencyKindEnum.Store)) yield return Stores(context, input);
                if (Wants(AgencyKindEnum.Pansion)) yield return Pansions(context, input);
                if (Wants(AgencyKindEnum.School)) yield return Schools(context, input);
            }
        }

        public static IQueryable<AgencyRow> Companions(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Companions.Where(c => !c.Deleted);
            // آرایشگاهی که به Groomer منتقل شده، دیگر به‌عنوان Companion نمایش داده نمی‌شود (مگر نوع دیگری هم داشته باشد، مثل کلینیک).
            model = model.Where(Application.Services.CompanionSrvs.CompanionSrv.CompanionMigrationFilter.NotFullyMigrated(context));

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(c => c.Name.Contains(q) || c.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(c => c.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(c => c.Approved == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(c => c.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(c => c.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));
            if (input.IsPersonal.HasValue)
                model = model.Where(c => c.IsPersonal == input.IsPersonal.Value);

            var now = DateTime.Now;
            if (input.GoldAccount == true && input.SilverAccount == true)
                model = model.Where(c => (c.GoldAccountDate != null && c.GoldAccountDate >= now) || (c.SilverAccountDate != null && c.SilverAccountDate >= now));
            else if (input.GoldAccount == true)
                model = model.Where(c => c.GoldAccountDate != null && c.GoldAccountDate >= now);
            else if (input.SilverAccount == true)
                model = model.Where(c => c.SilverAccountDate != null && c.SilverAccountDate >= now);

            return model.Select(c => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Companion,
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                ProviderId = c.ProviderId,
                Active = c.Active,
                Approved = c.Approved,
                ActivationValue = c.ActivationValue,
                IsPersonal = c.IsPersonal,
                RateAvg = c.RateAvg,
                RateCount = c.RateCount,
                GoldAccountDate = c.GoldAccountDate,
                SilverAccountDate = c.SilverAccountDate,
                OwnerId = c.OwnerId,
                OwnerFirstName = c.Owner.FirstName,
                OwnerLastName = c.Owner.LastName,
                OwnerMobile = c.Owner.Mobile,
                CityName = c.City.Name,
                StateName = c.City.State.Name,
                NeighborhoodName = c.Neighborhood.Name,
                PictureUrl = c.Picture == null ? null : c.Picture.Url + "/" + c.Picture.Name,
                IconUrl = c.Icon == null ? null : c.Icon.Url + "/" + c.Icon.Name,
            });
        }

        public static IQueryable<AgencyRow> Stores(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Stores.Where(s => !s.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(s => s.Name.Contains(q));
            }
            if (input.Available.HasValue)
                model = model.Where(s => s.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(s => s.Approved == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(s => s.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(s => s.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));

            return model.Select(s => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Store,
                Id = s.Id,
                Name = s.Name,
                Phone = s.Mobile ?? s.Phone,
                ProviderId = s.ProviderId,
                Active = s.Active,
                Approved = s.Approved,
                ActivationValue = s.ApprovalValue,
                IsPersonal = false,
                RateAvg = s.RateAvg,
                RateCount = s.RateCount,
                GoldAccountDate = null,
                SilverAccountDate = null,
                // فروشگاه مالک مستقیم ندارد؛ اولین کاربر وصل‌شده (StoreUser) نمایش داده می‌شود.
                OwnerId = s.Users.OrderBy(u => u.Id).Select(u => u.Id).FirstOrDefault(),
                OwnerFirstName = s.Users.OrderBy(u => u.Id).Select(u => u.FirstName).FirstOrDefault(),
                OwnerLastName = s.Users.OrderBy(u => u.Id).Select(u => u.LastName).FirstOrDefault(),
                OwnerMobile = s.Users.OrderBy(u => u.Id).Select(u => u.Mobile).FirstOrDefault(),
                CityName = s.City.Name,
                StateName = s.City.State.Name,
                NeighborhoodName = null,
                PictureUrl = s.Picture == null ? null : s.Picture.Url + "/" + s.Picture.Name,
                IconUrl = s.Icon == null ? null : s.Icon.Url + "/" + s.Icon.Name,
            });
        }

        public static IQueryable<AgencyRow> Groomers(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Groomers.Where(g => !g.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(g => g.Name.Contains(q) || g.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(g => g.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(g => g.Approved == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(g => g.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(g => g.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));
            var now = DateTime.Now;
            if (input.IsPersonal.HasValue)
                model = model.Where(g => g.IsPersonal == input.IsPersonal.Value);
            if (input.GoldAccount == true && input.SilverAccount == true)
                model = model.Where(g => (g.GoldAccountDate != null && g.GoldAccountDate >= now) || (g.SilverAccountDate != null && g.SilverAccountDate >= now));
            else if (input.GoldAccount == true)
                model = model.Where(g => g.GoldAccountDate != null && g.GoldAccountDate >= now);
            else if (input.SilverAccount == true)
                model = model.Where(g => g.SilverAccountDate != null && g.SilverAccountDate >= now);

            return model.Select(g => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Groomer,
                Id = g.Id,
                Name = g.Name,
                Phone = g.Phone,
                ProviderId = g.ProviderId,
                Active = g.Active,
                Approved = g.Approved,
                ActivationValue = g.ActivationValue,
                IsPersonal = g.IsPersonal,
                RateAvg = g.RateAvg,
                RateCount = g.RateCount,
                GoldAccountDate = g.GoldAccountDate,
                SilverAccountDate = g.SilverAccountDate,
                OwnerId = g.OwnerId,
                OwnerFirstName = g.Owner.FirstName,
                OwnerLastName = g.Owner.LastName,
                OwnerMobile = g.Owner.Mobile,
                CityName = g.City.Name,
                StateName = g.City.State.Name,
                NeighborhoodName = g.Neighborhood.Name,
                PictureUrl = g.Picture == null ? null : g.Picture.Url + "/" + g.Picture.Name,
                IconUrl = g.Icon == null ? null : g.Icon.Url + "/" + g.Icon.Name,
            });
        }

        public static IQueryable<AgencyRow> Clinics(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Clinics.Where(g => !g.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(g => g.Name.Contains(q) || g.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(g => g.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(g => g.Approved == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(g => g.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(g => g.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));
            var now = DateTime.Now;
            // کلینیک «شخصی» نیست.
            if (input.IsPersonal == true)
                model = model.Where(g => false);
            if (input.GoldAccount == true && input.SilverAccount == true)
                model = model.Where(g => (g.GoldAccountDate != null && g.GoldAccountDate >= now) || (g.SilverAccountDate != null && g.SilverAccountDate >= now));
            else if (input.GoldAccount == true)
                model = model.Where(g => g.GoldAccountDate != null && g.GoldAccountDate >= now);
            else if (input.SilverAccount == true)
                model = model.Where(g => g.SilverAccountDate != null && g.SilverAccountDate >= now);

            return model.Select(g => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Clinic,
                Id = g.Id,
                Name = g.Name,
                Phone = g.Phone,
                ProviderId = g.ProviderId,
                Active = g.Active,
                Approved = g.Approved,
                ActivationValue = g.ActivationValue,
                IsPersonal = false,
                RateAvg = g.RateAvg,
                RateCount = g.RateCount,
                GoldAccountDate = g.GoldAccountDate,
                SilverAccountDate = g.SilverAccountDate,
                OwnerId = g.OwnerId,
                OwnerFirstName = g.Owner.FirstName,
                OwnerLastName = g.Owner.LastName,
                OwnerMobile = g.Owner.Mobile,
                CityName = g.City.Name,
                StateName = g.City.State.Name,
                NeighborhoodName = g.Neighborhood.Name,
                PictureUrl = g.Picture == null ? null : g.Picture.Url + "/" + g.Picture.Name,
                IconUrl = g.Icon == null ? null : g.Icon.Url + "/" + g.Icon.Name,
            });
        }
        public static IQueryable<AgencyRow> Coaches(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Coaches.Where(g => !g.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(g => g.Name.Contains(q) || g.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(g => g.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(g => g.Approved == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(g => g.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(g => g.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));
            var now = DateTime.Now;
            // مربی «شخصی» نیست.
            if (input.IsPersonal == true)
                model = model.Where(g => false);
            if (input.GoldAccount == true && input.SilverAccount == true)
                model = model.Where(g => (g.GoldAccountDate != null && g.GoldAccountDate >= now) || (g.SilverAccountDate != null && g.SilverAccountDate >= now));
            else if (input.GoldAccount == true)
                model = model.Where(g => g.GoldAccountDate != null && g.GoldAccountDate >= now);
            else if (input.SilverAccount == true)
                model = model.Where(g => g.SilverAccountDate != null && g.SilverAccountDate >= now);

            return model.Select(g => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Coach,
                Id = g.Id,
                Name = g.Name,
                Phone = g.Phone,
                ProviderId = g.ProviderId,
                Active = g.Active,
                Approved = g.Approved,
                ActivationValue = g.ActivationValue,
                IsPersonal = false,
                RateAvg = g.RateAvg,
                RateCount = g.RateCount,
                GoldAccountDate = g.GoldAccountDate,
                SilverAccountDate = g.SilverAccountDate,
                OwnerId = g.OwnerId,
                OwnerFirstName = g.Owner.FirstName,
                OwnerLastName = g.Owner.LastName,
                OwnerMobile = g.Owner.Mobile,
                CityName = g.City.Name,
                StateName = g.City.State.Name,
                NeighborhoodName = g.Neighborhood.Name,
                PictureUrl = g.Picture == null ? null : g.Picture.Url + "/" + g.Picture.Name,
                IconUrl = g.Icon == null ? null : g.Icon.Url + "/" + g.Icon.Name,
            });
        }

        // پانسیون و مدرسه زیر یک Companion هستند؛ ارائه‌دهنده و مالک از همان Companion می‌آید.
        public static IQueryable<AgencyRow> Pansions(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Pansions.Where(p => !p.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(p => p.Name.Contains(q) || p.Companion.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(p => p.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(p => p.Approve == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(p => p.Companion.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(p => p.Companion.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));

            return model.Select(p => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.Pansion,
                Id = p.Id,
                Name = p.Name,
                Phone = p.Companion.Phone,
                ProviderId = p.Companion.ProviderId,
                Active = p.Active,
                Approved = p.Approve,
                ActivationValue = p.ApprovalValue,
                IsPersonal = false,
                RateAvg = p.RateAvg,
                RateCount = p.RateCount,
                GoldAccountDate = null,
                SilverAccountDate = null,
                OwnerId = p.Companion.OwnerId,
                OwnerFirstName = p.Companion.Owner.FirstName,
                OwnerLastName = p.Companion.Owner.LastName,
                OwnerMobile = p.Companion.Owner.Mobile,
                CityName = p.City.Name,
                StateName = p.City.State.Name,
                NeighborhoodName = null,
                PictureUrl = p.Picture == null ? null : p.Picture.Url + "/" + p.Picture.Name,
                IconUrl = null,
            });
        }

        public static IQueryable<AgencyRow> Schools(IDataBaseContext context, AgencyInputDto input)
        {
            var model = context.Schools.Where(p => !p.Deleted);

            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                model = model.Where(p => p.Name.Contains(q) || p.Companion.Phone == q);
            }
            if (input.Available.HasValue)
                model = model.Where(p => p.Active == input.Available.Value);
            if (input.Approved.HasValue)
                model = model.Where(p => p.Approve == input.Approved.Value);
            if (input.ProviderId.HasValue)
                model = model.Where(p => p.Companion.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(p => p.Companion.Provider.RoleAssignments.Any(a => !a.Deleted && a.ProviderRoleId == input.ProviderRoleId.Value));

            return model.Select(p => new AgencyRow
            {
                Kind = (int)AgencyKindEnum.School,
                Id = p.Id,
                Name = p.Name,
                Phone = p.Companion.Phone,
                ProviderId = p.Companion.ProviderId,
                Active = p.Active,
                Approved = p.Approve,
                ActivationValue = p.ApprovalValue,
                IsPersonal = false,
                RateAvg = p.RateAvg,
                RateCount = p.RateCount,
                GoldAccountDate = null,
                SilverAccountDate = null,
                OwnerId = p.Companion.OwnerId,
                OwnerFirstName = p.Companion.Owner.FirstName,
                OwnerLastName = p.Companion.Owner.LastName,
                OwnerMobile = p.Companion.Owner.Mobile,
                CityName = p.City.Name,
                StateName = p.City.State.Name,
                NeighborhoodName = null,
                PictureUrl = p.Picture == null ? null : p.Picture.Url + "/" + p.Picture.Name,
                IconUrl = null,
            });
        }

        /// <summary>
        /// ترکیب انواع. فیلترهای مخصوص نمایندگی (شخصی، طلایی، نقره‌ای) بقیه‌ی انواع را از نتیجه حذف می‌کنند.
        /// </summary>
        public static IQueryable<AgencyRow> Combined(IDataBaseContext context, AgencyInputDto input)
        {
            var companionOnly = input.IsPersonal.HasValue || input.GoldAccount == true || input.SilverAccount == true;
            bool Wants(AgencyKindEnum kind) => input.Kind == null || input.Kind == kind;

            IQueryable<AgencyRow> result = null;
            void Add(IQueryable<AgencyRow> query) => result = result == null ? query : result.Concat(query);

            if (Wants(AgencyKindEnum.Companion)) Add(Companions(context, input));
            // آرایشگاه هم مثل Companion «شخصی» و اشتراک دارد، پس فیلترهای مخصوص نمایندگی آن را حذف نمی‌کنند.
            if (Wants(AgencyKindEnum.Groomer)) Add(Groomers(context, input));
            if (Wants(AgencyKindEnum.Clinic)) Add(Clinics(context, input));
            if (Wants(AgencyKindEnum.Coach)) Add(Coaches(context, input));
            if (!companionOnly)
            {
                if (Wants(AgencyKindEnum.Store)) Add(Stores(context, input));
                if (Wants(AgencyKindEnum.Pansion)) Add(Pansions(context, input));
                if (Wants(AgencyKindEnum.School)) Add(Schools(context, input));
            }

            // فیلتر مخصوص Companion روی نوع دیگر: نتیجه‌ی خالی، نه خطا.
            return result ?? Companions(context, input).Where(r => false);
        }
    
        public static IOrderedQueryable<AgencyRow> Order(IQueryable<AgencyRow> model, SortEnum sort, DateTime now)
        {
            return sort switch
            {
                SortEnum.Old => model.OrderBy(x => x.Id).ThenBy(x => x.Kind),
                // اولویت ویژه: طلایی، بعد نقره‌ای، بعد بقیه (فروشگاه‌ها حساب ویژه ندارند و بعد از آن‌ها می‌آیند)
                SortEnum.MorePriority => model
                    .OrderBy(x => x.GoldAccountDate != null && x.GoldAccountDate > now ? 0 : x.SilverAccountDate != null && x.SilverAccountDate > now ? 1 : 2)
                    .ThenByDescending(x => x.Id).ThenBy(x => x.Kind),
                _ => model.OrderByDescending(x => x.Id).ThenBy(x => x.Kind),
            };
        }
    }
}
