using System;
using System.Collections.Generic;

namespace Application.Common.Security
{
    /// <summary>
    /// کنترلرهای ناحیه‌ی Admin که فقط «ادمین اصلی» (SuperAdminIdentity) به آن‌ها می‌رسد. هم policy «SuperAdminOnly»
    /// (AdminAreaAuthorizationConvention) و هم منوی پنل (PermissionService: برای بقیه‌ی ادمین‌ها پنهان) از همین یک فهرست می‌آیند.
    /// </summary>
    public static class SuperAdminControllers
    {
        public static readonly IReadOnlySet<string> Names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Role", "RolePermission", "Permission", "PermissionSync",
            "Merchant", "Wallet",
            "ServerMonitoring", "PushDiagnostics",
            "UserMemory", "PastilMatchMessage", "PastilMatchMessageAttachment", "PastilMatchMessageReaction"
        };

        public static bool Contains(string controller) => !string.IsNullOrEmpty(controller) && Names.Contains(controller);
    }
}
