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
        bool IsMenu);

    internal static class AdminPermissionCatalog
    {
        internal static readonly IReadOnlyList<PermissionParentDefinition> Parents =
        [
            new(1, "تنظیمات سیستم", "Settings", 1),
            new(2, "مدیریت کاربران", "UserManager", 2),
            new(3, "مدیریت پت ها", "PetManagement", 3),
            new(4, "مدیریت نمایندگان", "CompanionManagement", 4),
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

        private static readonly HashSet<string> MenuControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Address",
            "Assistance",
            "AssistanceQuestionnaire",
            "Banner",
            "Brand",
            "Category",
            "City",
            "Companion",
            "CompanionReserve",
            "Expertise",
            "ContactUs",
            "Country",
            "Delivery",
            "Detail",
            "Discount",
            "Driver",
            "Trip",
            "TripOption",
            "TripStop",
            "PriceCalculation",
            "DiscussionQuestion",
            "PastilAI",
            "Feature",
            "Finance",
            "Neighborhood",
            "Pansion",
            "PansionReserve",
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
            "ProductOrder",
            "PushMessage",
            "Rebate",
            "Reminder",
            "ReminderCycle",
            "ReminderType",
            "Role",
            "State",
            "Store",
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
                "Role",
                "Permission",
                "PermissionSync",
                "Category",
                "Ticket",
                "PushMessage",
                "PushBroadcast",
                "RolePermission",
                "CodeGroup",
                "Code",
                "TicketChangeImportant",
                "TicketAdmin",
                "TicketChangeStatus",
                "Bank",
                "TicketChangeAdmin",
                "TicketMessage",
                "BaseDetail",
                "ClubReward",
                "NotifyMessage",
                "SearchAnalytics"
            ]);

            AddGroup(result, "UserManager",
            [
                "User",
                "UserPet",
                "UserMemory"
            ]);

            AddGroup(result, "PetManagement",
            [
                "Pet",
                "PetBreed",
                "PetTag",
                "PetTagExcel",
                "UserPetPicture",
                "UserPetRecord"
            ]);

            AddGroup(result, "CompanionManagement",
            [
                "Companion",
                "CompanionInsurancePackage",
                "CompanionInsuranceManualPay",
                "PansionPet",
                "PansionPicture",
                "CompanionUserAvailable",
                "CompanionReport",
                "Assistance",
                "AssistanceGroup",
                "Expertise",
                "PansionActive",
                "CompanionActivation",
                "CompanionReserveAssign",
                "CompanionReserveAssignee",
                "CompanionAssistance",
                "CompanionAssistancePackageActivation",
                "CompanionAssistanceUserActivation",
                "CompanionInsurancePackageActivation",
                "PansionApprove",
                "CompanionAssistanceReport",
                "CompanionUpdateGoldAccount",
                "CompanionInsurancePackageSale",
                "CompanionAssistanceTime",
                "CompanionUpdateSilverAccount",
                "Pansion",
                "CompanionAssistancePackage",
                "PansionComment",
                "CompanionAssistanceUser",
                "CompanionZone",
                "CompanionUser",
                "CompanionReserveComment",
                "Notice",
                "AssistanceQuestionnaire",
                "CompanionAssistanceActivation",
                "CompanionAssistancePackagePicture",
                "CompanionComment",
                "CompanionPet",
                "CompanionType"
            ]);

            AddGroup(result, "ShopManagement",
            [
                "Store",
                "StoreApproval",
                "Product",
                "Brand",
                "BrandLanguage",
                "Variety",
                "VarietyLanguage",
                "Feature",
                "Delivery",
                "Discount",
                "DiscussionQuestion",
                "StoreComment",
                "ProductReport",
                "DiscussionAnswer",
                "ProductComment",
                "Cargo",
                "ProductFile",
                "DiscountGroup",
                "CategoryLanguage",
                "ProductPicture",
                "BrandCategory",
                "CategoryFeature",
                "DeliveryDistance",
                "DiscussionAnswerActive",
                "FeatureItem",
                "ProductChangeVariety",
                "ProductFeatureValue",
                "ProductItem",
                "ProductRelate",
                "ProductsExcel",
                "StoreUser",
                "UpdateCargoStatus",
                "VarietyItem"
            ]);

            AddGroup(result, "PastilAIManagement",
            [
                "PastilAI"
            ]);

            AddGroup(result, "ContentManagement",
            [
                "StoryItem",
                "StoryGroup",
                "WeekDay",
                "Post",
                "PostFile",
                "Banner",
                "StaticPage",
                "PostComment",
                "ContactUsGroup",
                "ContactUs",
                "Detail",
                "Gallery",
                "GalleryItem",
                "File",
                "Picture",
                "Hashtag",
                "PostProduct",
                "PostPicture",
                "Newsletter"
            ]);

            AddGroup(result, "ReminderManagement",
            [
                "Reminder",
                "ReminderType",
                "ReminderCycle"
            ]);

            AddGroup(result, "FinancialManagement",
            [
                "ProductOrderCancelRequest",
                "Finance",
                "FinanceStore",
                "UserBankCard",
                "ProductOrder",
                "ProductOrderChangeDescriptions",
                "FinanceCompanionAssistance",
                "ProductOrderChangeState",
                "CompanionReserve",
                "CompanionReserveCancel",
                "PansionReserveChangeStatus",
                "FinancePansion",
                "ProductOrderChangeStatus",
                "PansionReserve",
                "PansionReserveCancel",
                "ProductOrderTrackingCode",
                "Merchant",
                "ProductOrderItem",
                "BankCard",
                "CompanionReserveChangeState",
                "CompanionReserveExcel",
                "FinanceCompanion",
                "ManualTripPayment",
                "ManualPayment",
                "ProductOrderStore",
                "Settlement",
                "UserBankCardApprove",
                "Wallet"
            ]);

            AddGroup(result, "LocationManagement",
            [
                "Country",
                "State",
                "City",
                "CityLanguage",
                "ParkPicture",
                "Park",
                "Neighborhood",
                "Address"
            ]);

            AddGroup(result, "PastilMatchManagement",
            [
                "PastilMatchProfile",
                "PastilMatchProfileVerification",
                "PastilMatchProfileGoal",
                "PastilMatchReportReview",
                "PastilMatchProfileActive",
                "PastilMatchMessageAttachment",
                "PastilMatchRequest",
                "PastilMatch",
                "PastilMatchBlock",
                "PastilMatchReportReasonActive",
                "PastilMatchMessageReaction",
                "PastilMatchReport",
                "PastilMatchReportReason",
                "PastilMatchMessage"
            ]);

            AddGroup(result, "SiteManagement",
            [
                "SiteDashboard",
                "SitePost",
                "SiteGallery",
                "SiteBanner",
                "SiteCompanion",
                "SiteAssistance",
                "SitePansion",
                "SiteStore"
            ]);

            AddGroup(result, "PastilClubManagement",
            [
                "PastilClubPointRule",
                "PastilClubPointTransaction",
                "PastilClubPointIncrease",
                "PastilClubPointDecrease",
                "PastilClubRewardTemplate",
                "PastilClubRewardOffer",
                "PastilClubRewardRedemption",
                "PastilClubRewardApprove",
                "PastilClubRewardReject",
                "PastilClubRewardBulkApprove",
                "PastilClubRewardBulkReject",
                "Rebate"
            ]);

            AddGroup(result, "TripManagement",
            [
                "Driver",
                "DriverUpdateStatus",
                "Trip",
                "TripLive",
                "TripOption",
                "TripStop",
                "TripAddress",
                "TripChangeStatus",
                "TripChooseDriver",
                "TripShare",
                "PriceCalculation"
            ]);

            return result;
        }

        private static void AddGroup(
            IDictionary<string, ControllerPermissionDefinition> target,
            string parentLabel,
            IReadOnlyList<string> controllers)
        {
            var parent = Parents.Single(x =>
                x.Label.Equals(parentLabel, StringComparison.OrdinalIgnoreCase));

            for (var index = 0; index < controllers.Count; index++)
            {
                var controller = controllers[index];
                target.Add(
                    controller,
                    new ControllerPermissionDefinition(
                        parent,
                        index + 1,
                        MenuControllers.Contains(controller)));
            }
        }
    }
}
