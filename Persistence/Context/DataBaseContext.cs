using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.PansionField;
using Entities.Entities.PastilMatchField;
using Entities.Entities.PastilAIField;
using Entities.Entities.PastilClubField;
using Entities.Entities.Security;
using Entities.Entities.ShippingField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Context
{
    public class DataBaseContext : DbContext, IDataBaseContext
    {

        public DataBaseContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            return Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public async Task<long> GetNextPaymentCodeNumberAsync(CancellationToken cancellationToken = default)
        {
            return await GetNextSequenceValueAsync("PaymentCodeSequence", cancellationToken);
        }

        public async Task<long> GetNextBusinessCodeNumberAsync(CancellationToken cancellationToken = default)
        {
            return await GetNextSequenceValueAsync("BusinessCodeSequence", cancellationToken);
        }

        private async Task<long> GetNextSequenceValueAsync(string sequenceName, CancellationToken cancellationToken)
        {
            var connection = Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
                await connection.OpenAsync(cancellationToken);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"SELECT NEXT VALUE FOR dbo.{sequenceName};";
                command.Transaction = Database.CurrentTransaction?.GetDbTransaction();
                var result = await command.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt64(result);
            }
            finally
            {
                if (shouldCloseConnection)
                    await connection.CloseAsync();
            }
        }

        public override int SaveChanges()
        {
            return SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            NormalizeSlugs();

            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (DbUpdateException exception) when (IsDuplicateSlug(exception))
            {
                throw new ValidationException(
                    "Slug ساخته‌شده تکراری است. مقدار Label را تغییر دهید.",
                    exception);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            NormalizeSlugs();

            try
            {
                return await base.SaveChangesAsync(
                    acceptAllChangesOnSuccess,
                    cancellationToken);
            }
            catch (DbUpdateException exception) when (IsDuplicateSlug(exception))
            {
                throw new ValidationException(
                    "Slug ساخته‌شده تکراری است. مقدار Label را تغییر دهید.",
                    exception);
            }
        }

        private void NormalizeSlugs()
        {
            foreach (var entry in ChangeTracker.Entries<ISlugEntity>()
                         .Where(x => x.State is not EntityState.Deleted and not EntityState.Detached))
            {
                var normalizedSlug = SlugNormalizer.Normalize(entry.Entity.GetSlugSource());

                if (!string.Equals(entry.Entity.Slug, normalizedSlug, StringComparison.Ordinal))
                    entry.Entity.Slug = normalizedSlug;
            }
        }

        private static bool IsDuplicateSlug(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException &&
                   sqlException.Number is 2601 or 2627 &&
                   sqlException.Message.Contains("Slug", StringComparison.OrdinalIgnoreCase);
        }

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
        public DbSet<ShippingQuote> ShippingQuotes { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<DeliveryDistance> DeliveryDistances { get; set; }
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
        public DbSet<PetTag> PetTags { get; set; }
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
        public DbSet<ProductStockAlert> ProductStockAlerts { get; set; }
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
        public DbSet<TripPet> TripPets { get; set; }
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


        public IDbContextTransaction CurrentTransaction => base.Database.CurrentTransaction;


        public IDbContextTransaction BeginTransaction()
        {
            return base.Database.BeginTransaction();
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return base.Database.BeginTransactionAsync(cancellationToken);
        }

        public void CommitTransaction()
        {
            base.Database.CommitTransaction();
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            return base.Database.CommitTransactionAsync(cancellationToken);
        }


        public void RollbackTransaction()
        {
            base.Database.RollbackTransaction();
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            return base.Database.RollbackTransactionAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("PaymentCodeSequence");
            modelBuilder.HasSequence<long>("BusinessCodeSequence");

            modelBuilder.Entity<CompanionReserve>(entity =>
            {
                entity.Property(item => item.ReserveCode).HasMaxLength(40);
                entity.HasIndex(item => item.ReserveCode).IsUnique().HasFilter("[ReserveCode] IS NOT NULL");
            });
            modelBuilder.Entity<PansionReserve>(entity =>
            {
                entity.Property(item => item.ReserveCode).HasMaxLength(40);
                entity.HasIndex(item => item.ReserveCode).IsUnique().HasFilter("[ReserveCode] IS NOT NULL");
            });
            modelBuilder.Entity<PansionComment>(entity =>
            {
                entity.HasIndex(item => item.PansionReserveId)
                    .IsUnique()
                    .HasFilter("[PansionReserveId] IS NOT NULL");
                entity.HasOne(item => item.PansionReserve)
                    .WithMany()
                    .HasForeignKey(item => item.PansionReserveId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ProductStockAlert>(entity =>
            {
                entity.HasIndex(item => new { item.UserId, item.ProductId })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
                entity.HasIndex(item => item.ProductId);
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Product)
                    .WithMany()
                    .HasForeignKey(item => item.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.NotifiedStore)
                    .WithMany()
                    .HasForeignKey(item => item.NotifiedStoreId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<ProductOrder>(entity =>
            {
                entity.Property(item => item.OrderCode).HasMaxLength(40);
                entity.HasIndex(item => item.OrderCode).IsUnique().HasFilter("[OrderCode] IS NOT NULL");
            });

            modelBuilder.Entity<CompanionAssistance>()
                .Property(item => item.CommissionPercent)
                .HasPrecision(5, 2);
            modelBuilder.Entity<Pansion>(entity =>
            {
                entity.Property(item => item.DailyCommissionPercent).HasPrecision(5, 2);
                entity.Property(item => item.HourlyCommissionPercent).HasPrecision(5, 2);
            });
            modelBuilder.Entity<Store>()
                .Property(item => item.CommissionPercent)
                .HasPrecision(5, 2);
            modelBuilder.Entity<Companion>(entity =>
            {
                entity.Property(item => item.ReferralCode)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.HasIndex(item => item.ReferralCode)
                    .IsUnique();
            });
            modelBuilder.Entity<Store>(entity =>
            {
                entity.Property(item => item.ReferralCode)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.HasIndex(item => item.ReferralCode)
                    .IsUnique();
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(item => item.ReferralCode)
                    .HasMaxLength(10);
                entity.Property(item => item.UsedReferralCode)
                    .HasMaxLength(10);
                entity.HasIndex(item => item.ReferralCode)
                    .IsUnique()
                    .HasFilter("[ReferralCode] IS NOT NULL");
                entity.HasIndex(item => new
                {
                    item.RegistrationReferralSource,
                    item.ReferredByUserId,
                    item.ReferredByCompanionId,
                    item.ReferredByStoreId
                });
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(item => item.ReferredByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Companion>()
                    .WithMany()
                    .HasForeignKey(item => item.ReferredByCompanionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Store>()
                    .WithMany()
                    .HasForeignKey(item => item.ReferredByStoreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasIndex(item => new
                {
                    item.UserPetId,
                    item.ReminderTypeId,
                    item.ReminderCycleId,
                    item.StartDate
                })
                    .IsUnique()
                    .HasFilter("[Deleted] = 0");
            });
            modelBuilder.Entity<PastilMatchMessage>()
                .HasOne(item => item.Park)
                .WithMany()
                .HasForeignKey(item => item.ParkId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReminderCycle>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_ReminderCycle_Cycle",
                    "[Cycle] > 0"));
            modelBuilder.Entity<PushNotification>()
                .HasIndex(item => new
                {
                    item.IsSend,
                    item.Status,
                    item.NextAttemptDate,
                    item.SendDate
                });
            modelBuilder.Entity<ClubPointAccount>(entity =>
            {
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_ClubPointAccount_AvailablePoint", "[AvailablePoint] >= 0");
                    table.HasCheckConstraint("CK_ClubPointAccount_DebtPoint", "[DebtPoint] >= 0");
                });
                entity.Property(item => item.RowVersion).IsRowVersion();
                entity.HasIndex(item => item.UserId).IsUnique();
                entity.HasOne(item => item.User)
                    .WithOne(item => item.ClubPointAccount)
                    .HasForeignKey<ClubPointAccount>(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubPointRule>(entity =>
            {
                entity.Property(item => item.Name).IsRequired().HasMaxLength(200);
                entity.Property(item => item.Description).HasMaxLength(1000);
                entity.HasIndex(item => item.EventType).IsUnique();
                entity.ToTable(table =>
                    table.HasCheckConstraint("CK_ClubPointRule_PointAmount", "[PointAmount] > 0"));
            });

            modelBuilder.Entity<ClubPointTransaction>(entity =>
            {
                entity.Property(item => item.Description).HasMaxLength(1000);
                entity.Property(item => item.IdempotencyKey).IsRequired().HasMaxLength(250);
                entity.HasIndex(item => item.IdempotencyKey).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.CreateDate });
                entity.HasIndex(item => new { item.UserId, item.PointRuleId, item.CreateDate });
                entity.HasIndex(item => new { item.SourceType, item.SourceId });
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.PointAccount)
                    .WithMany(item => item.Transactions)
                    .HasForeignKey(item => item.PointAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.PointRule)
                    .WithMany(item => item.Transactions)
                    .HasForeignKey(item => item.PointRuleId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ParentTransaction)
                    .WithMany()
                    .HasForeignKey(item => item.ParentTransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubRewardTemplate>(entity =>
            {
                entity.Property(item => item.Name).IsRequired().HasMaxLength(200);
                entity.Property(item => item.Title).IsRequired().HasMaxLength(250);
                entity.Property(item => item.ShortDescription).HasMaxLength(500);
                entity.Property(item => item.Description).HasMaxLength(4000);
                entity.Property(item => item.Terms).HasMaxLength(4000);
                entity.Property(item => item.BenefitValue).HasPrecision(18, 2);
                entity.Property(item => item.MaximumBenefitValue).HasPrecision(18, 2);
                entity.HasIndex(item => new { item.Active, item.StartDate, item.EndDate });
                entity.ToTable(table =>
                    table.HasCheckConstraint("CK_ClubRewardTemplate_PointCost", "[PointCost] > 0"));
                entity.HasOne(item => item.Picture)
                    .WithMany()
                    .HasForeignKey(item => item.PictureId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ClubRewardTarget>(entity =>
            {
                entity.HasIndex(item => new { item.RewardTemplateId, item.TargetType, item.TargetId })
                    .IsUnique()
                    .HasFilter(null);
                entity.HasOne(item => item.RewardTemplate)
                    .WithMany(item => item.Targets)
                    .HasForeignKey(item => item.RewardTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClubRewardPetType>(entity =>
            {
                entity.HasIndex(item => new { item.RewardTemplateId, item.PetTypeId }).IsUnique();
                entity.HasOne(item => item.RewardTemplate)
                    .WithMany(item => item.PetTypes)
                    .HasForeignKey(item => item.RewardTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.PetType)
                    .WithMany()
                    .HasForeignKey(item => item.PetTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubRewardOffer>(entity =>
            {
                entity.Property(item => item.RowVersion).IsRowVersion();
                entity.Property(item => item.RejectReason).HasMaxLength(1000);
                entity.HasIndex(item => new { item.UserId, item.RewardTemplateId }).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.Status, item.ExpiresAt });
                entity.HasIndex(item => new { item.RewardTemplateId, item.Status });
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.RewardTemplate)
                    .WithMany(item => item.Offers)
                    .HasForeignKey(item => item.RewardTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubRewardRedemption>(entity =>
            {
                entity.Property(item => item.IdempotencyKey).IsRequired().HasMaxLength(250);
                entity.HasIndex(item => item.IdempotencyKey).IsUnique();
                entity.HasIndex(item => item.RewardOfferId).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.RedeemedDate });
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.RewardOffer)
                    .WithOne(item => item.Redemption)
                    .HasForeignKey<ClubRewardRedemption>(item => item.RewardOfferId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.RewardTemplate)
                    .WithMany()
                    .HasForeignKey(item => item.RewardTemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.PointTransaction)
                    .WithMany()
                    .HasForeignKey(item => item.PointTransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubCoupon>(entity =>
            {
                entity.Property(item => item.Code).IsRequired().HasMaxLength(80);
                entity.Property(item => item.OrderId).HasMaxLength(100);
                entity.HasIndex(item => item.Code).IsUnique();
                entity.HasIndex(item => item.RewardRedemptionId).IsUnique();
                entity.HasIndex(item => item.RebateId).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.Used, item.ExpiresAt });
                entity.HasOne(item => item.RewardRedemption).WithMany()
                    .HasForeignKey(item => item.RewardRedemptionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.User).WithMany()
                    .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Rebate).WithOne(item => item.ClubCoupon)
                    .HasForeignKey<ClubCoupon>(item => item.RebateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Payment).WithMany()
                    .HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubFreeDeliveryBenefit>(entity =>
            {
                entity.Property(item => item.MaximumDeliveryAmount).HasPrecision(18, 2);
                entity.Property(item => item.RowVersion).IsRowVersion();
                entity.HasIndex(item => item.RewardRedemptionId).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.ExpiresAt, item.RemainingUsageCount });
                entity.HasOne(item => item.RewardRedemption).WithMany()
                    .HasForeignKey(item => item.RewardRedemptionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.User).WithMany()
                    .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Store).WithMany()
                    .HasForeignKey(item => item.StoreId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany()
                    .HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_ClubFreeDeliveryBenefit_RemainingUsageCount",
                    "[RemainingUsageCount] >= 0"));
            });

            modelBuilder.Entity<Cart>()
                .HasOne(item => item.ClubFreeDeliveryBenefit)
                .WithMany()
                .HasForeignKey(item => item.ClubFreeDeliveryBenefitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductOrder>()
                .HasOne(item => item.ClubFreeDeliveryBenefit)
                .WithMany()
                .HasForeignKey(item => item.ClubFreeDeliveryBenefitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubPromotionalWalletCredit>(entity =>
            {
                entity.Property(item => item.OriginalAmount).HasPrecision(18, 2);
                entity.Property(item => item.RemainingAmount).HasPrecision(18, 2);
                entity.Property(item => item.RowVersion).IsRowVersion();
                entity.HasIndex(item => item.RewardRedemptionId).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.Status, item.ExpiresAt });
                entity.HasOne(item => item.RewardRedemption).WithMany()
                    .HasForeignKey(item => item.RewardRedemptionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.User).WithMany()
                    .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_ClubPromotionalWalletCredit_OriginalAmount", "[OriginalAmount] > 0");
                    table.HasCheckConstraint("CK_ClubPromotionalWalletCredit_RemainingAmount", "[RemainingAmount] >= 0");
                });
            });

            modelBuilder.Entity<ClubPromotionalCreditUsage>(entity =>
            {
                entity.Property(item => item.Amount).HasPrecision(18, 2);
                entity.Property(item => item.ReferenceKey).IsRequired().HasMaxLength(200);
                entity.HasIndex(item => new { item.PromotionalCreditId, item.ReferenceKey }).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.CreateDate });
                entity.HasOne(item => item.PromotionalCredit).WithMany()
                    .HasForeignKey(item => item.PromotionalCreditId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubRewardPastilAITarget>(entity =>
            {
                entity.HasIndex(item => item.RewardTemplateId).IsUnique();
                entity.HasOne(item => item.RewardTemplate).WithOne(item => item.PastilAITarget)
                    .HasForeignKey<ClubRewardPastilAITarget>(item => item.RewardTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Plan).WithMany()
                    .HasForeignKey(item => item.PlanId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.TargetPlan).WithMany()
                    .HasForeignKey(item => item.TargetPlanId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClubRewardCostTransaction>(entity =>
            {
                entity.Property(item => item.GrossValue).HasPrecision(18, 2);
                entity.Property(item => item.PastilFundedValue).HasPrecision(18, 2);
                entity.Property(item => item.OrderId).HasMaxLength(100);
                entity.HasIndex(item => new { item.RewardRedemptionId, item.CreateDate });
                entity.HasIndex(item => new { item.UserId, item.CreateDate });
                entity.HasOne(item => item.RewardRedemption).WithMany()
                    .HasForeignKey(item => item.RewardRedemptionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.User).WithMany()
                    .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Payment).WithMany()
                    .HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rebate>()
                .HasOne(item => item.ClubCoupon)
                .WithOne(item => item.Rebate)
                .HasForeignKey<ClubCoupon>(item => item.RebateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rebate>(entity =>
            {
                entity.Property(item => item.CodeValue).IsRequired().HasMaxLength(100);
                entity.HasIndex(item => item.CodeValue)
                    .IsUnique()
                    .HasFilter("[Deleted] = 0");
            });

            modelBuilder.Entity<ClubReward>(entity =>
            {
                entity.HasOne(item => item.Rebate)
                    .WithMany()
                    .HasForeignKey(item => item.RebateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserRebate>(entity =>
            {
                entity.HasIndex(item => new { item.UserId, item.RebateId }).IsUnique();
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_UserRebate_UsageCount",
                    "[UsageCount] >= 0"));
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(item => item.PaymentCode).HasMaxLength(40);
                entity.Property(item => item.IdempotencyKey).HasMaxLength(36).IsUnicode(false);
                entity.Property(item => item.CallbackToken).HasMaxLength(64);
                entity.Property(item => item.ApprovedIp).HasMaxLength(64);
                entity.Property(item => item.PaymentUrl).HasMaxLength(2048);
                entity.HasIndex(item => item.PaymentCode)
                    .IsUnique()
                    .HasFilter("[PaymentCode] IS NOT NULL");
                entity.HasIndex(item => item.IdempotencyKey)
                    .IsUnique()
                    .HasFilter("[IdempotencyKey] IS NOT NULL");
                entity.HasIndex(item => item.CallbackToken)
                    .IsUnique()
                    .HasFilter("[CallbackToken] IS NOT NULL");
                entity.HasIndex(item => new { item.UserId, item.CallBackTypeLabel, item.CallBackId });
                entity.HasIndex(item => new { item.RebateId, item.UserId, item.CreateDate });
                entity.HasIndex(item => item.RefNumber)
                    .IsUnique()
                    .HasFilter("[RefNumber] IS NOT NULL AND [IsOnline] = 0 AND [IsSuccess] = 1");
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_Payment_Amount", "[Amount] >= 0");
                    table.HasCheckConstraint("CK_Payment_GrossAmount", "[GrossAmount] >= 0");
                    table.HasCheckConstraint("CK_Payment_RebateAmount", "[RebateAmount] >= 0");
                    table.HasCheckConstraint("CK_Payment_WalletAmount", "[WalletAmount] >= 0");
                });
                entity.HasOne(item => item.Rebate).WithMany()
                    .HasForeignKey(item => item.RebateId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasIndex(item => item.PaymentId)
                    .IsUnique()
                    .HasFilter("[PaymentId] IS NOT NULL");
                entity.HasIndex(item => item.ProductOrderId)
                    .IsUnique()
                    .HasFilter("[ProductOrderId] IS NOT NULL");
                entity.HasIndex(item => item.CompanionReserveId)
                    .IsUnique()
                    .HasFilter("[CompanionReserveId] IS NOT NULL");
                entity.HasIndex(item => item.PansionReserveId)
                    .IsUnique()
                    .HasFilter("[PansionReserveId] IS NOT NULL");
                entity.HasIndex(item => item.TripId)
                    .IsUnique()
                    .HasFilter("[TripId] IS NOT NULL");
                entity.HasIndex(item => item.CargoId)
                    .IsUnique()
                    .HasFilter("[CargoId] IS NOT NULL");
                entity.HasIndex(item => item.CompanionInsurancePackageSaleId)
                    .IsUnique()
                    .HasFilter("[CompanionInsurancePackageSaleId] IS NOT NULL");
                entity.HasIndex(item => item.PastilAiSubscriptionId)
                    .IsUnique()
                    .HasFilter("[PastilAiSubscriptionId] IS NOT NULL");
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_Wallet_Amount",
                    "[Amount] >= 0"));
            });

            modelBuilder.Entity<PastilAiSubscription>()
                .HasOne(item => item.ClubRewardRedemption)
                .WithMany()
                .HasForeignKey(item => item.ClubRewardRedemptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Memory>(entity =>
            {
                entity.HasIndex(item => item.MemoryDate);
                entity.HasIndex(item => new { item.Deleted, item.MemoryDate });
                entity.HasOne(item => item.Picture)
                    .WithMany()
                    .HasForeignKey(item => item.PictureId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserMemory>(entity =>
            {
                entity.HasIndex(item => item.MemoryId).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.Deleted });
                entity.HasIndex(item => new { item.UserPetId, item.Deleted });
                entity.HasOne(item => item.Memory)
                    .WithOne(item => item.UserMemory)
                    .HasForeignKey<UserMemory>(item => item.MemoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.UserPet)
                    .WithMany()
                    .HasForeignKey(item => item.UserPetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SearchQueryLog>(entity =>
            {
                entity.HasIndex(item => item.NormalizedQuery);
                entity.HasIndex(item => item.CreateDateUtc);
                entity.HasIndex(item => new { item.Channel, item.CreateDateUtc });
            });

            modelBuilder.Entity<PastilMatchProfile>(entity =>
            {
                entity.Property(item => item.Username)
                    .HasMaxLength(32);

                // Usernames are unique among active profiles. Soft-deleted profiles
                // must not permanently reserve a handle.
                entity.HasIndex(item => item.Username)
                    .IsUnique()
                    .HasFilter("[Username] IS NOT NULL AND [Deleted] = 0");
            });

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
.SelectMany(t => t.GetForeignKeys())
.Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;


            modelBuilder.Entity<ProductOrder>()
    .Property(et => et.Id)
    .ValueGeneratedNever();
            modelBuilder.Entity<Category>().Navigation(e => e.Picture).AutoInclude();
            modelBuilder.Entity<Category>().Navigation(e => e.Icon).AutoInclude();
            modelBuilder.Entity<Brand>().Navigation(e => e.Picture).AutoInclude();
            modelBuilder.Entity<Brand>().Navigation(e => e.Icon).AutoInclude();
            modelBuilder.Entity<Product>().Navigation(e => e.Picture).AutoInclude();
            modelBuilder.Entity<Cart>().Navigation(e => e.Address).AutoInclude();
            modelBuilder.Entity<Address>().Navigation(e => e.City).AutoInclude();
            modelBuilder.Entity<City>().Navigation(e => e.State).AutoInclude();
            modelBuilder.Entity<Delivery>().Navigation(e => e.DeliveryType).AutoInclude();
            modelBuilder.Entity<ProductPicture>().Navigation(e => e.Picture).AutoInclude();
            modelBuilder.Entity<ProductFile>().Navigation(e => e.File).AutoInclude();
            modelBuilder.Entity<PostFile>().Navigation(e => e.File).AutoInclude();
            modelBuilder.Entity<ProductOrder>().Navigation(e => e.DeliveryType).AutoInclude();
            modelBuilder.Entity<Product>().Navigation(e => e.Status).AutoInclude();
            modelBuilder.Entity<Product>().Navigation(e => e.Type).AutoInclude();
            modelBuilder.Entity<Product>().Navigation(e => e.DiscountGroup).AutoInclude();
            modelBuilder.Entity<Product>().Navigation(e => e.Brand).AutoInclude();
            modelBuilder.Entity<ProductItem>().Navigation(e => e.Product).AutoInclude();
            modelBuilder.Entity<ProductItem>().Navigation(e => e.VarietyItem).AutoInclude();
            modelBuilder.Entity<ProductItem>().Navigation(e => e.VarietyItem2).AutoInclude();
            modelBuilder.Entity<ProductItem>().Navigation(e => e.DiscountGroup).AutoInclude();
            modelBuilder.Entity<ProductOrderItem>().Navigation(e => e.ProductItem).AutoInclude();
            modelBuilder.Entity<CartItem>().Navigation(e => e.ProductItem).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.ProductItem).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.Product).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.Brand).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.Category).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.Store).AutoInclude();
            modelBuilder.Entity<Discount>().Navigation(e => e.Type).AutoInclude();

            modelBuilder.Entity<ProductItem>()
           .Property(e => e.SystemActive)
           .ValueGeneratedOnAdd()
           .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<Permission>()
           .HasQueryFilter(post => EF.Property<bool>(post, "Deleted") == false);
            modelBuilder.Entity<Ticket>()
           .HasQueryFilter(post => EF.Property<bool>(post, "Deleted") == false);
            modelBuilder.Entity<TicketItem>()
           .HasQueryFilter(post => EF.Property<bool>(post, "Deleted") == false);
            modelBuilder.Entity<Post>()
           .HasQueryFilter(post => EF.Property<bool>(post, "Deleted") == false);
            modelBuilder.Entity<Banner>()
           .HasQueryFilter(x => EF.Property<bool>(x, "Deleted") == false);
            modelBuilder.Entity<Category>()
           .HasQueryFilter(x => EF.Property<bool>(x, "Deleted") == false);
            modelBuilder.Entity<User>()
         .HasQueryFilter(category => EF.Property<bool>(category, "Deleted") == false);
            modelBuilder.Entity<Feature>()
         .HasQueryFilter(category => EF.Property<bool>(category, "Deleted") == false);
            modelBuilder.Entity<FeatureItem>()
         .HasQueryFilter(category => EF.Property<bool>(category, "Deleted") == false);

            modelBuilder.Entity<PastilAiPlan>(entity =>
            {
                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => x.Code).IsUnique();
                entity.HasQueryFilter(x => !x.Deleted);
                entity.HasData(
                    new PastilAiPlan
                    {
                        Id = 1, Code = "Free", Name = "PastilAI", Description = "پلن رایگان PastilAI",
                        Price = 0, DurationDays = 30, DailyChatLimit = 3, DailyImageLimit = 1,
                        DailyAudioLimit = 0, DailyVideoLimit = 0, PurchaseEnabled = false, Active = true,
                        Deleted = false, SortOrder = 0,
                        CreateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc),
                        UpdateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc)
                    },
                    new PastilAiPlan
                    {
                        Id = 2, Code = "Plus", Name = "PastilAI+", Description = "پلن پیشرفته PastilAI",
                        Price = 0, DurationDays = 30, DailyChatLimit = 30, DailyImageLimit = 10,
                        DailyAudioLimit = 5, DailyVideoLimit = 1, PurchaseEnabled = false, Active = true,
                        Deleted = false, SortOrder = 10,
                        CreateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc),
                        UpdateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc)
                    },
                    new PastilAiPlan
                    {
                        Id = 3, Code = "Pro", Name = "PastilAI Pro", Description = "پلن نامحدود PastilAI",
                        Price = 0, DurationDays = 30, DailyChatLimit = null, DailyImageLimit = null,
                        DailyAudioLimit = null, DailyVideoLimit = null, PurchaseEnabled = false, Active = true,
                        Deleted = false, SortOrder = 20,
                        CreateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc),
                        UpdateDateUtc = new System.DateTime(2026, 7, 26, 0, 0, 0, System.DateTimeKind.Utc)
                    });
            });

            modelBuilder.Entity<PastilAiSubscription>(entity =>
            {
                entity.Property(x => x.PriceSnapshot).HasColumnType("decimal(18,2)");
                entity.Property(x => x.RebatePrice).HasColumnType("decimal(18,2)");
                entity.Property(x => x.WalletPrice).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => new { x.UserId, x.Status, x.EndDateUtc });
                entity.HasIndex(x => x.PaymentId).IsUnique().HasFilter("[PaymentId] IS NOT NULL");
                entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Plan).WithMany(x => x.Subscriptions).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Payment).WithOne(x => x.PastilAiSubscription)
                    .HasForeignKey<PastilAiSubscription>(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Rebate).WithMany().HasForeignKey(x => x.RebateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Wallet).WithOne(x => x.PastilAiSubscription)
                    .HasForeignKey<Wallet>(x => x.PastilAiSubscriptionId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PastilAiConversation>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.UpdateDateUtc });
                entity.HasQueryFilter(x => !x.Deleted);
                entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PastilAiMessage>(entity =>
            {
                entity.Property(x => x.Content).HasColumnType("nvarchar(max)");
                entity.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
                entity.HasIndex(x => new { x.ConversationId, x.Id });
                entity.HasOne(x => x.Conversation).WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PastilAiAttachment>(entity =>
            {
                entity.HasIndex(x => x.MessageId).IsUnique();
                entity.HasOne(x => x.Message).WithMany(x => x.Attachments)
                    .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Picture).WithMany().HasForeignKey(x => x.PictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.File).WithMany().HasForeignKey(x => x.FileId).OnDelete(DeleteBehavior.Restrict);
                entity.ToTable("PastilAiAttachments", table =>
                    table.HasCheckConstraint("CK_PastilAiAttachments_OneMedia",
                        "([PictureId] IS NOT NULL AND [FileId] IS NULL) OR ([PictureId] IS NULL AND [FileId] IS NOT NULL)"));
            });

            modelBuilder.Entity<PastilAiProviderAttempt>(entity =>
            {
                entity.HasIndex(x => new { x.MessageId, x.AttemptOrder }).IsUnique();
                entity.HasOne(x => x.Message).WithMany(x => x.ProviderAttempts)
                    .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PastilAiDailyUsage>(entity =>
            {
                entity.Property(x => x.UsageDate).HasColumnType("date");
                entity.HasIndex(x => new { x.UserId, x.UsageDate }).IsUnique();
                entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            });



            modelBuilder.Entity<Post>(e =>
            {
                e.HasOne(s => s.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(e => e.ParentId);
            });
            modelBuilder.Entity<Post>()
               .HasMany<Category>(s => s.Categories)
               .WithMany(c => c.Posts);


            modelBuilder.Entity<Post>().HasOne(s => s.Category);

            modelBuilder.Entity<User>()
                .HasMany<Store>(s => s.Stores)
                .WithMany(c => c.Users);

            modelBuilder.Entity<Role>()
                .HasMany<Permission>(s => s.Permissions)
                .WithMany(c => c.Roles);
            modelBuilder.Entity<Product>()
                .HasMany<Category>(s => s.Categories)
                .WithMany(c => c.Products);
            modelBuilder.Entity<Trip>()
                .HasMany<TripOption>(s => s.TripOptions)
                .WithMany(c => c.Trips);

            modelBuilder.Entity<CompanionAssistance>()
                .HasMany<Code>(s => s.Codes)
                .WithMany(c => c.CompanionAssistances);

            modelBuilder.Entity<CompanionReserve>()
                .HasMany<UserPet>(s => s.UserPets)
                .WithMany(c => c.CompanionReserves);

            modelBuilder.Entity<CompanionAssistance>()
                .HasOne(ca => ca.CompanionType)
                .WithMany()
                .HasForeignKey(ca => ca.CompanionTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assistance>()
                .HasOne(x => x.AssistanceGroup)
                .WithMany(x => x.Assistances)
                .HasForeignKey(x => x.AssistanceGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expertise>(entity =>
            {
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasIndex(x => new { x.Name, x.Deleted });
            });

            modelBuilder.Entity<CompanionUser>()
                .HasOne(x => x.Expertise)
                .WithMany(x => x.CompanionUsers)
                .HasForeignKey(x => x.ExpertiseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostComment>().ToTable("PostComments");
            modelBuilder.Entity<ProductComment>().ToTable("ProductComments");
            modelBuilder.Entity<CompanionReserveComment>().ToTable("CompanionReserveComments");

            modelBuilder.Entity<Product>()
                .HasOne<Category>(s => s.Category);

            modelBuilder.Entity<ProductFile>(e =>
            {
                e.HasOne(s => s.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(e => e.ParentId);
            });
            modelBuilder.Entity<Permission>(e =>
            {
                e.HasOne(s => s.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(e => e.ParentId);

            });
            modelBuilder.Entity<Category>(e =>
            {
                e.HasOne(s => s.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(e => e.ParentId);

                e.HasOne(p => p.Picture)
                .WithMany()
                .HasForeignKey(p => p.PictureId);

                e.HasOne(p => p.Icon)
                .WithMany()
                .HasForeignKey(p => p.IconId);
            });
            modelBuilder.Entity<Banner>(e =>
            {


                e.HasOne(p => p.Picture)
                .WithMany()
                .HasForeignKey(p => p.PictureId);

                e.HasOne(p => p.Picture2)
                .WithMany()
                .HasForeignKey(p => p.Picture2Id);
            });


            modelBuilder.Entity<Brand>(e =>
            {
                e.HasOne(p => p.Picture)
                .WithMany()
                .HasForeignKey(p => p.PictureId);
                ;

                e.HasOne(p => p.Icon)
                .WithMany()
                .HasForeignKey(p => p.IconId);
            });


            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.TripStatus)
                .WithMany()
                .HasForeignKey(t => t.TripStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.DriverStatus)
                .WithMany()
                .HasForeignKey(t => t.DriverStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.CompanionReserve)
                .WithMany()
                .HasForeignKey(t => t.CompanionReserveId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TripPet>()
                .HasOne(tp => tp.Trip)
                .WithMany(t => t.TripPets)
                .HasForeignKey(tp => tp.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TripPet>()
                .HasOne(tp => tp.UserPet)
                .WithMany()
                .HasForeignKey(tp => tp.UserPetId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<User>(e =>
            {
                e.HasOne(p => p.Driver)
                .WithOne(s => s.Owner)
                .HasForeignKey<Driver>(p => p.OwnerId);
                ;
            });
            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.ShippingLengthCm).HasPrecision(10, 2);
                e.Property(p => p.ShippingWidthCm).HasPrecision(10, 2);
                e.Property(p => p.ShippingHeightCm).HasPrecision(10, 2);
                e.HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.StatusId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.TypeId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Variety2)
                .WithMany()
                .HasForeignKey(p => p.Variety2Id).OnDelete(DeleteBehavior.Restrict);


            });
            modelBuilder.Entity<ProductItem>(e =>
            {
                e.HasOne(p => p.VarietyItem)
                .WithMany()
                .HasForeignKey(p => p.VarietyItemId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.VarietyItem2)
                .WithMany()
                .HasForeignKey(p => p.VarietyItem2Id).OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<ProductOrder>(e =>
            {
                e.HasOne(p => p.ProductOrderState)
                .WithMany()
                .HasForeignKey(p => p.ProductOrderStateId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.ProductOrderStatus)
                .WithMany()
                .HasForeignKey(p => p.ProductOrderStatusId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.PaymentType)
                .WithMany()
                .HasForeignKey(p => p.PaymentTypeId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.DeliveryType)
                .WithMany()
                .HasForeignKey(p => p.DeliveryTypeId).OnDelete(DeleteBehavior.NoAction);

            });
            modelBuilder.Entity<ShippingQuote>(entity =>
            {
                entity.Property(item => item.Currency).HasMaxLength(10).IsRequired();
                entity.Property(item => item.ExternalQuoteId).HasMaxLength(250);
                entity.Property(item => item.RequestFingerprint).HasMaxLength(128).IsRequired();
                entity.HasIndex(item => item.Token).IsUnique();
                entity.HasIndex(item => new { item.UserId, item.CartStoreId, item.Status });
                entity.HasIndex(item => item.ExpiresAtUtc);
                entity.HasOne(item => item.CartStore)
                    .WithMany()
                    .HasForeignKey(item => item.CartStoreId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(item => item.User)
                    .WithMany()
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(item => item.Address)
                    .WithMany()
                    .HasForeignKey(item => item.AddressId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(item => item.Delivery)
                    .WithMany()
                    .HasForeignKey(item => item.DeliveryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<CartStore>(entity =>
            {
                entity.HasOne(item => item.ShippingQuote)
                    .WithMany()
                    .HasForeignKey(item => item.ShippingQuoteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<ProductOrderStore>(entity =>
            {
                entity.HasOne(item => item.ShippingQuote)
                    .WithMany()
                    .HasForeignKey(item => item.ShippingQuoteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.Property(item => item.ExternalShipmentId).HasMaxLength(250);
                entity.Property(item => item.TrackingCode).HasMaxLength(250);
                entity.Property(item => item.FailureReason).HasMaxLength(1000);
                entity.HasIndex(item => item.ProductOrderStoreId).IsUnique();
                entity.HasIndex(item => new { item.Provider, item.Status });
                entity.HasOne(item => item.ProductOrderStore)
                    .WithOne(item => item.Shipment)
                    .HasForeignKey<Shipment>(item => item.ProductOrderStoreId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(item => item.ShippingQuote)
                    .WithMany()
                    .HasForeignKey(item => item.ShippingQuoteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<ProductFeatureValue>(e =>
            {
                e.HasOne(p => p.Product)
                .WithMany(p => p.ProductFeatureValues)
                .HasForeignKey(p => p.ProductId);
            });

            modelBuilder.Entity<NoticeType>(entity =>
            {
                entity.Property(x => x.Label)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.NavigationTemplate)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Label)
                    .IsUnique();
            });

            modelBuilder.Entity<Notice>(entity =>
            {
                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(x => x.ReferenceType)
                    .HasMaxLength(100);

                entity.Property(x => x.NavigationUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.MetadataJson)
                    .HasColumnType("nvarchar(max)");

                entity.Property(x => x.DeduplicationKey)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasIndex(x => x.DeduplicationKey)
                    .IsUnique();

                entity.HasIndex(x => new
                {
                    x.ArchivedAtUtc,
                    x.ArchiveDueAtUtc,
                    x.CreateDateUtc
                });

                entity.HasIndex(x => new
                {
                    x.NoticeTypeId,
                    x.CreateDateUtc
                });

                entity.HasIndex(x => new
                {
                    x.ReferenceType,
                    x.ReferenceId
                });

                entity.HasOne(x => x.NoticeType)
                    .WithMany(x => x.Notices)
                    .HasForeignKey(x => x.NoticeTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ActorUser)
                    .WithMany()
                    .HasForeignKey(x => x.ActorUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable("Notices", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Notices_MetadataJson_IsJson",
                        "[MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1");
                });
            });

            modelBuilder.Entity<NoticeRead>(entity =>
            {
                entity.Property(x => x.AdminNameSnapshot)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(x => x.NoticeId)
                    .IsUnique();

                entity.HasIndex(x => new
                {
                    x.AdminId,
                    x.ReadAtUtc
                });

                entity.HasOne(x => x.Notice)
                    .WithOne(x => x.Read)
                    .HasForeignKey<NoticeRead>(x => x.NoticeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Admin)
                    .WithMany()
                    .HasForeignKey(x => x.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PushNotification>()
                .HasOne(x => x.Notice)
                .WithMany(x => x.PushNotifications)
                .HasForeignKey(x => x.NoticeId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(x => typeof(ISlugEntity).IsAssignableFrom(x.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>(nameof(ISlugEntity.Slug))
                    .HasMaxLength(200);

                // Category is hierarchical: sibling categories under different
                // parents are allowed to share the same Label/Slug (e.g. a
                // "دامپزشکی" sub-category under both "سگ" and "گربه"). It gets
                // its own parent-scoped unique index below instead of the
                // table-wide one every other ISlugEntity uses.
                if (entityType.ClrType == typeof(Category))
                    continue;

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISlugEntity.Slug))
                    .IsUnique()
                    .HasFilter("[Slug] IS NOT NULL");
            }

            // SQL Server unique indexes never treat two NULLs as equal, so a
            // plain (ParentId, Slug) unique index would silently allow
            // duplicate Slugs among root categories (ParentId IS NULL).
            // A persisted computed column normalizes NULL to 0 so root
            // categories are scoped just like any other sibling group.
            modelBuilder.Entity<Category>()
                .Property<long>("SlugScopeParentId")
                .HasComputedColumnSql("ISNULL([ParentId], 0)", stored: true);

            modelBuilder.Entity<Category>()
                .HasIndex("SlugScopeParentId", nameof(ISlugEntity.Slug))
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL")
                .HasDatabaseName("IX_Categories_SlugScopeParentId_Slug");

            modelBuilder.Entity<PetTag>()
                .Property(x => x.Code)
                .HasMaxLength(64);

            modelBuilder.Entity<PetTag>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<PushSubscription>(e =>
            {
                e.ToTable("PushSubscriptions");

                e.Property(x => x.Endpoint).IsRequired();
                e.Property(x => x.P256dh).IsRequired();
                e.Property(x => x.Auth).IsRequired();

                e.HasIndex(x => x.Endpoint).IsUnique();

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserPet>()
                .HasOne(x => x.PetBreed)
                .WithMany()
                .HasForeignKey(x => x.PetBreedId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserPet>()
                .HasOne(x => x.PetBreed2)
                .WithMany()
                .HasForeignKey(x => x.PetBreed2Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.TicketCategoryId)
                    .HasDefaultValue(10139L);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Admin)
                    .WithMany()
                    .HasForeignKey(x => x.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Status)
                    .WithMany()
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Importance)
                    .WithMany()
                    .HasForeignKey(x => x.ImportanceId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.TicketCategory)
                    .WithMany()
                    .HasForeignKey(x => x.TicketCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.UpdateDate
                });

                entity.HasIndex(x => new
                {
                    x.AdminId,
                    x.StatusId,
                    x.UpdateDate
                });

                entity.HasIndex(x => new
                {
                    x.StatusId,
                    x.TicketCategoryId,
                    x.UpdateDate
                });
            });

            modelBuilder.Entity<TicketItem>(entity =>
            {
                entity.Property(x => x.Body)
                    .IsRequired(false);

                entity.Property(x => x.IsSeen)
                    .HasDefaultValue(false);

                entity.HasOne(x => x.Ticket)
                    .WithMany(x => x.TicketItems)
                    .HasForeignKey(x => x.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.File)
                    .WithMany()
                    .HasForeignKey(x => x.FileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReplyToTicketItem)
                    .WithMany(x => x.Replies)
                    .HasForeignKey(x => x.ReplyToTicketItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => new
                {
                    x.TicketId,
                    x.Id
                });

                entity.HasIndex(x => new
                {
                    x.TicketId,
                    x.IsSeen,
                    x.UserId
                });
            });
        }


    }
}
