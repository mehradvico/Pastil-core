using Entities.Entities;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    /// <summary>
    /// تنظیم جدول‌های مربی (Coach*). جدا از DataBaseContext نگه داشته شده تا فایل اصلی بزرگ‌تر نشود.
    /// قاعده: فرزند ← والد داخل درخت مربی Cascade، هر ارجاع به جدول‌های مشترک (کاربر، حیوان، عکس، شهر ...) Restrict
    /// تا SQL Server روی «multiple cascade paths» خطا ندهد و حذف یک کاربر/عکس ناخواسته مربی را پاک نکند.
    /// </summary>
    internal static class CoachModelConfiguration
    {
        public static void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coach>(entity =>
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

                entity.HasOne(item => item.Provider).WithMany(provider => provider.Coaches).HasForeignKey(item => item.ProviderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Owner).WithMany().HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.BackgroundPicture).WithMany().HasForeignKey(item => item.BackgroundPictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Icon).WithMany().HasForeignKey(item => item.IconId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachUser>(entity =>
            {
                entity.HasIndex(item => new { item.CoachId, item.UserId });
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachUsers).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachService>(entity =>
            {
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => new { item.CoachId, item.AssistanceId });
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachServices).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Assistance).WithMany().HasForeignKey(item => item.AssistanceId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachServicePackage>(entity =>
            {
                entity.Property(item => item.PetSize).HasMaxLength(20);
                entity.HasOne(item => item.CoachService).WithMany(service => service.Packages).HasForeignKey(item => item.CoachServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachServicePackageType>(entity =>
            {
                entity.HasIndex(item => new { item.CoachServicePackageId, item.Mode });
                entity.HasOne(item => item.CoachServicePackage).WithMany(package => package.Types).HasForeignKey(item => item.CoachServicePackageId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CoachServicePackagePicture>(entity =>
            {
                entity.HasOne(item => item.CoachServicePackage).WithMany(package => package.Pictures).HasForeignKey(item => item.CoachServicePackageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachServiceUser>(entity =>
            {
                entity.HasIndex(item => new { item.CoachServiceId, item.UserId });
                entity.HasOne(item => item.CoachService).WithMany(service => service.ServiceUsers).HasForeignKey(item => item.CoachServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachTime>(entity =>
            {
                entity.Property(item => item.StartTime).HasMaxLength(10);
                entity.Property(item => item.EndTime).HasMaxLength(10);
                entity.HasIndex(item => new { item.CoachId, item.WeekDayId });
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachTimes).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.WeekDay).WithMany().HasForeignKey(item => item.WeekDayId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachZone>(entity =>
            {
                entity.HasIndex(item => new { item.CoachId, item.CityId });
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachZones).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachPet>(entity =>
            {
                entity.HasIndex(item => new { item.CoachId, item.PetId });
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachPets).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Pet).WithMany().HasForeignKey(item => item.PetId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachPicture>(entity =>
            {
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachPictures).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachComment>(entity =>
            {
                // TPT: کلید Id همان کلید Comments است (کلید خارجی صریح؛ بدون آن EF ستون Id را Identity می‌سازد و با Comments هم‌شناسه نمی‌شود).
                entity.HasOne<Comment>().WithOne().HasForeignKey<CoachComment>(item => item.Id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Coach).WithMany(coach => coach.CoachComments).HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => item.CoachReserveId).IsUnique().HasFilter("[CoachReserveId] IS NOT NULL");
                entity.HasOne(item => item.CoachReserve).WithMany().HasForeignKey(item => item.CoachReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachReserveItem>(entity =>
            {
                entity.Property(item => item.PackageName).HasMaxLength(300);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.CoachReserveId);
                entity.HasOne(item => item.CoachReserve).WithMany(reserve => reserve.Items).HasForeignKey(item => item.CoachReserveId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.CoachService).WithMany().HasForeignKey(item => item.CoachServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.CoachServicePackage).WithMany().HasForeignKey(item => item.CoachServicePackageId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SettlementCoach>(entity =>
            {
                // هر رزرو فقط در یک تسویه پرداخت می‌شود.
                entity.HasIndex(item => item.CoachReserveId).IsUnique();
                entity.HasIndex(item => item.SettlementId);
                entity.HasOne(item => item.Settlement).WithMany().HasForeignKey(item => item.SettlementId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.CoachReserve).WithMany().HasForeignKey(item => item.CoachReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CoachReserve>(entity =>
            {
                entity.Property(item => item.ReserveCode).HasMaxLength(40);
                entity.Property(item => item.BookerNote).HasMaxLength(500);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.ReserveCode).IsUnique().HasFilter("[ReserveCode] IS NOT NULL");
                // ظرفیت و هم‌پوشانی نوبت‌ها: بر اساس مربی و بازه‌ی زمانی
                entity.HasIndex(item => new { item.CoachId, item.DoDate, item.EndDate });
                entity.HasIndex(item => new { item.BookerId, item.CreateDate });
                entity.HasIndex(item => new { item.OwnerDecision, item.OwnerApprovalDeadline });
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_CoachReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.HasCheckConstraint("CK_CoachReserve_Dates", "[EndDate] >= [DoDate]");
                });

                entity.HasOne(item => item.Coach).WithMany().HasForeignKey(item => item.CoachId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.CoachService).WithMany().HasForeignKey(item => item.CoachServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.CoachServicePackage).WithMany().HasForeignKey(item => item.CoachServicePackageId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Booker).WithMany().HasForeignKey(item => item.BookerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.UserPet).WithMany().HasForeignKey(item => item.UserPetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Assignee).WithMany().HasForeignKey(item => item.AssigneeUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.CoachTime).WithMany().HasForeignKey(item => item.CoachTimeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Address).WithMany().HasForeignKey(item => item.AddressId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Rebate).WithMany().HasForeignKey(item => item.RebateId).OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
