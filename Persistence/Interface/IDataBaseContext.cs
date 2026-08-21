using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.LocationField;
using Entities.Entities.PansionField;
using Entities.Entities.PastilMatchField;
using Entities.Entities.PastilAIField;
using Entities.Entities.PastilClubField;
using Entities.Entities.Security;
using Entities.Entities.ShippingField;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Interface
{
    public interface IDataBaseContext : IDbContextTransactionManager
    {
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
        Task<long> GetNextPaymentCodeNumberAsync(CancellationToken cancellationToken = default);
        Task<long> GetNextBusinessCodeNumberAsync(CancellationToken cancellationToken = default);
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AdminSetting> AdminSettings { get; set; }
        public DbSet<AssistanceGroup> AssistanceGroups { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<AssistanceQuestionnaire> AssistanceQuestionnaires { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankCard> BankCards { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<BaseDetail> BaseDetails { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Cargo> Cargoes { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartStore> CartStores { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ClubReward> ClubRewards { get; set; }
        public DbSet<ClubPointAccount> ClubPointAccounts { get; set; }
        public DbSet<ClubPointRule> ClubPointRules { get; set; }
        public DbSet<ClubPointTransaction> ClubPointTransactions { get; set; }
        public DbSet<ClubRewardTemplate> ClubRewardTemplates { get; set; }
        public DbSet<ClubRewardTarget> ClubRewardTargets { get; set; }
        public DbSet<ClubRewardPetType> ClubRewardPetTypes { get; set; }
        public DbSet<ClubRewardOffer> ClubRewardOffers { get; set; }
        public DbSet<ClubRewardRedemption> ClubRewardRedemptions { get; set; }
        public DbSet<ClubCoupon> ClubCoupons { get; set; }
        public DbSet<ClubFreeDeliveryBenefit> ClubFreeDeliveryBenefits { get; set; }
        public DbSet<ClubPromotionalWalletCredit> ClubPromotionalWalletCredits { get; set; }
        public DbSet<ClubPromotionalCreditUsage> ClubPromotionalCreditUsages { get; set; }
        public DbSet<ClubRewardPastilAITarget> ClubRewardPastilAITargets { get; set; }
        public DbSet<ClubRewardCostTransaction> ClubRewardCostTransactions { get; set; }
        public DbSet<Code> Codes { get; set; }
        public DbSet<CodeGroup> CodeGroups { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Companion> Companions { get; set; }
        public DbSet<CompanionAssistance> CompanionAssistances { get; set; }
        public DbSet<CompanionAssistancePackage> CompanionAssistancePackages { get; set; }
        public DbSet<CompanionAssistancePackagePicture> CompanionAssistancePackagePictures { get; set; }
        public DbSet<CompanionAssistanceReport> CompanionAssistanceReports { get; set; }
        public DbSet<CompanionAssistanceTime> CompanionAssistanceTimes { get; set; }
        public DbSet<CompanionAssistanceUser> CompanionAssistanceUsers { get; set; }
        public DbSet<CompanionComment> CompanionComments { get; set; }
        public DbSet<CompanionInsurancePackage> CompanionInsurancePackages { get; set; }
        public DbSet<CompanionInsurancePackageSale> CompanionInsurancePackageSales { get; set; }
        public DbSet<CompanionPet> CompanionPets { get; set; }
        public DbSet<CompanionReport> CompanionReports { get; set; }
        public DbSet<CompanionReserve> CompanionReserves { get; set; }
        public DbSet<CompanionReserveComment> CompanionReserveComments { get; set; }
        public DbSet<CompanionReserveCommentRate> CompanionReserveCommentRates { get; set; }
        public DbSet<CompanionType> CompanionTypes { get; set; }
        public DbSet<CompanionUser> CompanionUsers { get; set; }
        public DbSet<CompanionZone> CompanionZones { get; set; }
        public DbSet<ContactUs> ContactUses { get; set; }
        public DbSet<ContactUsGroup> ContactUsGroups { get; set; }
        public DbSet<ContactUsItem> ContactUsItems { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryDistance> DeliveryDistances { get; set; }
        public DbSet<ShippingQuote> ShippingQuotes { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountGroup> DiscountGroups { get; set; }
        public DbSet<DiscussionAnswer> DiscussionAnswers { get; set; }
        public DbSet<DiscussionAnswerLike> DiscussionAnswerLikes { get; set; }
        public DbSet<DiscussionQuestion> DiscussionQuestions { get; set; }
        public DbSet<PastilAiPlan> PastilAiPlans { get; set; }
        public DbSet<PastilAiSubscription> PastilAiSubscriptions { get; set; }
        public DbSet<PastilAiConversation> PastilAiConversations { get; set; }
        public DbSet<PastilAiMessage> PastilAiMessages { get; set; }
        public DbSet<PastilAiAttachment> PastilAiAttachments { get; set; }
        public DbSet<PastilAiProviderAttempt> PastilAiProviderAttempts { get; set; }
        public DbSet<PastilAiDailyUsage> PastilAiDailyUsages { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverUser> DriverUsers { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<EmailHost> EmailHosts { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Expertise> Expertises { get; set; }
        public DbSet<FeatureItem> FeatureItems { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<MapKey> MapKeys { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<NoticeRead> NoticeReads { get; set; }
        public DbSet<NoticeType> NoticeTypes { get; set; }
        public DbSet<NotifyMessage> NotifyMessages { get; set; }
        public DbSet<OtpVerify> OtpVerifies { get; set; }
        public DbSet<Pansion> Pansions { get; set; }
        public DbSet<PansionComment> PansionComments { get; set; }
        public DbSet<PansionPet> PansionPets { get; set; }
        public DbSet<PansionPicture> PansionPictures { get; set; }
        public DbSet<PansionReserve> PansionReserves { get; set; }
        public DbSet<Park> Parks { get; set; }
        public DbSet<ParkPicture> ParkPictures { get; set; }
        public DbSet<PastilMatch> PastilMatches { get; set; }
        public DbSet<PastilMatchBlock> PastilMatchBlocks { get; set; }
        public DbSet<PastilMatchMessage> PastilMatchMessages { get; set; }
        public DbSet<PastilMatchMessageAttachment> PastilMatchMessageAttachments { get; set; }
        public DbSet<PastilMatchMessageReaction> PastilMatchMessageReactions { get; set; }
        public DbSet<PastilMatchProfile> PastilMatchProfiles { get; set; }
        public DbSet<PastilMatchProfileGoal> PastilMatchProfileGoals { get; set; }
        public DbSet<PastilMatchProfileLike> PastilMatchProfileLikes { get; set; }
        public DbSet<PastilMatchReport> PastilMatchReports { get; set; }
        public DbSet<PastilMatchReportReason> PastilMatchReportReasons { get; set; }
        public DbSet<PastilMatchRequest> PastilMatchRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetBreed> PetBreeds { get; set; }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostFile> PostFiles { get; set; }
        public DbSet<PostPicture> PostPictures { get; set; }
        public DbSet<PriceCalculation> PriceCalculations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<ProductFeatureValue> ProductFeatureValues { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }
        public DbSet<ProductLike> ProductLikes { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<ProductOrderItem> ProductOrderItems { get; set; }
        public DbSet<ProductOrderStore> ProductOrderStores { get; set; }
        public DbSet<ProductPicture> ProductPictures { get; set; }
        public DbSet<ProductRelate> ProductRelates { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }
        public DbSet<PushMessage> PushMessages { get; set; }
        public DbSet<PushNotification> PushNotifications { get; set; }
        public DbSet<PushPattern> PushPatterns { get; set; }
        public DbSet<PushSetting> PushSettings { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<PushType> PushTypes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Rebate> Rebate { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Memory> Memories { get; set; }
        public DbSet<ReminderCycle> ReminderCycles { get; set; }
        public DbSet<ReminderType> ReminderTypes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ScoreTransaction> ScoreTransactions { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<SettlementCompanion> SettlementCompanions { get; set; }
        public DbSet<SettlementStore> SettlementStores { get; set; }
        public DbSet<Sms> Smses { get; set; }
        public DbSet<SmsNumber> SmsNumbers { get; set; }
        public DbSet<SmsProvider> SmsProviders { get; set; }
        public DbSet<SmsSetting> SmsSettings { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<SearchQueryLog> SearchQueryLogs { get; set; }
        public DbSet<StoreComment> StoreComments { get; set; }
        public DbSet<StoryGroup> StoryGroups { get; set; }
        public DbSet<StoryItem> StoryItems { get; set; }
        public DbSet<StoryUserLike> StoryUserLikes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketItem> TicketItems { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripAddress> TripAddresses { get; set; }
        public DbSet<TripOption> TripOptions { get; set; }
        public DbSet<TripStop> TripStops { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserCurrentLocation> UserCurrentLocations { get; set; }
        public DbSet<UserBankCard> UserBankCards { get; set; }
        public DbSet<UserCategory> UserCategories { get; set; }
        public DbSet<UserPet> UserPets { get; set; }
        public DbSet<UserMemory> UserMemories { get; set; }
        public DbSet<UserPetPicture> UserPetPictures { get; set; }
        public DbSet<UserPetRecord> UserPetRecords { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }
        public DbSet<UserRebate> UserRebates { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Variety> Varieties { get; set; }
        public DbSet<VarietyItem> VarietyItems { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WeekDay> WeekDays { get; set; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry Entry(object entity);

        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
        void Dispose();
    }
}
