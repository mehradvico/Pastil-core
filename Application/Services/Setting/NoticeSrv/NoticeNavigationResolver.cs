using System;
using System.Collections.Generic;

namespace Application.Services.Setting.NoticeSrv
{
    /// <summary>
    /// Converts an admin notice's domain reference into the panel route where
    /// that exact item can be reviewed.  NoticeType.NavigationTemplate remains
    /// a fallback for custom notice types, but built-in notices must not lose
    /// their reference id by navigating only to a generic list page.
    /// </summary>
    public static class NoticeNavigationResolver
    {
        private const string NoticeCenterRoute = "/admin/notice";

        public static string Resolve(
            string configuredNavigationUrl,
            string noticeLabel,
            string referenceType,
            long? referenceId,
            IReadOnlyDictionary<string, string> metadata = null,
            long? actorUserId = null)
        {
            var reference = PositiveId(referenceId);
            var route = (referenceType ?? string.Empty).Trim();

            switch (route)
            {
                case "Trip":
                    return Detail("/admin/trip/detail-", reference, configuredNavigationUrl);
                case "Driver":
                    return Detail("/admin/driver/edit-", reference, configuredNavigationUrl);
                case "Companion":
                    return Detail("/admin/companion/detail-", reference, configuredNavigationUrl);
                case "Pansion":
                    return Detail("/admin/pansion/detail-", reference, configuredNavigationUrl);
                case "Store":
                    return Detail("/admin/store/detail-", reference, configuredNavigationUrl);
                case "CompanionReserve":
                    return Detail("/admin/companionreserve/detail-", reference, configuredNavigationUrl);
                case "PansionReserve":
                    return Detail("/admin/pansionreserve/detail-", reference, configuredNavigationUrl);
                case "ProductOrder":
                    return Detail("/admin/productorder/detail-", reference, configuredNavigationUrl);
                case "User":
                    return Detail("/admin/user/profile-", reference, configuredNavigationUrl);
                case "UserBankCard":
                    return Query("/admin/userbankcard", "userBankCardId", reference, configuredNavigationUrl);
                case "CompanionComment":
                    return Query("/admin/companion-comment", "companionId", MetadataId(metadata, "companionId"), configuredNavigationUrl);
                case "CompanionReserveComment":
                    return Detail("/admin/companionreserve/detail-", MetadataId(metadata, "companionReserveId"), configuredNavigationUrl);
                case "PansionComment":
                    return Query("/admin/pansion-comment", "pansionId", MetadataId(metadata, "pansionId"), configuredNavigationUrl);
                case "CompanionAssistance":
                    return Query("/admin/companion-assistance", "companionAssistanceId", reference, configuredNavigationUrl);
                case "CompanionAssistancePackage":
                    return Query("/admin/companion-assistance-package", "packageId", reference, configuredNavigationUrl);
                case "CompanionAssistanceReport":
                    return Query("/admin/companion-assistance-report", "reportId", reference, configuredNavigationUrl);
                case "CompanionReport":
                    return Query("/admin/companion-report", "reportId", reference, configuredNavigationUrl);
                case "CompanionUser":
                    return Query("/admin/companion-user", "companionUserId", reference, configuredNavigationUrl);
                case "PastilMatchProfile":
                    return Query("/admin/pastilmatchprofile", "profileId", reference, configuredNavigationUrl);
                case "PastilMatchReport":
                    return Query("/admin/pastilmatchreport", "reportId", reference, configuredNavigationUrl);
                case "PetResanService":
                    return Detail("/admin/user/profile-", PositiveId(actorUserId), configuredNavigationUrl);
            }

            return Fallback(configuredNavigationUrl);
        }

        private static string Detail(string prefix, long? id, string configuredNavigationUrl) =>
            id.HasValue ? $"{prefix}{id.Value}" : Fallback(configuredNavigationUrl);

        private static string Query(string path, string name, long? id, string configuredNavigationUrl) =>
            id.HasValue ? $"{path}?{name}={id.Value}" : Fallback(configuredNavigationUrl);

        private static string Fallback(string configuredNavigationUrl) =>
            string.IsNullOrWhiteSpace(configuredNavigationUrl) ? NoticeCenterRoute : configuredNavigationUrl.Trim();

        private static long? MetadataId(IReadOnlyDictionary<string, string> metadata, string name)
        {
            if (metadata == null || !metadata.TryGetValue(name, out var value))
                return null;
            return long.TryParse(value, out var id) ? PositiveId(id) : null;
        }

        private static long? PositiveId(long? value) => value.GetValueOrDefault() > 0 ? value : null;
    }
}
