using Application.Common.Enumerable;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Entities.Entities.PansionField;
using Entities.Entities.SchoolField;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FinanceSrvs.SettlementSrv
{
    /// <summary>
    /// تعریف یگانه‌ی «چه ردیف‌هایی قابل تسویه‌اند». هم ثبت تسویه (SettlementService) و هم محاسبه‌ی مانده‌ها (ProviderFinanceService)
    /// از همین‌جا می‌خوانند تا عددی که ادمین می‌بیند دقیقاً همان چیزی باشد که تسویه می‌شود.
    /// فیلتر Permitted عمداً اینجا نیست: مصرف‌کننده تصمیم می‌گیرد مانده (false) یا تسویه‌شده (true) را بخواهد.
    /// </summary>
    public static class SettlementSources
    {
        /// <summary>رزرو خدمت نماینده‌ها: رزروشده و لغونشده.</summary>
        public static IQueryable<CompanionReserve> CompanionReserves(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => context.CompanionReserves
                .Where(r => r.IsReserved && !r.IsCancel && companionIds.Contains(r.CompanionAssistance.CompanionId));

        /// <summary>
        /// رزرو پانسیون/مهد: پرداخت‌شده، لغونشده، «تکمیل‌شده» و تأییدشده توسط مرکز (یا بدون نیاز به تأیید).
        /// رزروی که هنوز منتظر تأیید است ممکن است رد/منقضی شود و پولش به کیف پول برگردد؛ پس وارد استخر تسویه نمی‌شود.
        /// </summary>
        public static IQueryable<PansionReserve> PansionReserves(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
        {
            var complete = (long)Application.Common.Enumerable.Code.PansionReserveStatusEnum.PansionReserveState_Complete;
            var notRequired = (int)Application.Common.Enumerable.Code.PansionReserveOwnerDecisionEnum.NotRequired;
            var approved = (int)Application.Common.Enumerable.Code.PansionReserveOwnerDecisionEnum.Approved;
            return context.PansionReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.StatusId == complete
                            && (r.OwnerDecision == notRequired || r.OwnerDecision == approved)
                            && companionIds.Contains(r.Pansion.CompanionId));
        }

        /// <summary>مشاوره‌ی آنلاین: فقط «تکمیل‌شده» (بعد از آن دیگر قابل بازپرداخت نیست).</summary>
        public static IQueryable<ConsultationPurchase> ConsultationPurchases(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
        {
            var completed = (int)ConsultationPurchaseStatusEnum.Completed;
            return context.ConsultationPurchases
                .Where(p => p.CompanionId != null && companionIds.Contains(p.CompanionId.Value) && p.Status == completed);
        }

        /// <summary>مشاوره‌ی آنلاین کلینیک‌های مستقل: فقط «تکمیل‌شده» (مثل نمایندگی قدیمی).</summary>
        public static IQueryable<ConsultationPurchase> ClinicConsultationPurchases(IDataBaseContext context, IReadOnlyCollection<long> clinicIds)
        {
            var completed = (int)ConsultationPurchaseStatusEnum.Completed;
            return context.ConsultationPurchases
                .Where(p => p.ClinicId != null && clinicIds.Contains(p.ClinicId.Value) && p.Status == completed);
        }

        /// <summary>مشاوره‌ی آنلاین مربی‌های مستقل: فقط «تکمیل‌شده».</summary>
        public static IQueryable<ConsultationPurchase> CoachConsultationPurchases(IDataBaseContext context, IReadOnlyCollection<long> coachIds)
        {
            var completed = (int)ConsultationPurchaseStatusEnum.Completed;
            return context.ConsultationPurchases
                .Where(p => p.CoachId != null && coachIds.Contains(p.CoachId.Value) && p.Status == completed);
        }

        /// <summary>ثبت‌نام دوره‌ی مدرسه: رزروشده و لغونشده.</summary>
        /// <summary>ثبت‌نام مدرسه‌های زیر نمایندگی (مدرسه‌های قدیمی/دارای CompanionId)؛ در مجموع هر نمایندگی حساب می‌شوند.</summary>
        public static IQueryable<SchoolReserve> SchoolReserves(IDataBaseContext context, IReadOnlyCollection<long> companionIds)
            => context.SchoolReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.SchoolCourse.School.CompanionId != null
                            && companionIds.Contains(r.SchoolCourse.School.CompanionId.Value));

        /// <summary>
        /// ثبت‌نام مدرسه‌هایی که فقط به ارائه‌دهنده وصل‌اند (بدون نمایندگی، ۲۰۲۶-۱۰-۲۶)؛ در استخر «نمایندگی» همان ارائه‌دهنده تسویه می‌شوند
        /// (همان لینک SettlementCompanion.SchoolReserveId). مدرسه‌ی دارای CompanionId از مسیر SchoolReserves می‌آید تا دوبار شمرده نشود.
        /// </summary>
        public static IQueryable<SchoolReserve> ProviderSchoolReserves(IDataBaseContext context, long providerId)
            => context.SchoolReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.SchoolCourse.School.CompanionId == null
                            && r.SchoolCourse.School.ProviderId == providerId);

        public static IQueryable<SchoolReserve> ProviderSchoolReserves(IDataBaseContext context, IReadOnlyCollection<long> providerIds)
            => context.SchoolReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.SchoolCourse.School.CompanionId == null
                            && r.SchoolCourse.School.ProviderId != null && providerIds.Contains(r.SchoolCourse.School.ProviderId.Value));

        /// <summary>رزرو آرایشگاه: فقط «تکمیل‌شده»، پرداخت‌شده و لغونشده (بعد از تکمیل دیگر خطر لغو/برگشت وجه ندارد).</summary>
        public static IQueryable<Entities.Entities.GroomerField.GroomerReserve> GroomerReserves(IDataBaseContext context, IReadOnlyCollection<long> groomerIds)
        {
            var complete = (int)Application.Services.GroomerSrvs.GroomerReserveSrv.GroomerReserveStatusEnum.Complete;
            return context.GroomerReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.Status == complete && groomerIds.Contains(r.GroomerId));
        }

        public static IQueryable<Entities.Entities.ClinicField.ClinicReserve> ClinicReserves(IDataBaseContext context, IReadOnlyCollection<long> clinicIds)
        {
            var complete = (int)Application.Services.ClinicSrvs.ClinicReserveSrv.ClinicReserveStatusEnum.Complete;
            return context.ClinicReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.Status == complete && clinicIds.Contains(r.ClinicId));
        }
        public static IQueryable<Entities.Entities.CoachField.CoachReserve> CoachReserves(IDataBaseContext context, IReadOnlyCollection<long> coachIds)
        {
            var complete = (int)Application.Services.CoachSrvs.CoachReserveSrv.CoachReserveStatusEnum.Complete;
            return context.CoachReserves
                .Where(r => r.IsReserved && !r.IsCancel && r.Status == complete && coachIds.Contains(r.CoachId));
        }

        /// <summary>سفارش فروشگاه‌ها؛ سفارشی که چند فروشگاه‌ِ یک ارائه‌دهنده دارد فقط یک‌بار شمرده می‌شود.</summary>
        public static IQueryable<ProductOrder> StoreOrders(IDataBaseContext context, IReadOnlyCollection<long> storeIds)
            => context.ProductOrders
                .Where(o => !o.Deleted && o.ProductOrderStores.Any(s => storeIds.Contains(s.StoreId)));
    }
}
