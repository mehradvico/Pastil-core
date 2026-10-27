using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationOwnerSrv
{
    public enum ConsultationOwnerKind
    {
        /// <summary>نمایندگی قدیمی (Companion)</summary>
        Companion = 0,
        /// <summary>کلینیک مستقل</summary>
        Clinic = 1,
        /// <summary>مربی مستقل</summary>
        Coach = 2,
    }

    /// <summary>
    /// مالکِ سامانه‌ی مشاوره‌ی آنلاین فوری: نمایندگی قدیمی، کلینیک یا مربی. بسته/خرید/بازه‌ی رزرو هر کدام دقیقاً یک مالک دارند
    /// (ستون‌های CompanionId / ClinicId / CoachId؛ CHECK در دیتابیس) و همه‌ی سرویس‌ها فقط با همین کلید کار می‌کنند.
    /// </summary>
    public readonly record struct ConsultationOwner(ConsultationOwnerKind Kind, long Id)
    {
        public static ConsultationOwner Companion(long id) => new(ConsultationOwnerKind.Companion, id);
        public static ConsultationOwner Clinic(long id) => new(ConsultationOwnerKind.Clinic, id);
        public static ConsultationOwner Coach(long id) => new(ConsultationOwnerKind.Coach, id);

        /// <summary>مالک از روی سه ستون یک ردیف؛ اگر هیچ‌کدام پر نباشد (داده‌ی خراب) null.</summary>
        public static ConsultationOwner? From(long? companionId, long? clinicId, long? coachId)
        {
            if (clinicId.HasValue) return Clinic(clinicId.Value);
            if (coachId.HasValue) return Coach(coachId.Value);
            if (companionId.HasValue) return Companion(companionId.Value);
            return null;
        }

        public static ConsultationOwner? Of(ConsultationPackage p) => From(p.CompanionId, p.ClinicId, p.CoachId);
        public static ConsultationOwner? Of(ConsultationPurchase p) => From(p.CompanionId, p.ClinicId, p.CoachId);
    }

    public static class ConsultationOwners
    {
        public static IQueryable<ConsultationPackage> ForOwner(this IQueryable<ConsultationPackage> query, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => query.Where(s => s.ClinicId == id),
                ConsultationOwnerKind.Coach => query.Where(s => s.CoachId == id),
                _ => query.Where(s => s.CompanionId == id),
            };
        }

        public static IQueryable<ConsultationPurchase> ForOwner(this IQueryable<ConsultationPurchase> query, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => query.Where(s => s.ClinicId == id),
                ConsultationOwnerKind.Coach => query.Where(s => s.CoachId == id),
                _ => query.Where(s => s.CompanionId == id),
            };
        }

        public static IQueryable<ConsultationAvailability> ForOwner(this IQueryable<ConsultationAvailability> query, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => query.Where(s => s.ClinicId == id),
                ConsultationOwnerKind.Coach => query.Where(s => s.CoachId == id),
                _ => query.Where(s => s.CompanionId == id),
            };
        }

        public static void Assign(this ConsultationPackage row, ConsultationOwner owner)
        {
            row.CompanionId = owner.Kind == ConsultationOwnerKind.Companion ? owner.Id : null;
            row.ClinicId = owner.Kind == ConsultationOwnerKind.Clinic ? owner.Id : null;
            row.CoachId = owner.Kind == ConsultationOwnerKind.Coach ? owner.Id : null;
        }

        public static void Assign(this ConsultationPurchase row, ConsultationOwner owner)
        {
            row.CompanionId = owner.Kind == ConsultationOwnerKind.Companion ? owner.Id : null;
            row.ClinicId = owner.Kind == ConsultationOwnerKind.Clinic ? owner.Id : null;
            row.CoachId = owner.Kind == ConsultationOwnerKind.Coach ? owner.Id : null;
        }

        public static void Assign(this ConsultationAvailability row, ConsultationOwner owner)
        {
            row.CompanionId = owner.Kind == ConsultationOwnerKind.Companion ? owner.Id : null;
            row.ClinicId = owner.Kind == ConsultationOwnerKind.Clinic ? owner.Id : null;
            row.CoachId = owner.Kind == ConsultationOwnerKind.Coach ? owner.Id : null;
        }

        /// <summary>مرکز وجود دارد و حذف نشده (فعال/تأییدشدن بررسی نمی‌شود).</summary>
        /// <summary>
        /// برای نمایش عمومی: Companion قدیمی که به کلینیک/مربی منتقل شده (LegacyCompanionId) و خودش دیگر بسته‌ای ندارد،
        /// بسته‌ها و وضعیت آنلاین همان مرکز را نشان می‌دهد؛ صفحه‌ی قدیمی نمایندگی خالی نمی‌ماند. در غیر این صورت همان مالک برمی‌گردد.
        /// </summary>
        public static async Task<ConsultationOwner> ResolvePublicAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            if (owner.Kind != ConsultationOwnerKind.Companion)
                return owner;
            var id = owner.Id;
            if (await context.ConsultationPackages.AsNoTracking().AnyAsync(s => s.CompanionId == id && !s.Deleted))
                return owner;
            var clinicId = await context.Clinics.AsNoTracking()
                .Where(c => c.LegacyCompanionId == id && !c.Deleted).Select(c => (long?)c.Id).FirstOrDefaultAsync();
            if (clinicId.HasValue)
                return ConsultationOwner.Clinic(clinicId.Value);
            var coachId = await context.Coaches.AsNoTracking()
                .Where(c => c.LegacyCompanionId == id && !c.Deleted).Select(c => (long?)c.Id).FirstOrDefaultAsync();
            return coachId.HasValue ? ConsultationOwner.Coach(coachId.Value) : owner;
        }

        public static Task<bool> ExistsAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => context.Clinics.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted),
                ConsultationOwnerKind.Coach => context.Coaches.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted),
                _ => context.Companions.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted),
            };
        }

        /// <summary>مرکز برای خرید باز است: حذف‌نشده، فعال و تأییدشده.</summary>
        public static Task<bool> IsOpenAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => context.Clinics.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted && s.Active && s.Approved),
                ConsultationOwnerKind.Coach => context.Coaches.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted && s.Active && s.Approved),
                _ => context.Companions.AsNoTracking().AnyAsync(s => s.Id == id && !s.Deleted && s.Active && s.Approved),
            };
        }

        public static async Task<string> NameAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => await context.Clinics.AsNoTracking().Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync(),
                ConsultationOwnerKind.Coach => await context.Coaches.AsNoTracking().Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync(),
                _ => await context.Companions.AsNoTracking().Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync(),
            };
        }

        public static async Task<long?> OwnerUserIdAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => await context.Clinics.AsNoTracking().Where(s => s.Id == id && !s.Deleted).Select(s => (long?)s.OwnerId).FirstOrDefaultAsync(),
                ConsultationOwnerKind.Coach => await context.Coaches.AsNoTracking().Where(s => s.Id == id && !s.Deleted).Select(s => (long?)s.OwnerId).FirstOrDefaultAsync(),
                _ => await context.Companions.AsNoTracking().Where(s => s.Id == id && !s.Deleted).Select(s => (long?)s.OwnerId).FirstOrDefaultAsync(),
            };
        }

        public static async Task<bool> IsOwnerAsync(IDataBaseContext context, long userId, ConsultationOwner owner)
            => await OwnerUserIdAsync(context, owner) == userId;

        /// <summary>اعضای فعال و تأییدشده‌ی تیم (بدون مالک).</summary>
        public static async Task<List<long>> MemberUserIdsAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var id = owner.Id;
            return owner.Kind switch
            {
                ConsultationOwnerKind.Clinic => await context.ClinicUsers.AsNoTracking()
                    .Where(s => s.ClinicId == id && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.UserId).ToListAsync(),
                ConsultationOwnerKind.Coach => await context.CoachUsers.AsNoTracking()
                    .Where(s => s.CoachId == id && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.UserId).ToListAsync(),
                _ => await context.CompanionUsers.AsNoTracking()
                    .Where(s => s.CompanionId == id && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.UserId).ToListAsync(),
            };
        }

        /// <summary>نمایندگان مجاز مشاوره: مالک + همه‌ی اعضای فعال و تأییدشده‌ی تیم (بدون تکرار).</summary>
        public static async Task<List<long>> AgentUserIdsAsync(IDataBaseContext context, ConsultationOwner owner)
        {
            var members = await MemberUserIdsAsync(context, owner);
            var ownerId = await OwnerUserIdAsync(context, owner);
            if (ownerId.HasValue)
                members.Add(ownerId.Value);
            return members.Distinct().ToList();
        }

        /// <summary>مراکزی که کاربر مالک یا عضو فعال و تأییدشده‌شان است، به تفکیک نوع.</summary>
        public static async Task<AgentOwnerIds> OwnerIdsOfAgentAsync(IDataBaseContext context, long userId)
        {
            var companions = context.Companions.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id)
                .Union(context.CompanionUsers.AsNoTracking().Where(s => s.UserId == userId && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.CompanionId));
            var clinics = context.Clinics.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id)
                .Union(context.ClinicUsers.AsNoTracking().Where(s => s.UserId == userId && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.ClinicId));
            var coaches = context.Coaches.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id)
                .Union(context.CoachUsers.AsNoTracking().Where(s => s.UserId == userId && s.Active && s.UserAccept == true && !s.Deleted).Select(s => s.CoachId));
            return new AgentOwnerIds(await companions.ToListAsync(), await clinics.ToListAsync(), await coaches.ToListAsync());
        }

        /// <summary>فقط مراکزی که کاربر «مالک» آن‌هاست (تخصیص خرید به همکار فقط با مالک)، به تفکیک نوع.</summary>
        public static async Task<AgentOwnerIds> OwnedIdsAsync(IDataBaseContext context, long userId)
            => new AgentOwnerIds(
                await context.Companions.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id).ToListAsync(),
                await context.Clinics.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id).ToListAsync(),
                await context.Coaches.AsNoTracking().Where(s => s.OwnerId == userId && !s.Deleted).Select(s => s.Id).ToListAsync());
    }

    public sealed class AgentOwnerIds
    {
        public AgentOwnerIds(List<long> companions, List<long> clinics, List<long> coaches)
        {
            Companions = companions;
            Clinics = clinics;
            Coaches = coaches;
        }

        public List<long> Companions { get; }
        public List<long> Clinics { get; }
        public List<long> Coaches { get; }

        public bool Contains(ConsultationOwner owner) => owner.Kind switch
        {
            ConsultationOwnerKind.Clinic => Clinics.Contains(owner.Id),
            ConsultationOwnerKind.Coach => Coaches.Contains(owner.Id),
            _ => Companions.Contains(owner.Id),
        };

        public bool Contains(ConsultationPurchase p)
        {
            var owner = ConsultationOwner.Of(p);
            return owner.HasValue && Contains(owner.Value);
        }

        public IEnumerable<ConsultationOwner> All() =>
            Companions.Select(ConsultationOwner.Companion)
                .Concat(Clinics.Select(ConsultationOwner.Clinic))
                .Concat(Coaches.Select(ConsultationOwner.Coach));

        /// <summary>خریدهایی که مالکشان یکی از این مراکز است.</summary>
        public IQueryable<ConsultationPurchase> Filter(IQueryable<ConsultationPurchase> query)
        {
            var companions = Companions;
            var clinics = Clinics;
            var coaches = Coaches;
            return query.Where(p => (p.CompanionId != null && companions.Contains(p.CompanionId.Value))
                                    || (p.ClinicId != null && clinics.Contains(p.ClinicId.Value))
                                    || (p.CoachId != null && coaches.Contains(p.CoachId.Value)));
        }
    }
}
