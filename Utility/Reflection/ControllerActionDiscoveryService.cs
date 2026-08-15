using System.Reflection;
using System.Xml.Linq;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using Utility.Reflection.Dto;
using Utility.Reflection.Iface;

namespace Utility.Reflection
{
    public class ControllerActionDiscoveryService : IControllerActionDiscoveryService
    {
        private readonly IDataBaseContext _context;

        public ControllerActionDiscoveryService(IDataBaseContext context)
        {
            _context = context;
        }

        public List<ControllerActionInfoDto> GetControllerActions(
            Assembly assembly,
            XDocument xmlComments)
        {
            var summaries = ReadXmlSummaries(xmlComments);

            var controllers = assembly
                .GetTypes()
                .Where(IsAdminApiController)
                .Select(type => CreateControllerInfo(type, summaries))
                .OrderBy(controller => controller.ParentId)
                .ThenBy(controller => controller.Priority)
                .ThenBy(controller => controller.Name)
                .ToList();

            EnsureUniqueControllerDisplayNames(controllers);
            return controllers;
        }

        public async Task<PermissionSyncResultDto> SynchronizePermissionsAsync(
            Assembly assembly,
            XDocument xmlComments,
            CancellationToken cancellationToken = default)
        {
            var controllers = GetControllerActions(assembly, xmlComments);
            var result = new PermissionSyncResultDto
            {
                ParentCount = AdminPermissionCatalog.Parents.Count,
                ControllerCount = controllers.Count,
                ActionCount = controllers.Sum(x => x.Actions.Count),
                UnmappedControllers = controllers
                    .Where(x => x.ParentId == 0)
                    .Select(x => x.Name)
                    .OrderBy(x => x)
                    .ToList()
            };

            if (result.UnmappedControllers.Count > 0)
                return result;

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
            try
            {
                await AcquireSynchronizationLockAsync(cancellationToken);
                await NormalizeParentPermissionsAsync(cancellationToken);

                var permissions = await _context.Permissions
                    .IgnoreQueryFilters()
                    .AsTracking()
                    .Include(x => x.Roles)
                    .ToListAsync(cancellationToken);

                foreach (var controller in controllers)
                {
                    SynchronizeController(controller, permissions, result);
                }

                EnsureUniquePermissionKeys(permissions, controllers);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static ControllerActionInfoDto CreateControllerInfo(
            Type controllerType,
            IReadOnlyDictionary<string, string> summaries)
        {
            var controllerName = controllerType.Name.EndsWith(
                "Controller",
                StringComparison.OrdinalIgnoreCase)
                ? controllerType.Name[..^"Controller".Length]
                : controllerType.Name;

            AdminPermissionCatalog.TryGetController(controllerName, out var definition);

            var actions = controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(IsApiAction)
                .GroupBy(method => method.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var method = group.First();
                    var httpMethod = GetHttpMethod(method);
                    return new ActionInfoDto
                    {
                        Name = method.Name,
                        Summary = FindMethodSummary(summaries, controllerType, method.Name),
                        HttpMethod = httpMethod,
                        Priority = GetActionPriority(method.Name, httpMethod)
                    };
                })
                .OrderBy(action => action.Priority)
                .ThenBy(action => action.Name)
                .Select((action, index) =>
                {
                    action.Priority = index;
                    return action;
                })
                .ToList();

            return new ControllerActionInfoDto
            {
                Name = controllerName,
                Summary = summaries.GetValueOrDefault($"T:{controllerType.FullName}") ?? controllerName,
                Parent = definition?.Parent.Label,
                ParentId = definition?.Parent.Id ?? 0,
                IsMenu = definition?.IsMenu ?? false,
                Priority = definition?.Priority ?? int.MaxValue,
                Actions = actions
            };
        }

        private static void EnsureUniqueControllerDisplayNames(
            IReadOnlyCollection<ControllerActionInfoDto> controllers)
        {
            var duplicate = controllers
                .GroupBy(
                    controller => $"{controller.ParentId}:{NormalizeKey(controller.Summary)}",
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate == null)
                return;

            var controllerNames = string.Join(
                ", ",
                duplicate.Select(x => x.Name).OrderBy(x => x));

            throw new InvalidOperationException(
                $"Duplicate admin permission display name detected for: {controllerNames}.");
        }

        private void SynchronizeController(
            ControllerActionInfoDto controller,
            List<Permission> permissions,
            PermissionSyncResultDto result)
        {
            if (controller.Actions.Count == 0)
                return;

            var anchorAction = controller.Actions
                .OrderBy(x => IsGetAction(x) ? 0 : 1)
                .ThenBy(x => x.Priority)
                .First();

            var controllerPermissions = permissions
                .Where(x =>
                    string.Equals(x.Controller, controller.Name, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrWhiteSpace(x.Area) ||
                     string.Equals(x.Area, "Admin", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var anchor = FindPermission(
                    controllerPermissions,
                    anchorAction.Name,
                    controller.ParentId)
                ?? controllerPermissions
                    .Where(x => x.ParentId is >= 1 and <= 13)
                    .OrderBy(x => x.Deleted)
                    .ThenBy(x => x.Id)
                    .FirstOrDefault();

            var anchorIsNew = anchor == null;
            if (anchor == null)
            {
                anchor = new Permission
                {
                    Area = "Admin",
                    Controller = controller.Name,
                    Action = anchorAction.Name,
                    Label = controller.Name,
                    Roles = []
                };
                _context.Permissions.Add(anchor);
                permissions.Add(anchor);
                controllerPermissions.Add(anchor);
                result.InsertedCount++;
            }

            var anchorWasChanged = ApplyPermission(
                    anchor,
                    controller.Summary,
                    controller.Name,
                    anchorAction.Name,
                    controller.Name.Equals("PermissionSync", StringComparison.OrdinalIgnoreCase)
                        ? false
                        : anchorIsNew
                            ? controller.IsMenu
                            : null,
                    controller.Priority,
                    controller.ParentId);

            if (anchorWasChanged && anchor.Id != 0)
                result.UpdatedCount++;

            var canonicalPermissions = new List<Permission> { anchor };

            foreach (var action in controller.Actions.Where(x => x != anchorAction))
            {
                var permission = FindPermission(
                    controllerPermissions,
                    action.Name,
                    anchor.Id == 0 ? null : anchor.Id);
                var permissionIsNew = permission == null;
                if (permission == null)
                {
                    permission = new Permission
                    {
                        Area = "Admin",
                        Controller = controller.Name,
                        Action = action.Name,
                        Label = $"{action.Name}{controller.Name}",
                        Parent = anchor,
                        Roles = []
                    };
                    _context.Permissions.Add(permission);
                    permissions.Add(permission);
                    controllerPermissions.Add(permission);
                    result.InsertedCount++;
                }

                var changed = ApplyPermission(
                    permission,
                    action.Summary,
                    controller.Name,
                    action.Name,
                    permissionIsNew ? false : null,
                    action.Priority,
                    anchor.Id == 0 ? null : anchor.Id);

                if (anchor.Id == 0)
                    permission.Parent = anchor;

                if (changed && permission.Id != 0)
                    result.UpdatedCount++;

                canonicalPermissions.Add(permission);
            }

            MergeDuplicateControllerPermissions(
                controllerPermissions,
                canonicalPermissions,
                permissions,
                result);
        }

        private static Permission FindPermission(
            IEnumerable<Permission> permissions,
            string action,
            long? expectedParentId)
        {
            return permissions
                .Where(x => string.Equals(x.Action, action, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Deleted)
                .ThenByDescending(x =>
                    string.Equals(x.Area, "Admin", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x =>
                    expectedParentId.HasValue && x.ParentId == expectedParentId)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private void MergeDuplicateControllerPermissions(
            IReadOnlyCollection<Permission> controllerPermissions,
            IReadOnlyCollection<Permission> canonicalPermissions,
            List<Permission> allPermissions,
            PermissionSyncResultDto result)
        {
            var duplicates = controllerPermissions
                .Where(permission => !canonicalPermissions.Contains(permission))
                .ToList();

            foreach (var duplicate in duplicates)
            {
                var canonical = canonicalPermissions.FirstOrDefault(permission =>
                    string.Equals(
                        permission.Action,
                        duplicate.Action,
                        StringComparison.OrdinalIgnoreCase));

                if (canonical == null)
                {
                    if (!duplicate.Deleted || duplicate.IsMenu)
                    {
                        duplicate.Deleted = true;
                        duplicate.IsMenu = false;
                        result.UpdatedCount++;
                    }

                    continue;
                }

                foreach (var role in duplicate.Roles.ToList())
                {
                    if (canonical.Roles.All(x => x.Id != role.Id))
                        canonical.Roles.Add(role);
                }

                duplicate.Roles.Clear();

                foreach (var child in allPermissions.Where(x => x.ParentId == duplicate.Id))
                {
                    child.Parent = canonical;
                    child.ParentId = canonical.Id == 0 ? null : canonical.Id;
                }

                _context.Permissions.Remove(duplicate);
                allPermissions.Remove(duplicate);
                result.MergedDuplicateCount++;
            }
        }

        private static void EnsureUniquePermissionKeys(
            IReadOnlyCollection<Permission> permissions,
            IReadOnlyCollection<ControllerActionInfoDto> controllers)
        {
            var controllerNames = controllers
                .Select(x => x.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var duplicate = permissions
                .Where(x =>
                    !x.Deleted &&
                    controllerNames.Contains(x.Controller) &&
                    (string.IsNullOrWhiteSpace(x.Area) ||
                     string.Equals(x.Area, "Admin", StringComparison.OrdinalIgnoreCase)))
                .GroupBy(
                    x => $"{NormalizeKey(x.Controller)}:{NormalizeKey(x.Action)}",
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate == null)
                return;

            throw new InvalidOperationException(
                $"Duplicate admin permission key detected for controller '{duplicate.First().Controller}' " +
                $"and action '{duplicate.First().Action}'.");
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private static bool ApplyPermission(
            Permission permission,
            string name,
            string controller,
            string action,
            bool? isMenu,
            int priority,
            long? parentId)
        {
            var changed = false;

            changed |= SetIfDifferent(permission.Name, name ?? $"{action} {controller}", x => permission.Name = x);
            changed |= SetIfDifferent(permission.Area, "Admin", x => permission.Area = x);
            changed |= SetIfDifferent(permission.Controller, controller, x => permission.Controller = x);
            changed |= SetIfDifferent(permission.Action, action, x => permission.Action = x);

            if (string.IsNullOrWhiteSpace(permission.Label))
            {
                permission.Label = action.Equals("Get", StringComparison.OrdinalIgnoreCase)
                    ? controller
                    : $"{action}{controller}";
                changed = true;
            }

            if (isMenu.HasValue && permission.IsMenu != isMenu.Value)
            {
                permission.IsMenu = isMenu.Value;
                changed = true;
            }

            if (permission.Priority != priority)
            {
                permission.Priority = priority;
                changed = true;
            }

            if (parentId.HasValue && permission.ParentId != parentId)
            {
                permission.ParentId = parentId;
                changed = true;
            }

            if (permission.Deleted)
            {
                permission.Deleted = false;
                changed = true;
            }

            return changed;
        }

        private static bool SetIfDifferent(
            string current,
            string value,
            Action<string> setter)
        {
            if (string.Equals(current, value, StringComparison.Ordinal))
                return false;

            setter(value);
            return true;
        }

        private async Task NormalizeParentPermissionsAsync(CancellationToken cancellationToken)
        {
            if (_context is not DbContext dbContext)
                throw new InvalidOperationException("Permission synchronization requires an EF Core DbContext.");

            await dbContext.Database.ExecuteSqlRawAsync(ParentNormalizationSql, cancellationToken);
        }

        private async Task AcquireSynchronizationLockAsync(CancellationToken cancellationToken)
        {
            if (_context is not DbContext dbContext)
                throw new InvalidOperationException("Permission synchronization requires an EF Core DbContext.");

            await dbContext.Database.ExecuteSqlRawAsync(
                SynchronizationLockSql,
                cancellationToken);
        }

        private static bool IsAdminApiController(Type type)
        {
            return type.IsClass &&
                   !type.IsAbstract &&
                   type.Namespace == "Api.Areas.Admin.Controllers" &&
                   type.CustomAttributes.Any(attribute =>
                       attribute.AttributeType.Name == "ApiControllerAttribute");
        }

        private static bool IsApiAction(MethodInfo method)
        {
            if (method.IsSpecialName ||
                method.CustomAttributes.Any(attribute =>
                    attribute.AttributeType.Name == "NonActionAttribute"))
            {
                return false;
            }

            return method.CustomAttributes.Any(attribute =>
                attribute.AttributeType.Name.StartsWith("Http", StringComparison.Ordinal) &&
                attribute.AttributeType.Name.EndsWith("Attribute", StringComparison.Ordinal));
        }

        private static string GetHttpMethod(MethodInfo method)
        {
            var attribute = method.CustomAttributes.First(x =>
                x.AttributeType.Name.StartsWith("Http", StringComparison.Ordinal) &&
                x.AttributeType.Name.EndsWith("Attribute", StringComparison.Ordinal));

            return attribute.AttributeType.Name
                .Replace("Http", string.Empty, StringComparison.Ordinal)
                .Replace("Attribute", string.Empty, StringComparison.Ordinal)
                .ToUpperInvariant();
        }

        private static int GetActionPriority(string actionName, string httpMethod)
        {
            if (actionName.Equals("Get", StringComparison.OrdinalIgnoreCase))
                return 0;
            if (actionName.Equals("Post", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (actionName.Equals("Put", StringComparison.OrdinalIgnoreCase))
                return 2;
            if (actionName.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                return 3;
            if (actionName.Equals("Patch", StringComparison.OrdinalIgnoreCase))
                return 4;

            return httpMethod switch
            {
                "GET" => 5,
                "POST" => 6,
                "PUT" => 7,
                "DELETE" => 8,
                "PATCH" => 9,
                _ => 10
            };
        }

        private static bool IsGetAction(ActionInfoDto action)
        {
            return action.Name.Equals("Get", StringComparison.OrdinalIgnoreCase);
        }

        private static IReadOnlyDictionary<string, string> ReadXmlSummaries(XDocument xmlComments)
        {
            return xmlComments
                .Descendants("member")
                .Where(member => member.Attribute("name") != null)
                .GroupBy(member => member.Attribute("name")!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => NormalizeText(group.First().Element("summary")?.Value));
        }

        private static string FindMethodSummary(
            IReadOnlyDictionary<string, string> summaries,
            Type controllerType,
            string methodName)
        {
            var prefix = $"M:{controllerType.FullName}.{methodName}";
            return summaries
                       .Where(x => x.Key.Equals(prefix, StringComparison.Ordinal) ||
                                   x.Key.StartsWith($"{prefix}(", StringComparison.Ordinal))
                       .Select(x => x.Value)
                       .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                   ?? methodName;
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : string.Join(
                    " ",
                    value.Split(
                        [' ', '\r', '\n', '\t'],
                        StringSplitOptions.RemoveEmptyEntries));
        }

        private const string SynchronizationLockSql = """
            DECLARE @LockResult int;

            EXEC @LockResult = sys.sp_getapplock
                @Resource = N'Pastil.PermissionSync',
                @LockMode = N'Exclusive',
                @LockOwner = N'Transaction',
                @LockTimeout = 30000;

            IF @LockResult < 0
                THROW 51002, 'Could not acquire the permission synchronization lock.', 1;
            """;

        private const string ParentNormalizationSql = """
            SET XACT_ABORT ON;

            DECLARE @Desired TABLE
            (
                Id bigint NOT NULL PRIMARY KEY,
                [Name] nvarchar(max) NOT NULL,
                Label nvarchar(100) NOT NULL,
                AlternateLabel nvarchar(100) NULL,
                Priority int NOT NULL
            );

            INSERT INTO @Desired (Id, [Name], Label, AlternateLabel, Priority)
            VALUES
                (1, N'تنظیمات سیستم', N'Settings', NULL, 1),
                (2, N'مدیریت کاربران', N'UserManager', NULL, 2),
                (3, N'مدیریت پت ها', N'PetManagement', NULL, 3),
                (4, N'مدیریت نمایندگان', N'CompanionManagement', NULL, 4),
                (5, N'مدیریت فروشگاه', N'ShopManagement', NULL, 5),
                (11, N'مدیریت PastilAI', N'PastilAIManagement', NULL, 6),
                (6, N'مدیریت محتوا', N'ContentManagement', N'ContentManagment', 7),
                (7, N'مدیریت یادآورها', N'ReminderManagement', NULL, 8),
                (8, N'مدیریت مالی', N'FinancialManagement', NULL, 9),
                (9, N'مدیریت موقعیت ها', N'LocationManagement', N'locationManagement', 10),
                (10, N'مدیریت پاستیل فرند', N'PastilMatchManagement', NULL, 11),
                (12, N'مدیریت سایت', N'SiteManagement', NULL, 12),
                (13, N'مدیریت پاستیل کلاب', N'PastilClubManagement', NULL, 13);

            CREATE TABLE #DuplicateParentMap
            (
                DuplicateId bigint NOT NULL PRIMARY KEY,
                CanonicalId bigint NOT NULL
            );

            ;WITH ParentCandidates AS
            (
                SELECT
                    p.Id,
                    d.Id AS DesiredId,
                    FIRST_VALUE(p.Id) OVER
                    (
                        PARTITION BY d.Id
                        ORDER BY CASE WHEN p.Id = d.Id THEN 0 ELSE 1 END, p.Id
                    ) AS CanonicalId
                FROM dbo.Permissions AS p
                INNER JOIN @Desired AS d
                    ON p.Label = d.Label OR p.Label = d.AlternateLabel
                WHERE p.ParentId IS NULL
            )
            INSERT INTO #DuplicateParentMap (DuplicateId, CanonicalId)
            SELECT Id, CanonicalId
            FROM ParentCandidates
            WHERE Id <> CanonicalId;

            INSERT INTO dbo.PermissionRole (PermissionsId, RolesId)
            SELECT DISTINCT duplicateMap.CanonicalId, permissionRole.RolesId
            FROM #DuplicateParentMap AS duplicateMap
            INNER JOIN dbo.PermissionRole AS permissionRole
                ON permissionRole.PermissionsId = duplicateMap.DuplicateId
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM dbo.PermissionRole AS existingRole
                WHERE existingRole.PermissionsId = duplicateMap.CanonicalId
                  AND existingRole.RolesId = permissionRole.RolesId
            );

            UPDATE child
            SET child.ParentId = duplicateMap.CanonicalId
            FROM dbo.Permissions AS child
            INNER JOIN #DuplicateParentMap AS duplicateMap
                ON child.ParentId = duplicateMap.DuplicateId;

            DELETE permissionRole
            FROM dbo.PermissionRole AS permissionRole
            INNER JOIN #DuplicateParentMap AS duplicateMap
                ON permissionRole.PermissionsId = duplicateMap.DuplicateId;

            DELETE duplicateParent
            FROM dbo.Permissions AS duplicateParent
            INNER JOIN #DuplicateParentMap AS duplicateMap
                ON duplicateParent.Id = duplicateMap.DuplicateId;

            IF (
                SELECT COUNT(*)
                FROM dbo.Permissions AS p
                INNER JOIN @Desired AS d ON d.Id = p.Id AND p.Label = d.Label
                WHERE p.ParentId IS NULL
            ) = 13
            BEGIN
                UPDATE p
                SET
                    p.[Name] = d.[Name],
                    p.Label = d.Label,
                    p.Area = N'',
                    p.Controller = N'',
                    p.[Action] = N'',
                    p.Priority = d.Priority,
                    p.ParentId = NULL,
                    p.Deleted = 0
                FROM dbo.Permissions AS p
                INNER JOIN @Desired AS d ON d.Id = p.Id;
                RETURN;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.Permissions
                WHERE Id = 11
                  AND NOT (ParentId IS NULL AND Label = N'PastilAIManagement')
            )
            BEGIN
                DECLARE @RelocatedPermission TABLE (Id bigint NOT NULL);

                INSERT INTO dbo.Permissions
                    ([Name], Label, Area, Controller, [Action], IsMenu, Priority, ParentId, Deleted)
                OUTPUT inserted.Id INTO @RelocatedPermission (Id)
                SELECT
                    [Name],
                    Label,
                    Area,
                    Controller,
                    [Action],
                    IsMenu,
                    Priority,
                    ParentId,
                    Deleted
                FROM dbo.Permissions
                WHERE Id = 11;

                DECLARE @RelocatedPermissionId bigint =
                    (SELECT TOP (1) Id FROM @RelocatedPermission);

                UPDATE dbo.Permissions
                SET ParentId = @RelocatedPermissionId
                WHERE ParentId = 11;

                UPDATE dbo.PermissionRole
                SET PermissionsId = @RelocatedPermissionId
                WHERE PermissionsId = 11;

                DELETE FROM dbo.Permissions
                WHERE Id = 11;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.Permissions
                WHERE Id = 12
                  AND NOT (ParentId IS NULL AND Label = N'SiteManagement')
            )
            BEGIN
                DECLARE @RelocatedSitePermission TABLE (Id bigint NOT NULL);

                INSERT INTO dbo.Permissions
                    ([Name], Label, Area, Controller, [Action], IsMenu, Priority, ParentId, Deleted)
                OUTPUT inserted.Id INTO @RelocatedSitePermission (Id)
                SELECT
                    [Name],
                    Label,
                    Area,
                    Controller,
                    [Action],
                    IsMenu,
                    Priority,
                    ParentId,
                    Deleted
                FROM dbo.Permissions
                WHERE Id = 12;

                DECLARE @RelocatedSitePermissionId bigint =
                    (SELECT TOP (1) Id FROM @RelocatedSitePermission);

                UPDATE dbo.Permissions
                SET ParentId = @RelocatedSitePermissionId
                WHERE ParentId = 12;

                UPDATE dbo.PermissionRole
                SET PermissionsId = @RelocatedSitePermissionId
                WHERE PermissionsId = 12;

                DELETE FROM dbo.Permissions
                WHERE Id = 12;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.Permissions
                WHERE Id = 13
                  AND NOT (ParentId IS NULL AND Label = N'PastilClubManagement')
            )
            BEGIN
                DECLARE @RelocatedPastilClubPermission TABLE (Id bigint NOT NULL);

                INSERT INTO dbo.Permissions
                    ([Name], Label, Area, Controller, [Action], IsMenu, Priority, ParentId, Deleted)
                OUTPUT inserted.Id INTO @RelocatedPastilClubPermission (Id)
                SELECT
                    [Name],
                    Label,
                    Area,
                    Controller,
                    [Action],
                    IsMenu,
                    Priority,
                    ParentId,
                    Deleted
                FROM dbo.Permissions
                WHERE Id = 13;

                DECLARE @RelocatedPastilClubPermissionId bigint =
                    (SELECT TOP (1) Id FROM @RelocatedPastilClubPermission);

                UPDATE dbo.Permissions
                SET ParentId = @RelocatedPastilClubPermissionId
                WHERE ParentId = 13;

                UPDATE dbo.PermissionRole
                SET PermissionsId = @RelocatedPastilClubPermissionId
                WHERE PermissionsId = 13;

                DELETE FROM dbo.Permissions
                WHERE Id = 13;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM dbo.Permissions AS p
                WHERE p.Id BETWEEN 1 AND 13
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM @Desired AS d
                      WHERE p.Label = d.Label
                         OR p.Label = d.AlternateLabel
                  )
            )
                THROW 51000, 'Permission IDs 1 through 13 contain a non-parent permission.', 1;

            IF EXISTS
            (
                SELECT d.Id
                FROM @Desired AS d
                INNER JOIN dbo.Permissions AS p
                    ON p.Label = d.Label OR p.Label = d.AlternateLabel
                WHERE p.ParentId IS NULL
                GROUP BY d.Id
                HAVING COUNT(*) > 1
            )
                THROW 51001, 'Duplicate parent permissions were found.', 1;

            CREATE TABLE #ParentMap
            (
                DesiredId bigint NOT NULL PRIMARY KEY,
                OldId bigint NOT NULL UNIQUE,
                IsMenu bit NOT NULL
            );

            INSERT INTO #ParentMap (DesiredId, OldId, IsMenu)
            SELECT d.Id, p.Id, p.IsMenu
            FROM @Desired AS d
            INNER JOIN dbo.Permissions AS p
                ON p.Label = d.Label OR p.Label = d.AlternateLabel
            WHERE p.ParentId IS NULL;

            SET IDENTITY_INSERT dbo.Permissions ON;

            INSERT INTO dbo.Permissions
                (Id, [Name], Label, Area, Controller, [Action], IsMenu, Priority, ParentId, Deleted)
            SELECT
                -d.Id,
                d.[Name],
                d.Label,
                N'',
                N'',
                N'',
                m.IsMenu,
                d.Priority,
                NULL,
                0
            FROM @Desired AS d
            INNER JOIN #ParentMap AS m ON m.DesiredId = d.Id;

            SET IDENTITY_INSERT dbo.Permissions OFF;

            UPDATE child
            SET child.ParentId = -m.DesiredId
            FROM dbo.Permissions AS child
            INNER JOIN #ParentMap AS m ON child.ParentId = m.OldId;

            UPDATE permissionRole
            SET permissionRole.PermissionsId = -m.DesiredId
            FROM dbo.PermissionRole AS permissionRole
            INNER JOIN #ParentMap AS m ON permissionRole.PermissionsId = m.OldId;

            DELETE parent
            FROM dbo.Permissions AS parent
            INNER JOIN #ParentMap AS m ON parent.Id = m.OldId;

            SET IDENTITY_INSERT dbo.Permissions ON;

            INSERT INTO dbo.Permissions
                (Id, [Name], Label, Area, Controller, [Action], IsMenu, Priority, ParentId, Deleted)
            SELECT
                d.Id,
                d.[Name],
                d.Label,
                N'',
                N'',
                N'',
                COALESCE(m.IsMenu, CONVERT(bit, 1)),
                d.Priority,
                NULL,
                0
            FROM @Desired AS d
            LEFT JOIN #ParentMap AS m ON m.DesiredId = d.Id;

            SET IDENTITY_INSERT dbo.Permissions OFF;

            UPDATE child
            SET child.ParentId = m.DesiredId
            FROM dbo.Permissions AS child
            INNER JOIN #ParentMap AS m ON child.ParentId = -m.DesiredId;

            UPDATE permissionRole
            SET permissionRole.PermissionsId = m.DesiredId
            FROM dbo.PermissionRole AS permissionRole
            INNER JOIN #ParentMap AS m ON permissionRole.PermissionsId = -m.DesiredId;

            DELETE temporaryParent
            FROM dbo.Permissions AS temporaryParent
            INNER JOIN #ParentMap AS m ON temporaryParent.Id = -m.DesiredId;
            """;
    }
}
