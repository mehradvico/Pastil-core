using Application.Services.ProviderSrvs.ProviderUserSrv;
using Microsoft.AspNetCore.Http;
using Entities.Entities.ProviderField;
using Sec = Application.Services.ProviderSrvs.ProviderUserSrv.ProviderPermissionCatalog.Sections;

namespace Api.Authorization;

public enum ProviderEndpointMode
{
    /// <summary>فقط خودِ مالک با هویت خودش. کارمند پنل (با هدر X-Provider-Id) هرگز، حتی با همه‌ی دسترسی‌ها.</summary>
    OwnerOnly = 0,
    /// <summary>مالک با هویت خودش؛ کارمند پنل فقط اگر نقشش کلید <see cref="ProviderEndpointRule.Permission"/> را داشته باشد.</summary>
    Permission = 1,
    /// <summary>کار شخصیِ خود کاربر (من آنلاینم، جلسه‌ی مشاوره‌ی من، نسخه‌ی من، ...): هدر جانشینی نادیده گرفته می‌شود و همیشه با هویت واقعی اجرا می‌شود.</summary>
    Personal = 2,
    /// <summary>جدول‌های پایه‌ی بدون داده‌ی کسب‌وکار (روزهای هفته، برندها): هر عضو معتبر پنل.</summary>
    Lookup = 3,
}

/// <param name="StaffAllowed">پرسنل/اپراتور مرکز (ClinicUser/GroomerUser/CoachUser) بدون هدر هم می‌تواند صدا بزند؛ سرویس خودش دسترسی را محدود می‌کند.</param>
public sealed record ProviderEndpointRule(ProviderEndpointMode Mode, string Permission = null, bool StaffAllowed = false);

/// <summary>
/// نگاشت تک‌تک کنترلرهای پنل ارائه‌دهنده (ناحیه‌های Companion و Seller) به دسترسی‌های کاتالوگ — هم‌ارز جدول Permission پنل ادمین،
/// ولی در کد تا با هر deploy همگام بماند و قابل بازبینی باشد. قاعده‌ی پیش‌فرض: GET = «مشاهده‌ی» بخش، بقیه‌ی متدها = «مدیریت» بخش.
/// کنترلرِ نگاشت‌نشده برای کارمند پنل بسته است (OwnerOnly)؛ تست ProviderEndpointPolicyTests هر کنترلر جدید بدون نگاشت را می‌گیرد.
/// </summary>
public static class ProviderEndpointPolicy
{
    public static readonly string[] Areas = { "Companion", "Seller" };

    private sealed record ControllerRule(ProviderEndpointMode Mode, string Scope = null, string Section = null, bool StaffAllowed = false,
        IReadOnlyDictionary<string, ProviderEndpointRule> Actions = null);

    private static ControllerRule Of(string scope, string section, bool staffAllowed = false, IReadOnlyDictionary<string, ProviderEndpointRule> actions = null) =>
        new(ProviderEndpointMode.Permission, scope, section, staffAllowed, actions);

    private static readonly ControllerRule OwnerOnly = new(ProviderEndpointMode.OwnerOnly);
    private static readonly ControllerRule Personal = new(ProviderEndpointMode.Personal, StaffAllowed: true);
    private static readonly ProviderEndpointRule OwnerOnlyAction = new(ProviderEndpointMode.OwnerOnly);

    private static string Key(string scope, string section, string level) => ProviderPermissionCatalog.Key(scope, section, level);

    /// <summary>ساختن مرکز جدید و حذف کامل مرکز کار مالک است (روی اشتراک/تسویه‌ی خودِ ارائه‌دهنده اثر دارد).</summary>
    private static readonly IReadOnlyDictionary<string, ProviderEndpointRule> VenueLifecycleOwnerOnly =
        new Dictionary<string, ProviderEndpointRule>(StringComparer.OrdinalIgnoreCase) { ["Post"] = OwnerOnlyAction, ["Delete"] = OwnerOnlyAction };

    /// <summary>جدول‌های پایه: فقط خواندن برای کارمند؛ نوشتن (اگر endpointی داشته باشد) فقط مالک.</summary>
    private static readonly ControllerRule LookupReadOnly = new(ProviderEndpointMode.Lookup);

    private static readonly Dictionary<string, ControllerRule> Companion = new(StringComparer.OrdinalIgnoreCase)
    {
        // ------------------------------------------------ کلینیک
        ["Clinic"] = Of(ProviderRoleLabels.Clinic, Sec.Profile, actions: VenueLifecycleOwnerOnly),
        ["ClinicPicture"] = Of(ProviderRoleLabels.Clinic, Sec.Profile),
        ["ClinicSchedule"] = Of(ProviderRoleLabels.Clinic, Sec.Profile),
        ["ClinicService"] = Of(ProviderRoleLabels.Clinic, Sec.Service),
        ["ClinicReserve"] = Of(ProviderRoleLabels.Clinic, Sec.Reserve),
        ["ClinicReservePackage"] = Of(ProviderRoleLabels.Clinic, Sec.Reserve),
        ["ClinicConsultationAvailability"] = Of(ProviderRoleLabels.Clinic, Sec.Consultation),
        ["ClinicConsultationPackage"] = Of(ProviderRoleLabels.Clinic, Sec.Consultation),
        ["ClinicConsultationOnline"] = Personal,
        ["ClinicReview"] = Of(ProviderRoleLabels.Clinic, Sec.Review),
        ["ClinicUser"] = Of(ProviderRoleLabels.Clinic, Sec.Staff),
        ["ClinicSettlement"] = Of(ProviderRoleLabels.Clinic, Sec.Finance),

        // ------------------------------------------------ مربی
        ["Coach"] = Of(ProviderRoleLabels.Coach, Sec.Profile, actions: VenueLifecycleOwnerOnly),
        ["CoachPicture"] = Of(ProviderRoleLabels.Coach, Sec.Profile),
        ["CoachSchedule"] = Of(ProviderRoleLabels.Coach, Sec.Profile),
        ["CoachService"] = Of(ProviderRoleLabels.Coach, Sec.Service),
        ["CoachReserve"] = Of(ProviderRoleLabels.Coach, Sec.Reserve),
        ["CoachConsultationAvailability"] = Of(ProviderRoleLabels.Coach, Sec.Consultation),
        ["CoachConsultationPackage"] = Of(ProviderRoleLabels.Coach, Sec.Consultation),
        ["CoachConsultationOnline"] = Personal,
        ["CoachReview"] = Of(ProviderRoleLabels.Coach, Sec.Review),
        ["CoachUser"] = Of(ProviderRoleLabels.Coach, Sec.Staff),
        ["CoachSettlement"] = Of(ProviderRoleLabels.Coach, Sec.Finance),

        // ------------------------------------------------ آرایشگاه
        ["Groomer"] = Of(ProviderRoleLabels.Groomer, Sec.Profile, actions: VenueLifecycleOwnerOnly),
        ["GroomerPicture"] = Of(ProviderRoleLabels.Groomer, Sec.Profile),
        ["GroomerSchedule"] = Of(ProviderRoleLabels.Groomer, Sec.Profile),
        ["GroomerService"] = Of(ProviderRoleLabels.Groomer, Sec.Service),
        ["GroomerReserve"] = Of(ProviderRoleLabels.Groomer, Sec.Reserve),
        ["GroomerPortfolio"] = Of(ProviderRoleLabels.Groomer, Sec.Portfolio),
        ["GroomerReview"] = Of(ProviderRoleLabels.Groomer, Sec.Review),
        ["GroomerUser"] = Of(ProviderRoleLabels.Groomer, Sec.Staff),
        ["GroomerSettlement"] = Of(ProviderRoleLabels.Groomer, Sec.Finance),

        // ------------------------------------------------ پانسیون
        ["Pansion"] = Of(ProviderRoleLabels.Pansion, Sec.Profile, actions: VenueLifecycleOwnerOnly),
        ["PansionActive"] = Of(ProviderRoleLabels.Pansion, Sec.Profile),
        ["PansionPicture"] = Of(ProviderRoleLabels.Pansion, Sec.Profile),
        ["PansionPet"] = Of(ProviderRoleLabels.Pansion, Sec.Profile),
        ["PansionReserve"] = Of(ProviderRoleLabels.Pansion, Sec.Reserve),
        ["PansionReserveApprove"] = Of(ProviderRoleLabels.Pansion, Sec.Reserve),
        ["PansionReserveReject"] = Of(ProviderRoleLabels.Pansion, Sec.Reserve),
        ["PansionReserveCancel"] = Of(ProviderRoleLabels.Pansion, Sec.Reserve),
        ["PansionReserveChangeStatus"] = Of(ProviderRoleLabels.Pansion, Sec.Reserve),
        ["PansionComment"] = Of(ProviderRoleLabels.Pansion, Sec.Review),
        ["PansionSettlement"] = Of(ProviderRoleLabels.Pansion, Sec.Finance),
        ["PansionFinance"] = Of(ProviderRoleLabels.Pansion, Sec.Finance),

        // ------------------------------------------------ مدرسه
        ["School"] = Of(ProviderRoleLabels.School, Sec.Profile, actions: VenueLifecycleOwnerOnly),
        ["SchoolActive"] = Of(ProviderRoleLabels.School, Sec.Profile),
        ["SchoolPicture"] = Of(ProviderRoleLabels.School, Sec.Profile),
        ["SchoolCourse"] = Of(ProviderRoleLabels.School, Sec.Course),
        ["SchoolCourseSession"] = Of(ProviderRoleLabels.School, Sec.Course),
        ["SchoolCourseVideo"] = Of(ProviderRoleLabels.School, Sec.Course),
        ["SchoolCoursePet"] = Of(ProviderRoleLabels.School, Sec.Course),
        // لایو و حضور/مهارت/کارنامه را مدرسِ دوره هم با هویت خودش انجام می‌دهد (سرویس CanTeachCourse/SchoolOfSession را چک می‌کند)
        ["SchoolCourseLive"] = Of(ProviderRoleLabels.School, Sec.Course, staffAllowed: true),
        ["SchoolTeaching"] = Of(ProviderRoleLabels.School, Sec.Course, staffAllowed: true,
            actions: new Dictionary<string, ProviderEndpointRule>(StringComparer.OrdinalIgnoreCase)
            {
                ["MyCourses"] = new(ProviderEndpointMode.Personal, StaffAllowed: true),
                ["AddInstructor"] = new(ProviderEndpointMode.Permission, Key(ProviderRoleLabels.School, Sec.Course, ProviderPermissionCatalog.Manage)),
                ["RemoveInstructor"] = new(ProviderEndpointMode.Permission, Key(ProviderRoleLabels.School, Sec.Course, ProviderPermissionCatalog.Manage)),
                ["Waitlist"] = new(ProviderEndpointMode.Permission, Key(ProviderRoleLabels.School, Sec.Reserve, ProviderPermissionCatalog.View)),
                // داشبورد درآمد را هم نشان می‌دهد
                ["Dashboard"] = new(ProviderEndpointMode.Permission, Key(ProviderRoleLabels.School, Sec.Finance, ProviderPermissionCatalog.View)),
            }),
        ["SchoolReserve"] = Of(ProviderRoleLabels.School, Sec.Reserve),
        ["SchoolSettlement"] = Of(ProviderRoleLabels.School, Sec.Finance),

        // ------------------------------------------------ کارهای شخصیِ دکتر/اپراتور/مدرس (همیشه هویت واقعی)
        ["ConsultationOnline"] = Personal,
        ["ConsultationSession"] = Personal,
        ["OnlinePrescription"] = Personal,
        ["CompanionDaySchedule"] = Personal,
        // ثبت دریافت بدهی رزرو: مالک، یا پرسنلِ مسئولِ همان رزرو با هویت خودش (سرویس چک می‌کند). نوع رزرو در بدنه است، پس به کارمند پنل داده نمی‌شود.
        ["CompanionReserveDebt"] = new(ProviderEndpointMode.OwnerOnly, StaffAllowed: true),

        // ------------------------------------------------ جدول‌های پایه
        ["WeekDay"] = LookupReadOnly,
        ["CompanionType"] = OwnerOnly, // نوع‌های اختصاصیِ نمایندگی قدیمی، نه جدول پایه

        // ------------------------------------------------ مدیریت کارمندان/نقش‌ها: فقط مالک (هیچ کارمندی حتی با همه‌ی دسترسی‌ها)
        ["ProviderPermission"] = OwnerOnly,
        ["ProviderUserRole"] = OwnerOnly,
        ["ProviderUser"] = OwnerOnly,

        // ------------------------------------------------ نمایندگی قدیمی (بازنشسته، ۲۰۲۶-۱۱-۰۱) و مالی سطح مالک
        ["Companion"] = OwnerOnly,
        ["CompanionAssistance"] = OwnerOnly,
        ["CompanionAssistancePackage"] = OwnerOnly,
        ["CompanionAssistancePackageOnline"] = OwnerOnly,
        ["CompanionAssistancePackageOnlineSelection"] = OwnerOnly,
        ["CompanionAssistancePackagePicture"] = OwnerOnly,
        ["CompanionAssistanceTime"] = OwnerOnly,
        ["CompanionAssistanceUser"] = OwnerOnly,
        ["CompanionComment"] = OwnerOnly,
        ["CompanionInsuranceManualPay"] = OwnerOnly,
        ["CompanionInsurancePackage"] = OwnerOnly,
        ["CompanionInsurancePackageSale"] = OwnerOnly,
        ["CompanionPet"] = OwnerOnly,
        ["CompanionReserve"] = OwnerOnly,
        ["CompanionReserveAssign"] = OwnerOnly,
        ["CompanionReserveAssignee"] = OwnerOnly,
        ["CompanionReserveCancel"] = OwnerOnly,
        ["CompanionReserveChangeState"] = OwnerOnly,
        ["CompanionReserveComment"] = OwnerOnly,
        ["CompanionReserveCompanionUpdate"] = OwnerOnly,
        ["CompanionReservePackage"] = OwnerOnly,
        ["CompanionReserveReschedule"] = OwnerOnly,
        ["CompanionTime"] = OwnerOnly,
        ["CompanionUser"] = OwnerOnly,
        ["CompanionUserGetMobile"] = OwnerOnly,
        ["CompanionZone"] = OwnerOnly,
        ["ConsultationAvailability"] = OwnerOnly,
        ["ConsultationPackage"] = OwnerOnly,
        ["FinanceCompanion"] = OwnerOnly,
        ["Settlement"] = OwnerOnly,
    };

    private static readonly Dictionary<string, ControllerRule> Seller = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Store"] = Of(ProviderRoleLabels.Store, Sec.Profile),
        ["StoreWorkingHour"] = Of(ProviderRoleLabels.Store, Sec.Profile),
        ["Delivery"] = Of(ProviderRoleLabels.Store, Sec.Profile),

        ["Product"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["ProductAdmin"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["ProductItem"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["ProductVariety"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["ProductImage"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["AiProductMatch"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["MissingProduct"] = Of(ProviderRoleLabels.Store, Sec.Product),
        ["Brand"] = LookupReadOnly,

        ["ProductOrder"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderStore"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderCancel"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderCancelRequest"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderChangeDescriptions"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderChangeState"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderChangeStatus"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderItemAdjust"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["ProductOrderTrackingCode"] = Of(ProviderRoleLabels.Store, Sec.Order),
        ["Shipment"] = Of(ProviderRoleLabels.Store, Sec.Order),

        ["Discount"] = Of(ProviderRoleLabels.Store, Sec.Discount),
        ["DiscountGroup"] = Of(ProviderRoleLabels.Store, Sec.Discount),

        ["StoreComment"] = Of(ProviderRoleLabels.Store, Sec.Review),

        ["FinanceStore"] = Of(ProviderRoleLabels.Store, Sec.Finance),
        ["Settlement"] = Of(ProviderRoleLabels.Store, Sec.Finance),
    };

    public static bool IsProviderArea(string area) =>
        area != null && Areas.Contains(area, StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, ControllerRule> MapOf(string area) =>
        string.Equals(area, "Seller", StringComparison.OrdinalIgnoreCase) ? Seller
        : string.Equals(area, "Companion", StringComparison.OrdinalIgnoreCase) ? Companion
        : null;

    public static bool IsMapped(string area, string controller) => MapOf(area)?.ContainsKey(controller ?? string.Empty) == true;

    public static ProviderEndpointRule Resolve(string area, string controller, string action, string httpMethod)
    {
        var map = MapOf(area);
        if (map == null || controller == null || !map.TryGetValue(controller, out var rule))
            return new ProviderEndpointRule(ProviderEndpointMode.OwnerOnly);

        if (rule.Actions != null && action != null && rule.Actions.TryGetValue(action, out var actionRule))
            return actionRule;

        var isRead = HttpMethods.IsGet(httpMethod ?? string.Empty) || HttpMethods.IsHead(httpMethod ?? string.Empty);
        switch (rule.Mode)
        {
            case ProviderEndpointMode.Permission:
                // بخشی که «مدیریت» ندارد (حسابداری) برای متد غیرخواندنی کلید ناشناخته می‌سازد → همیشه رد
                var level = isRead ? ProviderPermissionCatalog.View : ProviderPermissionCatalog.Manage;
                return new ProviderEndpointRule(ProviderEndpointMode.Permission, Key(rule.Scope, rule.Section, level), rule.StaffAllowed);
            case ProviderEndpointMode.Lookup:
                return isRead ? new ProviderEndpointRule(ProviderEndpointMode.Lookup) : new ProviderEndpointRule(ProviderEndpointMode.OwnerOnly);
            default:
                return new ProviderEndpointRule(rule.Mode, null, rule.StaffAllowed);
        }
    }
}
