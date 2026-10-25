using Entities.Entities;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    /// <summary>
    /// تنظیم جدول‌های آرایشگاه (Groomer*). جدا از DataBaseContext نگه داشته شده تا فایل اصلی بزرگ‌تر نشود.
    /// قاعده: فرزند ← والد داخل درخت آرایشگاه Cascade، هر ارجاع به جدول‌های مشترک (کاربر، حیوان، عکس، شهر ...) Restrict
    /// تا SQL Server روی «multiple cascade paths» خطا ندهد و حذف یک کاربر/عکس ناخواسته آرایشگاه را پاک نکند.
    /// </summary>
    internal static class GroomerModelConfiguration
    {
        public static void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Groomer>(entity =>
            {
                entity.Property(item => item.Name).HasMaxLength(200);
                entity.Property(item => item.Phone).HasMaxLength(40);
                entity.Property(item => item.ReferralCode).HasMaxLength(10);
                entity.Property(item => item.LicenseNumber).HasMaxLength(100);
                entity.Property(item => item.InstagramUrl).HasMaxLength(300);
                entity.Property(item => item.WebsiteUrl).HasMaxLength(300);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.ReferralCode).IsUnique().HasFilter("[ReferralCode] IS NOT NULL");
                // یک Companion قدیمی فقط یک‌بار منتقل می‌شود (اسکریپت انتقال روی همین ایندکس idempotent است).
                entity.HasIndex(item => item.LegacyCompanionId).IsUnique().HasFilter("[LegacyCompanionId] IS NOT NULL");
                entity.HasIndex(item => new { item.ProviderId, item.Deleted });
                entity.HasIndex(item => new { item.CityId, item.Active, item.Approved, item.Deleted });

                entity.HasOne(item => item.Provider).WithMany(provider => provider.Groomers).HasForeignKey(item => item.ProviderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Owner).WithMany().HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.BackgroundPicture).WithMany().HasForeignKey(item => item.BackgroundPictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Icon).WithMany().HasForeignKey(item => item.IconId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerUser>(entity =>
            {
                entity.HasIndex(item => new { item.GroomerId, item.UserId });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerUsers).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerService>(entity =>
            {
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => new { item.GroomerId, item.AssistanceId });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerServices).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Assistance).WithMany().HasForeignKey(item => item.AssistanceId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerServicePackage>(entity =>
            {
                entity.Property(item => item.PetSize).HasMaxLength(20);
                entity.HasOne(item => item.GroomerService).WithMany(service => service.Packages).HasForeignKey(item => item.GroomerServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerServicePackagePicture>(entity =>
            {
                entity.HasOne(item => item.GroomerServicePackage).WithMany(package => package.Pictures).HasForeignKey(item => item.GroomerServicePackageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerServiceUser>(entity =>
            {
                entity.HasIndex(item => new { item.GroomerServiceId, item.UserId });
                entity.HasOne(item => item.GroomerService).WithMany(service => service.ServiceUsers).HasForeignKey(item => item.GroomerServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerTime>(entity =>
            {
                entity.Property(item => item.StartTime).HasMaxLength(10);
                entity.Property(item => item.EndTime).HasMaxLength(10);
                entity.HasIndex(item => new { item.GroomerId, item.WeekDayId });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerTimes).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.WeekDay).WithMany().HasForeignKey(item => item.WeekDayId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerZone>(entity =>
            {
                entity.HasIndex(item => new { item.GroomerId, item.CityId });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerZones).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPet>(entity =>
            {
                entity.HasIndex(item => new { item.GroomerId, item.PetId });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerPets).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Pet).WithMany().HasForeignKey(item => item.PetId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPicture>(entity =>
            {
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerPictures).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerComment>(entity =>
            {
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.GroomerComments).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => item.GroomerReserveId).IsUnique().HasFilter("[GroomerReserveId] IS NOT NULL");
                entity.HasOne(item => item.GroomerReserve).WithMany().HasForeignKey(item => item.GroomerReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerReserveItem>(entity =>
            {
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.GroomerReserveId);
                entity.HasOne(item => item.GroomerReserve).WithMany(reserve => reserve.Items).HasForeignKey(item => item.GroomerReserveId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.GroomerService).WithMany().HasForeignKey(item => item.GroomerServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerServicePackage).WithMany().HasForeignKey(item => item.GroomerServicePackageId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SettlementGroomer>(entity =>
            {
                // هر رزرو فقط در یک تسویه پرداخت می‌شود.
                entity.HasIndex(item => item.GroomerReserveId).IsUnique();
                entity.HasIndex(item => item.SettlementId);
                entity.HasOne(item => item.Settlement).WithMany().HasForeignKey(item => item.SettlementId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerReserve).WithMany().HasForeignKey(item => item.GroomerReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerReserve>(entity =>
            {
                entity.Property(item => item.CompleteDetail).HasMaxLength(500);
                entity.Property(item => item.ReserveCode).HasMaxLength(40);
                entity.Property(item => item.BookerNote).HasMaxLength(500);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.ReserveCode).IsUnique().HasFilter("[ReserveCode] IS NOT NULL");
                // ظرفیت و هم‌پوشانی نوبت‌ها: بر اساس آرایشگاه و بازه‌ی زمانی
                entity.HasIndex(item => new { item.GroomerId, item.DoDate, item.EndDate });
                entity.HasIndex(item => new { item.BookerId, item.CreateDate });
                entity.HasIndex(item => new { item.OwnerDecision, item.OwnerApprovalDeadline });
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_GroomerReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.HasCheckConstraint("CK_GroomerReserve_Dates", "[EndDate] >= [DoDate]");
                });

                entity.HasOne(item => item.Groomer).WithMany().HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerService).WithMany().HasForeignKey(item => item.GroomerServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerServicePackage).WithMany().HasForeignKey(item => item.GroomerServicePackageId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Booker).WithMany().HasForeignKey(item => item.BookerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.UserPet).WithMany().HasForeignKey(item => item.UserPetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Assignee).WithMany().HasForeignKey(item => item.AssigneeUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerTime).WithMany().HasForeignKey(item => item.GroomerTimeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Address).WithMany().HasForeignKey(item => item.AddressId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Rebate).WithMany().HasForeignKey(item => item.RebateId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPortfolioPost>(entity =>
            {
                entity.Property(item => item.Caption).HasMaxLength(2000);
                entity.HasIndex(item => new { item.GroomerId, item.Deleted, item.Approved, item.Active, item.Id });
                entity.HasOne(item => item.Groomer).WithMany(groomer => groomer.PortfolioPosts).HasForeignKey(item => item.GroomerId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Pet).WithMany().HasForeignKey(item => item.PetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.GroomerService).WithMany().HasForeignKey(item => item.GroomerServiceId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPortfolioPicture>(entity =>
            {
                entity.HasIndex(item => new { item.PostId, item.SortOrder });
                entity.HasOne(item => item.Post).WithMany(post => post.Pictures).HasForeignKey(item => item.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPortfolioLike>(entity =>
            {
                // هر کاربر یک لایک برای هر پست؛ لایک هم‌زمان دوباره با خطای یکتایی رد می‌شود نه شمارش دوباره.
                entity.HasIndex(item => new { item.PostId, item.UserId }).IsUnique();
                entity.HasOne(item => item.Post).WithMany(post => post.Likes).HasForeignKey(item => item.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<GroomerPortfolioComment>(entity =>
            {
                entity.HasIndex(item => new { item.PostId, item.Deleted });
                entity.HasOne(item => item.Post).WithMany(post => post.Comments).HasForeignKey(item => item.PostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Parent).WithMany().HasForeignKey(item => item.ParentId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
