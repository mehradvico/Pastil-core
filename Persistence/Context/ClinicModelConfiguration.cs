using Entities.Entities;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    /// <summary>
    /// تنظیم جدول‌های کلینیک (Clinic*). جدا از DataBaseContext نگه داشته شده تا فایل اصلی بزرگ‌تر نشود.
    /// قاعده: فرزند ← والد داخل درخت کلینیک Cascade، هر ارجاع به جدول‌های مشترک (کاربر، حیوان، عکس، شهر ...) Restrict
    /// تا SQL Server روی «multiple cascade paths» خطا ندهد و حذف یک کاربر/عکس ناخواسته کلینیک را پاک نکند.
    /// </summary>
    internal static class ClinicModelConfiguration
    {
        public static void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clinic>(entity =>
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

                entity.HasOne(item => item.Provider).WithMany(provider => provider.Clinics).HasForeignKey(item => item.ProviderId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Owner).WithMany().HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.BackgroundPicture).WithMany().HasForeignKey(item => item.BackgroundPictureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Icon).WithMany().HasForeignKey(item => item.IconId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicUser>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicId, item.UserId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicUsers).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicService>(entity =>
            {
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => new { item.ClinicId, item.AssistanceId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicServices).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Assistance).WithMany().HasForeignKey(item => item.AssistanceId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicServicePackage>(entity =>
            {
                entity.Property(item => item.PetSize).HasMaxLength(20);
                entity.HasOne(item => item.ClinicService).WithMany(service => service.Packages).HasForeignKey(item => item.ClinicServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicServicePackageType>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicServicePackageId, item.Mode });
                entity.HasOne(item => item.ClinicServicePackage).WithMany(package => package.Types).HasForeignKey(item => item.ClinicServicePackageId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClinicType>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicId, item.TypeId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicTypes).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Type).WithMany().HasForeignKey(item => item.TypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicServicePackagePicture>(entity =>
            {
                entity.HasOne(item => item.ClinicServicePackage).WithMany(package => package.Pictures).HasForeignKey(item => item.ClinicServicePackageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicServiceUser>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicServiceId, item.UserId });
                entity.HasOne(item => item.ClinicService).WithMany(service => service.ServiceUsers).HasForeignKey(item => item.ClinicServiceId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicTime>(entity =>
            {
                entity.Property(item => item.StartTime).HasMaxLength(10);
                entity.Property(item => item.EndTime).HasMaxLength(10);
                entity.HasIndex(item => new { item.ClinicId, item.WeekDayId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicTimes).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.WeekDay).WithMany().HasForeignKey(item => item.WeekDayId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicZone>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicId, item.CityId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicZones).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.State).WithMany().HasForeignKey(item => item.StateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.City).WithMany().HasForeignKey(item => item.CityId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Neighborhood).WithMany().HasForeignKey(item => item.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicPet>(entity =>
            {
                entity.HasIndex(item => new { item.ClinicId, item.PetId });
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicPets).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Pet).WithMany().HasForeignKey(item => item.PetId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicPicture>(entity =>
            {
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicPictures).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Picture).WithMany().HasForeignKey(item => item.PictureId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicComment>(entity =>
            {
                // TPT: کلید Id همان کلید Comments است (کلید خارجی صریح؛ بدون آن EF ستون Id را Identity می‌سازد و با Comments هم‌شناسه نمی‌شود).
                entity.HasOne<Comment>().WithOne().HasForeignKey<ClinicComment>(item => item.Id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Clinic).WithMany(clinic => clinic.ClinicComments).HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(item => item.ClinicReserveId).IsUnique().HasFilter("[ClinicReserveId] IS NOT NULL");
                entity.HasOne(item => item.ClinicReserve).WithMany().HasForeignKey(item => item.ClinicReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicReserveItem>(entity =>
            {
                entity.Property(item => item.PackageName).HasMaxLength(300);
                entity.Property(item => item.StatusReason).HasMaxLength(1000);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.ClinicReserveId);
                entity.HasOne(item => item.ClinicReserve).WithMany(reserve => reserve.Items).HasForeignKey(item => item.ClinicReserveId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.ClinicService).WithMany().HasForeignKey(item => item.ClinicServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ClinicServicePackage).WithMany().HasForeignKey(item => item.ClinicServicePackageId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SettlementClinic>(entity =>
            {
                // هر رزرو فقط در یک تسویه پرداخت می‌شود.
                entity.HasIndex(item => item.ClinicReserveId).IsUnique();
                entity.HasIndex(item => item.SettlementId);
                entity.HasOne(item => item.Settlement).WithMany().HasForeignKey(item => item.SettlementId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ClinicReserve).WithMany().HasForeignKey(item => item.ClinicReserveId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicReserve>(entity =>
            {
                entity.Property(item => item.CompleteDetail).HasMaxLength(500);
                entity.Property(item => item.ReserveCode).HasMaxLength(40);
                entity.Property(item => item.BookerNote).HasMaxLength(500);
                entity.Property(item => item.CommissionPercent).HasPrecision(5, 2);
                entity.HasIndex(item => item.ReserveCode).IsUnique().HasFilter("[ReserveCode] IS NOT NULL");
                // ظرفیت و هم‌پوشانی نوبت‌ها: بر اساس کلینیک و بازه‌ی زمانی
                entity.HasIndex(item => new { item.ClinicId, item.DoDate, item.EndDate });
                entity.HasIndex(item => new { item.BookerId, item.CreateDate });
                entity.HasIndex(item => new { item.OwnerDecision, item.OwnerApprovalDeadline });
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_ClinicReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.HasCheckConstraint("CK_ClinicReserve_Dates", "[EndDate] >= [DoDate]");
                });

                entity.HasOne(item => item.Clinic).WithMany().HasForeignKey(item => item.ClinicId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ClinicService).WithMany().HasForeignKey(item => item.ClinicServiceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ClinicServicePackage).WithMany().HasForeignKey(item => item.ClinicServicePackageId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Booker).WithMany().HasForeignKey(item => item.BookerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.UserPet).WithMany().HasForeignKey(item => item.UserPetId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Assignee).WithMany().HasForeignKey(item => item.AssigneeUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.ClinicTime).WithMany().HasForeignKey(item => item.ClinicTimeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Address).WithMany().HasForeignKey(item => item.AddressId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(item => item.Rebate).WithMany().HasForeignKey(item => item.RebateId).OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
