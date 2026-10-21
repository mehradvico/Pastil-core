using Application.Common.Enumerable.Code;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Services.CompanionSrvs.CompanionSrv
{
    /// <summary>
    /// Companionی که به Groomer (آرایشگاه)، Clinic (کلینیک/آزمایشگاه/پرستار) یا Coach (مربی) منتقل شده و نوع دیگری ندارد، دیگر به‌عنوان نماینده نمایش داده نمی‌شود.
    /// اگر نوعی داشته باشد که منتقل نشده (مثلاً دوگ‌واکر) همچنان دیده می‌شود.
    /// </summary>
    public static class CompanionMigrationFilter
    {
        private const long Grooming = (long)CompanionTypeEnum.CompanionType_Grooming;
        private const long ClinicType = (long)CompanionTypeEnum.CompanionType_Clinic;
        private const long Laboratory = (long)CompanionTypeEnum.CompanionType_Laboratory;
        private const long Nurse = (long)CompanionTypeEnum.CompanionType_Nurse;

        // مربی در enum کد نوع ندارد؛ برچسب نوع Companion در جدول Codes است (همان که وب‌اپ برای «مربیان» می‌خواند).
        private const string CoachTypeLabel = "CompanionType_Coach";

        public static Expression<Func<Companion, bool>> NotFullyMigrated(IDataBaseContext context) =>
            c => !((context.Groomers.Any(g => g.LegacyCompanionId == c.Id && !g.Deleted) ||
                    context.Clinics.Any(k => k.LegacyCompanionId == c.Id && !k.Deleted) ||
                    context.Coaches.Any(k => k.LegacyCompanionId == c.Id && !k.Deleted))
                   && !c.CompanionTypes.Any(t => !t.Deleted &&
                        !((t.TypeId == Grooming && context.Groomers.Any(g => g.LegacyCompanionId == c.Id && !g.Deleted)) ||
                          ((t.TypeId == ClinicType || t.TypeId == Laboratory || t.TypeId == Nurse) && context.Clinics.Any(k => k.LegacyCompanionId == c.Id && !k.Deleted)) ||
                          (t.Type.Label == CoachTypeLabel && context.Coaches.Any(k => k.LegacyCompanionId == c.Id && !k.Deleted)))));
    }
}
