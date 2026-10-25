namespace Utility.Reflection
{
    internal sealed record PermissionParentDefinition(
        long Id,
        string Name,
        string Label,
        int Priority);

    internal sealed record ControllerPermissionDefinition(
        PermissionParentDefinition Parent,
        int Priority,
        bool IsMenu,
        string HostController);

    // یک «بخش» پنل: کنترلر اصلی + کنترلرهای کمکی‌اش (فعال‌سازی، تأیید، تغییر وضعیت، تصاویر، اکسل و…).
    // کنترلرهای کمکی در درخت دسترسی زیر همان بخش می‌نشینند، نه هم‌سطح آن زیر گروه.
    internal sealed record PermissionSectionDefinition(
        string Controller,
        IReadOnlyList<string> Children);

    internal static class AdminPermissionCatalog
    {
        internal static readonly IReadOnlyList<PermissionParentDefinition> Parents =
        [
            new(1, "تنظیمات سیستم", "Settings", 1),
            new(2, "مدیریت کاربران", "UserManager", 2),
            new(3, "مدیریت پت ها", "PetManagement", 3),
            new(4, "مدیریت نمایندگی‌ها", "CompanionManagement", 4),
            new(5, "مدیریت فروشگاه", "ShopManagement", 5),
            new(11, "مدیریت PastilAI", "PastilAIManagement", 6),
            new(6, "مدیریت محتوا", "ContentManagement", 7),
            new(7, "مدیریت یادآورها", "ReminderManagement", 8),
            new(8, "مدیریت مالی", "FinancialManagement", 9),
            new(9, "مدیریت موقعیت ها", "LocationManagement", 10),
            new(10, "مدیریت پاستیل فرند", "PastilMatchManagement", 11),
            new(12, "مدیریت سایت", "SiteManagement", 12),
            new(13, "مدیریت پاستیل کلاب", "PastilClubManagement", 13),
            new(14, "مدیریت پت رسان", "TripManagement", 14)
        ];

        // sync فقط هنگام «ساخت» permission مقدار IsMenu را می‌گذارد و برای ردیف موجود دستش نمی‌زند (تا ویرایش دستی
        // حفظ شود). کنترلری که اول بدون منو ساخته شده و بعداً وارد MenuControllers شده باید یک‌بار اجباراً منو شود؛
        // برای همین اینجا فهرست می‌شود (idempotent: بعد از اولین sync دیگر تغییری نمی‌دهد).
        internal static readonly HashSet<string> ForceMenuOnSync = new(StringComparer.OrdinalIgnoreCase)
        {
            "MissingProduct",
            "DiscountGroup",
            "CompanionReserveComment",
            "Provider",
            "ProviderRole",
            "ProviderFinance",
            "SubscriptionPlan",
        };

        // «Store» عمداً منو ندارد: فروشگاه‌ها تب «فروشگاه‌ها» داخل صفحه‌ی «نمایندگی‌ها» (/admin/companion?kind=store) هستند.
        private static readonly HashSet<string> MenuControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "CompanionReserveComment",
            "Provider",
            "ProviderRole",
            "ProviderFinance",
            "SubscriptionPlan",
            "Address",
            "Assistance",
            "AssistanceQuestionnaire",
            "Banner",
            "Brand",
            "Category",
            "City",
            "Companion",
            "CompanionReserve",
            "CompanionReservePackage",
            "ClinicReservePackage",
            "Expertise",
            "ContactUs",
            "PetMicrochipRequest",
            "Country",
            "Delivery",
            "Detail",
            "Discount",
            "DiscountGroup",
            "Driver",
            "Trip",
            "PetResanService",
            "TripOption",
            "TripStop",
            "PriceCalculation",
            "CompanionAssistancePackageOnline",
            "DiscussionQuestion",
            "PastilAI",
            "Feature",
            "Finance",
            "FinanceReport",
            "Neighborhood",
            "PansionReserve",
            "SchoolReserve",
            "GroomerReserve",
            "ClinicReserve",
            "CoachReserve",
            "Park",
            "PastilMatch",
            "PastilMatchBlock",
            "PastilMatchProfile",
            "PastilMatchReport",
            "PastilMatchRequest",
            "Permission",
            "Pet",
            "PetBreed",
            "PetTag",
            "Product",
            "MissingProduct",
            "ConsultationPurchase",
            "ConsultationPackage",
            "ConsultationVenue",
            "ProductOrder",
            "PushMessage",
            "Rebate",
            "Reminder",
            "ReminderCycle",
            "ReminderType",
            "Role",
            "State",
            "SiteDashboard",
            "SitePost",
            "SiteGallery",
            "SiteBanner",
            "SiteCompanion",
            "SiteAssistance",
            "SitePansion",
            "SiteStore",
            "StoryGroup",
            "Ticket",
            "User",
            "UserMemory",
            "PastilClubPointRule",
            "PastilClubPointTransaction",
            "PastilClubRewardTemplate",
            "PastilClubRewardOffer",
            "PastilClubRewardRedemption",
            "Variety",
            "Wallet"
        };

        private static readonly IReadOnlyDictionary<string, ControllerPermissionDefinition> Controllers =
            BuildControllers();

        internal static bool TryGetController(
            string controller,
            out ControllerPermissionDefinition definition)
        {
            return Controllers.TryGetValue(controller, out definition!);
        }

        private static IReadOnlyDictionary<string, ControllerPermissionDefinition> BuildControllers()
        {
            var result = new Dictionary<string, ControllerPermissionDefinition>(
                StringComparer.OrdinalIgnoreCase);

            AddGroup(result, "Settings",
            [
                S("Role", "RolePermission"),
                S("Permission", "PermissionSync"),
                S("Category", "CategoryFeature", "CategoryLanguage", "CategoryVariety"),
                S("Ticket", "TicketAdmin", "TicketMessage", "TicketChangeStatus", "TicketChangeImportant", "TicketChangeAdmin"),
                S("PushMessage", "PushBroadcast", "PushDiagnostics", "NotifyMessage"),
                S("CodeGroup", "Code"),
                S("Bank"),
                S("BaseDetail"),
                S("ClubReward"),
                S("SearchAnalytics"),
                S("ServerMonitoring"),
            ]);

            AddGroup(result, "UserManager",
            [
                S("User", "UserStats", "UserDebt", "UserAnalytics"),
                S("UserPet", "UserPetRecord", "UserPetPicture"),
                S("UserMemory"),
            ]);

            AddGroup(result, "PetManagement",
            [
                S("Pet"),
                S("PetBreed", "PetBreedCharacteristic"),
                S("PetTag", "PetTagExcel"),
                S("PetMicrochipRequest"),
            ]);

            AddGroup(result, "CompanionManagement",
            [
                S("Companion",
                    "CompanionActivation", "CompanionUpdateGoldAccount", "CompanionUpdateSilverAccount",
                    "CompanionPet", "CompanionTime", "CompanionZone", "CompanionComment", "CompanionReport", "Agency"),
                S("CompanionUser", "CompanionUserAvailable"),
                S("CompanionType"),
                S("Provider", "ProviderRoleAssignment", "ProviderSubscription", "ProviderVenue", "ProviderOverview", "ProviderOnboarding"),
                S("ProviderRole"),
                S("Store", "StoreApproval", "StoreUser", "StoreComment"),
                S("SubscriptionPlan"),
                S("Assistance", "AssistanceExpertise"),
                S("AssistanceGroup"),
                S("Expertise"),
                S("AssistanceQuestionnaire"),
                S("CompanionAssistance",
                    "CompanionAssistanceActivation", "CompanionAssistanceTime", "CompanionAssistanceReport"),
                S("CompanionAssistanceUser", "CompanionAssistanceUserActivation"),
                S("CompanionAssistancePackage",
                    "CompanionAssistancePackageActivation", "CompanionAssistancePackagePicture"),
                S("CompanionAssistancePackageOnline",
                    "CompanionAssistancePackageOnlineSelection", "CompanionAssistancePackageOnlineSelectionActivation"),
                S("CompanionInsurancePackage",
                    "CompanionInsurancePackageActivation", "CompanionInsurancePackageSale", "CompanionInsuranceManualPay"),
                S("Pansion", "PansionActive", "PansionApprove", "PansionPet", "PansionPicture", "PansionComment"),
                S("Groomer",
                    "GroomerActive", "GroomerApprove", "GroomerSetting", "GroomerService", "GroomerSchedule",
                    "GroomerUser", "GroomerPicture", "GroomerPortfolio", "GroomerPortfolioComment", "GroomerReview"),

                S("Clinic",
                    "ClinicActive", "ClinicApprove", "ClinicSetting", "ClinicService", "ClinicSchedule",
                    "ClinicUser", "ClinicPicture", "ClinicReview"),
                S("Coach",
                    "CoachActive", "CoachApprove", "CoachSetting", "CoachService", "CoachSchedule",
                    "CoachUser", "CoachPicture", "CoachReview"),
                S("School",
                    "SchoolActive", "SchoolApprove", "SchoolPicture",
                    "SchoolCourse", "SchoolCourseSession", "SchoolCourseVideo", "SchoolCoursePet"),
                S("Notice"),
            ]);

            AddGroup(result, "ShopManagement",
            [
                S("Product",
                    "ProductPicture", "ProductFile", "ProductItem", "ProductRelate", "ProductFeatureValue",
                    "ProductChangeVariety", "ProductVariety", "ProductsExcel", "ProductComment", "ProductReport"),
                S("MissingProduct"),
                S("Brand", "BrandLanguage", "BrandCategory"),
                S("Variety", "VarietyLanguage", "VarietyItem"),
                S("Feature", "FeatureItem"),
                S("Delivery", "DeliveryDistance", "ShippingSlot"),
                S("Cargo", "UpdateCargoStatus"),
                S("Discount"),
                S("DiscountGroup"),
                S("DiscussionQuestion", "DiscussionAnswer", "DiscussionAnswerActive"),
            ]);

            AddGroup(result, "PastilAIManagement",
            [
                S("PastilAI"),
            ]);

            AddGroup(result, "ContentManagement",
            [
                S("StoryGroup", "StoryItem"),
                S("Post", "PostFile", "PostPicture", "PostProduct", "PostComment", "Hashtag"),
                S("Banner"),
                S("StaticPage"),
                S("Detail"),
                S("Gallery", "GalleryItem"),
                S("ContactUs", "ContactUsGroup"),
                S("Newsletter"),
                S("WeekDay"),
                S("File"),
                S("Picture"),
            ]);

            AddGroup(result, "ReminderManagement",
            [
                S("Reminder"),
                S("ReminderType"),
                S("ReminderCycle"),
            ]);

            AddGroup(result, "FinancialManagement",
            [
                S("Finance",
                    "FinanceStore", "FinanceCompanion", "FinanceCompanionAssistance", "FinanceConsultationPackage",
                    "FinancePansion", "FinanceDriver", "Settlement", "ManualPayment", "ManualTripPayment"),
                S("FinanceReport"),
                S("ProviderFinance"),
                S("ProductOrder",
                    "ProductOrderItem", "ProductOrderItemAdjust", "ProductOrderStore", "ProductOrderChangeStatus",
                    "ProductOrderChangeState", "ProductOrderChangeDescriptions", "ProductOrderTrackingCode",
                    "ProductOrderCancelRequest", "ProductOrderCancel", "OrderShipment"),
                // رزرو نمایندگی‌ها + هرچه به رزرو و مشاوره‌ی آنلاین مربوط است؛ مشاوره‌ها فقط این‌جا هستند، نه زیر «مدیریت نمایندگی‌ها».
                S("CompanionReserve",
                    "CompanionReserveUnified", "CompanionReserveAssign", "CompanionReserveAssignee",
                    "CompanionReserveChangeState", "CompanionReserveReschedule", "CompanionReserveCancel",
                    "CompanionReserveExcel", "CompanionReserveComment",
                    "ConsultationPurchase", "ConsultationPackage", "ConsultationVenue"),
                S("CompanionReservePackage"),
                S("ClinicReservePackage"),
                S("PansionReserve",
                    "PansionReserveApprove", "PansionReserveReject", "PansionReserveChangeStatus", "PansionReserveCancel"),
                S("SchoolReserve", "SchoolReserveChangeStatus", "SchoolReserveCancel"),
                S("GroomerReserve"),
                S("ClinicReserve"),
                S("CoachReserve"),
                S("Wallet"),
                S("UserBankCard", "UserBankCardApprove", "BankCard"),
                S("Merchant"),
            ]);

            AddGroup(result, "LocationManagement",
            [
                S("Country"),
                S("State"),
                S("City", "CityLanguage"),
                S("Neighborhood"),
                S("Park", "ParkPicture"),
                S("Address"),
            ]);

            AddGroup(result, "PastilMatchManagement",
            [
                S("PastilMatchProfile",
                    "PastilMatchProfileActive", "PastilMatchProfileVerification", "PastilMatchProfileGoal"),
                S("PastilMatchRequest"),
                S("PastilMatch"),
                S("PastilMatchBlock"),
                S("PastilMatchMessage", "PastilMatchMessageAttachment", "PastilMatchMessageReaction"),
                S("PastilMatchReport", "PastilMatchReportReview"),
                S("PastilMatchReportReason", "PastilMatchReportReasonActive"),
            ]);

            AddGroup(result, "SiteManagement",
            [
                S("SiteDashboard"),
                S("SitePost"),
                S("SiteGallery"),
                S("SiteBanner"),
                S("SiteCompanion"),
                S("SiteAssistance"),
                S("SitePansion"),
                S("SiteStore"),
                S("SiteSchool"),
            ]);

            AddGroup(result, "PastilClubManagement",
            [
                S("PastilClubPointRule"),
                S("PastilClubPointTransaction", "PastilClubPointIncrease", "PastilClubPointDecrease"),
                S("PastilClubRewardTemplate"),
                S("PastilClubRewardOffer"),
                S("PastilClubRewardRedemption",
                    "PastilClubRewardApprove", "PastilClubRewardReject",
                    "PastilClubRewardBulkApprove", "PastilClubRewardBulkReject"),
                S("Rebate"),
            ]);

            AddGroup(result, "TripManagement",
            [
                S("Driver", "DriverUpdateStatus"),
                S("Trip", "TripLive", "TripAddress", "TripChangeStatus", "TripChooseDriver", "TripShare"),
                S("PetResanService"),
                S("TripOption"),
                S("TripStop"),
                S("PriceCalculation"),
            ]);

            return result;
        }

        private static PermissionSectionDefinition S(string controller, params string[] children) =>
            new(controller, children);

        // اولویت زیربخش‌ها بعد از actionهای خودِ بخش است تا در درخت دسترسی زیر «مشاهده/افزودن/ویرایش/حذف» بیایند.
        internal const int ChildControllerPriorityOffset = 100;

        private static void AddGroup(
            IDictionary<string, ControllerPermissionDefinition> target,
            string parentLabel,
            IReadOnlyList<PermissionSectionDefinition> sections)
        {
            var parent = Parents.Single(x =>
                x.Label.Equals(parentLabel, StringComparison.OrdinalIgnoreCase));

            for (var index = 0; index < sections.Count; index++)
            {
                var section = sections[index];
                target.Add(
                    section.Controller,
                    new ControllerPermissionDefinition(
                        parent,
                        index + 1,
                        MenuControllers.Contains(section.Controller),
                        null));

                for (var childIndex = 0; childIndex < section.Children.Count; childIndex++)
                {
                    var child = section.Children[childIndex];
                    target.Add(
                        child,
                        new ControllerPermissionDefinition(
                            parent,
                            ChildControllerPriorityOffset + childIndex + 1,
                            MenuControllers.Contains(child),
                            section.Controller));
                }
            }
        }
    }
}
