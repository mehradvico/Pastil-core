using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Interface;
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

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Code> Codes { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<CodeGroup> CodeGroups { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureItem> FeatureItems { get; set; }
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFeatureValue> ProductFeatureValues { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<ProductPicture> ProductPictures { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostFile> PostFiles { get; set; }
        public DbSet<PostPicture> PostPictures { get; set; }
        public DbSet<SeoFieldLang> SeoFieldLangs { get; set; }
        public DbSet<NameFieldLang> NameFieldLangs { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<Variety> Varieties { get; set; }
        public DbSet<VarietyItem> VarietyItems { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<FullNameFieldLang> FullNameFieldLangs { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<OtpVerify> OtpVerifies { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ContactUs> ContactUses { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }
        public DbSet<Sms> Smses { get; set; }
        public DbSet<SmsNumber> SmsNumbers { get; set; }
        public DbSet<SmsProvider> SmsProviders { get; set; }
        public DbSet<SmsSetting> SmsSettings { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Rebate> Rebate { get; set; }
        public DbSet<ProductOrderItem> ProductOrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ProductLike> ProductLikes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketItem> TicketItems { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<ContactUsGroup> ContactUsGroups { get; set; }
        public DbSet<UserCategory> UserCategories { get; set; }
        public DbSet<ProductRelate> ProductRelates { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryDistance> DeliveryDistances { get; set; }
        public DbSet<BaseDetail> BaseDetails { get; set; }
        public DbSet<AdminSetting> AdminSettings { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreComment> StoreComments { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<EmailHost> EmailHosts { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountGroup> DiscountGroups { get; set; }
        public DbSet<CartStore> CartStores { get; set; }
        public DbSet<ProductOrderStore> ProductOrderStores { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<UserPet> UserPets { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<PriceCalculation> PriceCalculations { get; set; }
        public DbSet<MapKey> MapKeys { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<Companion> Companions { get; set; }
        public DbSet<CompanionPet> CompanionPets { get; set; }
        public DbSet<CompanionType> CompanionTypes { get; set; }
        public DbSet<CompanionAssistance> CompanionAssistances { get; set; }
        public DbSet<CompanionAssistancePackage> CompanionAssistancePackages { get; set; }
        public DbSet<CompanionAssistanceUser> CompanionAssistanceUsers { get; set; }
        public DbSet<CompanionAssistanceTime> CompanionAssistanceTimes { get; set; }
        public DbSet<CompanionReserve> CompanionReserves { get; set; }
        public DbSet<WeekDay> WeekDays { get; set; }
        public DbSet<AssistanceQuestionnaire> AssistanceQuestionnaires { get; set; }
        public DbSet<CompanionReserveComment> CompanionReserveComments { get; set; }
        public DbSet<CompanionReserveCommentRate> CompanionReserveCommentRates { get; set; }
        public DbSet<CompanionUser> CompanionUsers { get; set; }
        public DbSet<DriverUser> DriverUsers { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }
        public DbSet<DiscussionQuestion> DiscussionQuestions { get; set; }
        public DbSet<DiscussionAnswer> DiscussionAnswers { get; set; }
        public DbSet<DiscussionAnswerLike> DiscussionAnswerLikes { get; set; }
        public DbSet<CompanionReport> CompanionReports { get; set; }
        public DbSet<CompanionAssistanceReport> CompanionAssistanceReports { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<ContactUsItem> ContactUsItems { get; set; }
        public DbSet<TripStop> TripStops { get; set; }
        public DbSet<TripOption> TripOptions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Cargo> Cargoes { get; set; }
        public DbSet<CompanionInsurancePackage> CompanionInsurancePackages { get; set; }
        public DbSet<CompanionInsurancePackageSale> CompanionInsurancePackageSales { get; set; }
        public DbSet<CompanionComment> CompanionComments { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<ReminderCycle> ReminderCycles { get; set; }
        public DbSet<ReminderType> ReminderTypes { get; set; }
        public DbSet<TripAddress> TripAddresses { get; set; }
        public DbSet<UserPetRecord> UserPetRecords { get; set; }
        public DbSet<NotifyMessage> NotifyMessages { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<CompanionAssistancePackagePicture> CompanionAssistancePackagePictures { get; set; }
        public DbSet<Pansion> Pansions { get; set; }
        public DbSet<PansionReserve> PansionReserves { get; set; }
        public DbSet<PansionPet> PansionPets { get; set; }
        public DbSet<PansionComment> PansionComments { get; set; }
        public DbSet<PansionPicture> PansionPictures { get; set; }
        public DbSet<CompanionZone> CompanionZones { get; set; }
        public DbSet<UserPetPicture> UserPetPictures { get; set; }
        public DbSet<StoryGroup> StoryGroups { get; set; }
        public DbSet<StoryItem> StoryItems { get; set; }
        public DbSet<StoryUserLike> StoryUserLikes { get; set; }

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

            //modelBuilder.Entity<Variety>().Navigation(e => e.VarietyItems).AutoInclude();
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
           .ValueGeneratedOnAdd() // تنظیم این ویژگی به این معنی است که فیلد فقط در هنگام افزودن داده به جدول مقداردهی می‌شود
           .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore); // فیلد بعد از ذخیره تغییرات به‌روزرسانی نخواهد شد

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

            modelBuilder.Entity<Variety>()
                .HasMany<NameFieldLang>(s => s.NameFieldLangs)
                .WithMany(c => c.Varieties)
                .UsingEntity(join => join.ToTable("VarietyNameFieldLangs"));

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


            modelBuilder.Entity<Ticket>(e =>
            {
                e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);
                ;

                e.HasOne(p => p.Admin)
                .WithMany()
                .HasForeignKey(p => p.AdminId);
            });
            modelBuilder.Entity<User>(e =>
            {
                e.HasOne(p => p.Driver)
                .WithOne(s => s.Owner)
                .HasForeignKey<Driver>(p => p.OwnerId);
                ;
            });
            modelBuilder.Entity<Product>(e =>
            {
                e.HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.StatusId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.TypeId).OnDelete(DeleteBehavior.NoAction);

                //e.HasOne(p => p.Variety)
                //.WithMany()
                //.HasForeignKey(p => p.VarietyId).OnDelete(DeleteBehavior.Restrict); 
                e.HasOne(p => p.Variety2)
                .WithMany()
                .HasForeignKey(p => p.Variety2Id).OnDelete(DeleteBehavior.Restrict);


            });
            modelBuilder.Entity<Ticket>(e =>
            {
                e.HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.StatusId).OnDelete(DeleteBehavior.NoAction);

                e.HasOne(p => p.Importance)
                .WithMany()
                .HasForeignKey(p => p.ImportanceId).OnDelete(DeleteBehavior.NoAction);
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
            modelBuilder.Entity<ProductFeatureValue>(e =>
            {
                e.HasOne(p => p.Product)
                .WithMany(p => p.ProductFeatureValues)
                .HasForeignKey(p => p.ProductId);
            });
            //modelBuilder.Entity<CartItem>(e =>
            //{
            //    e.HasOne(p => p.Cart)
            //    .WithMany()
            //    .HasForeignKey(p => p.CartId).OnDelete(DeleteBehavior.);
            //});
            modelBuilder.Entity<Notice>()
                .HasOne(n => n.Type)
                .WithMany()
                .HasForeignKey(n => n.TypeId);

            modelBuilder.Entity<Notice>()
                .HasOne(n => n.UserType)
                .WithMany()
                .HasForeignKey(n => n.UserTypeId);

            modelBuilder.Entity<GalleryItem>()
             .HasMany<FullNameFieldLang>(s => s.FullNameFieldLangs)
              .WithMany(c => c.GalleryItems)
                 .UsingEntity(join => join.ToTable("GalleryItemFullNameFieldLangs"));
            base.OnModelCreating(modelBuilder);

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

        }


    }
}
