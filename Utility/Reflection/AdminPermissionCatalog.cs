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
            new(6, "مدیریت محتوا", "ContentManagement", 6),
            new(7, "مدیریت یادآورها", "ReminderManagement", 7),
            new(8, "مدیریت مالی", "FinancialManagement", 8),
            new(9, "مدیریت موقعیت ها", "LocationManagement", 9),
            new(10, "مدیریت پاستیل فرند", "PastilMatchManagement", 10)
        ];

        private static readonly HashSet<string> MenuControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Address",
            "Assistance",
            "AssistanceQuestionnaire",
            "Banner",
            "Brand",
            "ClubReward",
            "Category",
            "City",
            "Companion",
            "CompanionReserve",
            "ContactUs",
            "Country",
            "Delivery",
            "Detail",
            "Discount",
            "DiscussionQuestion",
            "Driver",
            "Feature",
            "Finance",
            "Neighborhood",
            "Newsletter",
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
            "Product",
            "ProductComment",
            "ProductOrder",
            "PushMessage",
            "Rebate",
            "Reminder",
            "ReminderCycle",
            "ReminderType",
            "Role",
            "Settlement",
            "State",
            "StaticPage",
            "Store",
            "StoryGroup",
            "Ticket",
            "User",
            "UserBankCard",
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
                "PushMessage",
                "PushBroadcast",
                "Permission",
                "RolePermission",
                "Role",
                "Category",
                "CodeGroup",
                "Code",
                "TicketChangeImportant",
                "TicketAdmin",
                "TicketChangeStatus",
                "Bank",
                "Ticket",
                "TicketChangeAdmin",
                "TicketMessage",
                "BaseDetail",
                "NotifyMessage"
            ]);

            AddGroup(result, "UserManager",
            [
                "UserPet",
                "Driver",
                "User",
                "ClubReward",
                "DriverUpdateStatus"
            ]);

            AddGroup(result, "PetManagement",
            [
                "Pet",
                "PetBreed",
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
                "PansionActive",
                "CompanionActivation",
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
                "CompanionType",
                "PriceCalculation"
            ]);

            AddGroup(result, "ShopManagement",
            [
                "Delivery",
                "ProductReport",
                "DiscussionQuestion",
                "DiscussionAnswer",
                "Trip",
                "TripOption",
                "Variety",
                "TripStop",
                "ProductComment",
                "Store",
                "StoreComment",
                "Feature",
                "Cargo",
                "Rebate",
                "Product",
                "Brand",
                "ProductFile",
                "Discount",
                "DiscountGroup",
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
                "TripAddress",
                "TripChangeStatus",
                "TripChooseDriver",
                "TripShare",
                "UpdateCargoStatus",
                "VarietyItem"
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
