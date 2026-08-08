namespace Entities.Entities.PastilAIField
{
    public enum PastilAiPlanCode
    {
        Free = 0,
        Plus = 1,
        Pro = 2
    }

    public enum PastilAiSubscriptionStatus
    {
        PendingPayment = 0,
        Active = 1,
        Expired = 2,
        Cancelled = 3,
        PaymentFailed = 4
    }

    public enum PastilAiMessageRole
    {
        User = 0,
        Assistant = 1,
        System = 2
    }

    public enum PastilAiMessageStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Rejected = 3
    }

    public enum PastilAiInputType
    {
        Text = 0,
        Image = 1,
        Audio = 2,
        Video = 3
    }

    public enum PastilAiScope
    {
        Unknown = 0,
        PastilData = 1,
        PetGeneral = 2,
        PetMedical = 3,
        NearbyService = 4,
        OutOfScope = 5
    }

    public enum PastilAiProviderAttemptStatus
    {
        Started = 0,
        Succeeded = 1,
        Failed = 2,
        Skipped = 3
    }
}
