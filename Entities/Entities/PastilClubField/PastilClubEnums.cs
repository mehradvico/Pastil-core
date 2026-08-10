namespace Entities.Entities.PastilClubField
{
    public enum ClubPointTransactionTypeEnum
    {
        Earn = 1,
        Spend = 2,
        ReverseEarn = 3,
        RefundSpend = 4,
        DebtCreated = 5,
        DebtPaid = 6,
        ManualIncrease = 7,
        ManualDecrease = 8,
        ReferralEarn = 9,
        Adjustment = 10
    }

    public enum ClubPointSourceTypeEnum
    {
        None = 0,
        ProductOrder = 1,
        CompanionReservation = 2,
        PansionReservation = 3,
        PetProfile = 4,
        Memory = 5,
        UserReferral = 6,
        BusinessReferral = 7,
        RewardRedemption = 8,
        Admin = 9,
        System = 10
    }

    public enum ClubPointEventTypeEnum
    {
        ProductOrderCompleted = 1,
        CompanionReservationCompleted = 2,
        PansionReservationCompleted = 3,
        PetProfileCompleted = 4,
        MemoryCreated = 5,
        UserReferralReferrer = 6,
        UserReferralReferee = 7,
        BusinessReferralUser = 8
    }

    public enum ClubRewardTypeEnum
    {
        FixedDiscount = 1,
        PercentageDiscount = 2,
        FreeDelivery = 3,
        PromotionalWalletCredit = 4,
        PastilAIPlanFixedDiscount = 5,
        PastilAIPlanPercentageDiscount = 6,
        PastilAIFreeDays = 7,
        PastilAIFreeMonth = 8,
        PastilAIUpgrade = 9
    }

    public enum ClubRewardTargetTypeEnum
    {
        Global = 1,
        Store = 2,
        Product = 3,
        ProductCategory = 4,
        Companion = 5,
        Assistance = 6,
        CompanionPackage = 7,
        Pansion = 8,
        PastilAI = 9,
        PastilAIPlan = 10,
        City = 11
    }

    public enum ClubRewardExpirationTypeEnum
    {
        EndOfDay = 1,
        SevenDays = 2,
        TenDays = 3,
        ThirtyDays = 4,
        FixedDate = 5
    }

    public enum ClubRewardFundingTypeEnum
    {
        Pastil = 1
    }

    public enum ClubRewardNotificationLevelEnum
    {
        Normal = 1,
        HighValue = 2,
        VeryHighValue = 3
    }

    public enum ClubRewardOfferSourceEnum
    {
        ManualAdmin = 1,
        Automation = 2
    }

    public enum ClubRewardOfferStatusEnum
    {
        PendingApproval = 1,
        Approved = 2,
        Rejected = 3,
        Redeemed = 4,
        Expired = 5,
        Cancelled = 6
    }

    public enum ClubRewardRedemptionStatusEnum
    {
        Completed = 1,
        Failed = 2,
        Cancelled = 3,
        Expired = 4
    }

    public enum ClubRewardBenefitTypeEnum
    {
        Discount = 1,
        FreeDelivery = 2,
        PromotionalWalletCredit = 3,
        PastilAI = 4
    }

    public enum ClubRewardApplicationMethodEnum
    {
        ProductOrder = 1,
        CompanionReservation = 2,
        PansionReservation = 3,
        PastilAI = 4
    }

    public enum ClubPromotionalCreditStatusEnum
    {
        Active = 1,
        Consumed = 2,
        Expired = 3,
        Cancelled = 4
    }
}
