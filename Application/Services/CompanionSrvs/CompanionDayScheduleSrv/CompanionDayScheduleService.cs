using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionDayScheduleSrv
{
    // برنامه‌ی روز نماینده/کلینیک (صفحه‌ی «برنامه‌ی امروز» اپ نماینده). طراحی: backend/Docs/COMPANION_DAY_SCHEDULE_AMIRMOHSEN_FA.md
    public class CompanionDayScheduleService : ICompanionDayScheduleService
    {
        // سقف آیتم‌های یک روز؛ برای محافظت از پاسخ (یک روز معمولی ده‌ها رزرو دارد)
        private const int MaxItems = 300;
        private const int MaxConsultationCustomerPets = 10;

        private readonly IDataBaseContext _context;

        public CompanionDayScheduleService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<CompanionDayScheduleVDto>> GetAsync(long userId, long? requestedCompanionId, DateTime date, bool includeCancelled, bool mineOnly)
        {
            try
            {
                // کلینیک‌های قابل‌دسترس: مالک + عضو فعال و تأییدشده‌ی تیم. CurrentUser.CompanionId فقط کلینیکِ «مالک» است و برای
                // اعضای تیم (اپراتور/دکتر) خالی است، پس اینجا از روی خود دیتابیس پیدا می‌شود.
                var owned = await _context.Companions.AsNoTracking()
                    .Where(c => c.OwnerId == userId && !c.Deleted)
                    .Select(c => new DayClinicVDto { Id = c.Id, Name = c.Name, IsOwner = true })
                    .ToListAsync();
                var ownedIds = owned.Select(c => c.Id).ToList();
                var memberOf = await _context.CompanionUsers.AsNoTracking()
                    .Where(s => s.UserId == userId && s.Active && s.UserAccept == true && !s.Deleted && !s.Companion.Deleted && !ownedIds.Contains(s.CompanionId))
                    .Select(s => new DayClinicVDto { Id = s.CompanionId, Name = s.Companion.Name, IsOwner = false })
                    .Distinct()
                    .ToListAsync();
                var clinics = owned.Concat(memberOf).OrderByDescending(c => c.IsOwner).ThenBy(c => c.Id).ToList();
                if (clinics.Count == 0)
                    return new BaseResultDto<CompanionDayScheduleVDto>(false, Resource.Notification.AccessDenied, null);

                DayClinicVDto selected;
                if (requestedCompanionId.HasValue)
                {
                    selected = clinics.FirstOrDefault(c => c.Id == requestedCompanionId.Value);
                    if (selected == null)
                        return new BaseResultDto<CompanionDayScheduleVDto>(false, Resource.Notification.AccessDenied, null);
                }
                else
                {
                    // پیش‌فرض: اولین کلینیکی که مالکش است، وگرنه اولین کلینیکی که عضو آن است
                    selected = clinics[0];
                }

                var companion = new { Id = selected.Id, selected.Name };
                var companionId = selected.Id;
                var isOwner = selected.IsOwner;

                var now = DateTime.Now;
                var day = date.Date;
                var dayEnd = day.AddDays(1);
                // مالک بدون mineOnly همه را می‌بیند؛ عضو تیم یا mineOnly فقط رزروهای تخصیص‌یافته به خودش
                var onlyMine = !isOwner || mineOnly;

                var result = new CompanionDayScheduleVDto
                {
                    CompanionId = companion.Id,
                    CompanionName = companion.Name,
                    Date = day,
                    ServerNow = now,
                    DayOfWeek = day.DayOfWeek.ToString(),
                    WeekDayId = ConsultationBookingRules.WeekDayId(day),
                    IsOwner = isOwner,
                    AvailableClinics = clinics
                };

                var dayName = day.DayOfWeek.ToString();
                result.WorkingHours = await _context.CompanionTimes.AsNoTracking()
                    .Where(t => t.CompanionId == companionId && t.Active && !t.Deleted && t.WeekDay.Label == dayName)
                    .OrderBy(t => t.StartTime)
                    .Select(t => new DayWorkingHourVDto { Id = t.Id, Start = t.StartTime, End = t.EndTime, Capacity = t.Capacity })
                    .ToListAsync();

                var items = new List<DayScheduleItemVDto>();
                items.AddRange(await LoadServiceItemsAsync(userId, companionId, day, dayEnd, now, includeCancelled, onlyMine));
                items.AddRange(await LoadConsultationItemsAsync(userId, companionId, day, dayEnd, now, includeCancelled, onlyMine));

                result.Items = items
                    .OrderBy(i => i.Start)
                    .ThenBy(i => i.Kind)
                    .ThenBy(i => i.Id)
                    .Take(MaxItems)
                    .ToList();

                result.Summary = new DayScheduleSummaryVDto
                {
                    Total = result.Items.Count,
                    Upcoming = result.Items.Count(i => i.Status == DayItemStatus.Upcoming),
                    Now = result.Items.Count(i => i.Status == DayItemStatus.Now),
                    Done = result.Items.Count(i => i.Status == DayItemStatus.Done),
                    Missed = result.Items.Count(i => i.Status == DayItemStatus.Missed),
                    Cancelled = result.Items.Count(i => i.Status == DayItemStatus.Cancelled)
                };

                return new BaseResultDto<CompanionDayScheduleVDto>(true, result);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionDayScheduleVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        // ---- رزروهای خدمت
        private async Task<List<DayScheduleItemVDto>> LoadServiceItemsAsync(long userId, long companionId, DateTime day, DateTime dayEnd,
            DateTime now, bool includeCancelled, bool onlyMine)
        {
            var query = _context.CompanionReserves.AsNoTracking()
                .Where(r => r.CompanionAssistance.CompanionId == companionId && r.IsReserved && r.DoDate >= day && r.DoDate < dayEnd);
            if (!includeCancelled)
                query = query.Where(r => !r.IsCancel);
            if (onlyMine)
                query = query.Where(r => r.CompanionAssistanceUser != null && r.CompanionAssistanceUser.UserId == userId);

            var rows = await query
                .Include(r => r.Booker).ThenInclude(u => u.Picture)
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Assistance)
                .Include(r => r.CompanionTime)
                .Include(r => r.CompanionAssistanceTime)
                .Include(r => r.CompanionAssistanceType)
                .Include(r => r.State)
                .Include(r => r.CompanionAssistanceUser).ThenInclude(a => a.User)
                .Include(r => r.Address).ThenInclude(a => a.City)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(o => o.CompanionAssistancePackageOnline)
                .Include(r => r.CompanionAssistancePackages)
                .Include(r => r.UserPets).ThenInclude(p => p.Pet)
                .Include(r => r.UserPets).ThenInclude(p => p.PetBreed)
                .Include(r => r.UserPets).ThenInclude(p => p.PetBreed2)
                .Include(r => r.UserPets).ThenInclude(p => p.Picture)
                .OrderBy(r => r.DoDate).ThenBy(r => r.Id)
                .Take(MaxItems)
                .AsSplitQuery()
                .ToListAsync();

            return rows.Select(r =>
            {
                var startText = r.CompanionTime?.StartTime ?? r.CompanionAssistanceTime?.StartTime;
                var endText = r.CompanionTime?.EndTime ?? r.CompanionAssistanceTime?.EndTime;
                var (start, end) = CompanionDayScheduleRules.ServiceRange(r.DoDate, startText, endText);
                var online = r.CompanionAssistancePackageOnlineSelection?.CompanionAssistancePackageOnline;
                return new DayScheduleItemVDto
                {
                    Kind = DayItemKinds.Service,
                    Id = r.Id,
                    Code = r.ReserveCode,
                    Start = start,
                    End = end,
                    StartTime = CompanionDayScheduleRules.TimeText(start),
                    EndTime = CompanionDayScheduleRules.TimeText(end),
                    Status = CompanionDayScheduleRules.ServiceStatus(r.IsCancel, r.DoneDate, start, end, now),
                    Title = r.CompanionAssistance?.Assistance?.Name,
                    ServiceModeName = r.CompanionAssistanceType?.Name,
                    StateName = r.State?.Name,
                    IsOnline = r.CompanionAssistancePackageOnlineSelectionId.HasValue,
                    OnlineMethodName = online?.Name,
                    Customer = ToCustomer(r.Booker),
                    Pets = (r.UserPets ?? new List<UserPet>()).Select(p => ToPet(p, now)).ToList(),
                    Assignee = r.CompanionAssistanceUser == null ? null : new DayAssigneeVDto
                    {
                        UserId = r.CompanionAssistanceUser.UserId,
                        FullName = FullName(r.CompanionAssistanceUser.User),
                        IsMe = r.CompanionAssistanceUser.UserId == userId
                    },
                    Packages = (r.CompanionAssistancePackages ?? new List<CompanionAssistancePackage>())
                        .Select(p => new DayPackageVDto { Name = p.Name, PetSize = p.PetSize }).ToList(),
                    Address = r.Address == null ? null : new DayAddressVDto
                    {
                        AddressValue = r.Address.AddressValue,
                        Unit = r.Address.Unit,
                        Floor = r.Address.Floor,
                        CityName = r.Address.City?.Name,
                        Latitude = r.Address.Location?.Y,
                        Longitude = r.Address.Location?.X
                    },
                    CustomerNote = r.BookerDetail,
                    AssistanceDetail = r.AssistanceDetail,
                    CancelDetail = r.IsCancel ? r.CancelDetail : null,
                    PaymentPrice = r.PaymentPrice,
                    OperatorUnpaid = r.OperatorUnpaid && r.OperatorDebtPaidDate == null,
                    OperatorUnpaidAmount = r.OperatorUnpaidAmount
                };
            }).ToList();
        }

        // ---- مشاوره‌های آنلاین رزروشده با ساعت (ConsultationPurchase.ScheduledStart)
        private async Task<List<DayScheduleItemVDto>> LoadConsultationItemsAsync(long userId, long companionId, DateTime day, DateTime dayEnd,
            DateTime now, bool includeCancelled, bool onlyMine)
        {
            var statuses = new List<int>
            {
                (int)ConsultationPurchaseStatusEnum.Paid,
                (int)ConsultationPurchaseStatusEnum.Active,
                (int)ConsultationPurchaseStatusEnum.Completed
            };
            if (includeCancelled)
            {
                statuses.Add((int)ConsultationPurchaseStatusEnum.Expired);
                statuses.Add((int)ConsultationPurchaseStatusEnum.Cancelled);
                statuses.Add((int)ConsultationPurchaseStatusEnum.Refunded);
            }

            var query = _context.ConsultationPurchases.AsNoTracking()
                .Where(c => c.CompanionId == companionId && c.ScheduledStart != null &&
                            c.ScheduledStart >= day && c.ScheduledStart < dayEnd && statuses.Contains(c.Status));
            // مشاوره‌ی بی‌نماینده را هر عضو تیم می‌تواند شروع کند؛ پس برای عضو هم دیده می‌شود. «mineOnly» فقط تخصیص‌یافته‌ها
            if (onlyMine)
                query = query.Where(c => c.AgentUserId == userId || c.AgentUserId == null);

            var rows = await query
                .Include(c => c.User).ThenInclude(u => u.Picture)
                .Include(c => c.AgentUser)
                .OrderBy(c => c.ScheduledStart).ThenBy(c => c.Id)
                .Take(MaxItems)
                .ToListAsync();
            if (rows.Count == 0)
                return new List<DayScheduleItemVDto>();

            // پروفایل پت‌ها: مشاوره پت مشخصی ندارد؛ پت‌های فعال مشتری برای آماده‌شدن نماینده
            var customerIds = rows.Select(c => c.UserId).Distinct().ToList();
            var pets = (await _context.UserPets.AsNoTracking()
                    .Where(p => customerIds.Contains(p.UserId) && !p.Deleted)
                    .Include(p => p.Pet).Include(p => p.PetBreed).Include(p => p.PetBreed2).Include(p => p.Picture)
                    .ToListAsync())
                .GroupBy(p => p.UserId)
                .ToDictionary(g => g.Key, g => g.Take(MaxConsultationCustomerPets).Select(p => ToPet(p, now)).ToList());

            return rows.Select(c => new DayScheduleItemVDto
            {
                Kind = DayItemKinds.Consultation,
                Id = c.Id,
                Code = c.PurchaseCode,
                Start = c.ScheduledStart.Value,
                End = c.ScheduledEnd,
                StartTime = CompanionDayScheduleRules.TimeText(c.ScheduledStart),
                EndTime = CompanionDayScheduleRules.TimeText(c.ScheduledEnd),
                Status = CompanionDayScheduleRules.ConsultationStatus(c.Status, c.StartDeadline, now),
                Title = c.PackageName,
                IsOnline = true,
                Customer = ToCustomer(c.User),
                Pets = pets.TryGetValue(c.UserId, out var list) ? list : new List<DayPetVDto>(),
                Assignee = c.AgentUserId.HasValue ? new DayAssigneeVDto
                {
                    UserId = c.AgentUserId.Value,
                    FullName = FullName(c.AgentUser),
                    IsMe = c.AgentUserId.Value == userId
                } : null,
                CancelDetail = c.CancelReason,
                PaymentPrice = c.PaymentPrice,
                ChannelId = c.ChannelId,
                DurationMinutes = c.DurationMinutes,
                ConsultationStatus = c.Status,
                OnlineSessionId = c.OnlineSessionId,
                CanStart = CompanionDayScheduleRules.ConsultationCanStart(c.Status, c.StartDeadline, now, c.ScheduledStart)
                           && (c.AgentUserId == null || c.AgentUserId == userId)
            }).ToList();
        }

        private static string FullName(Entities.Entities.Security.User user) =>
            user == null ? null : $"{user.FirstName} {user.LastName}".Trim();

        private static DayCustomerVDto ToCustomer(Entities.Entities.Security.User user) => user == null ? null : new DayCustomerVDto
        {
            UserId = user.Id,
            FullName = FullName(user),
            Mobile = user.Mobile,
            Picture = ToPicture(user.Picture)
        };

        private static DayPetVDto ToPet(UserPet p, DateTime now) => new()
        {
            UserPetId = p.Id,
            Name = p.Name,
            PetName = p.Pet?.Name,
            BreedName = p.PetBreed?.Name,
            Breed2Name = p.PetBreed2?.Name,
            IsMixBreed = p.IsMixBreed,
            IsMale = p.IsMale,
            IsSterile = p.IsSterile,
            Birthday = p.Birthday,
            AgeMonths = CompanionDayScheduleRules.AgeMonths(p.Birthday, now),
            Size = p.Size,
            Weight = p.Weight,
            SpecificDisease = p.SpecificDisease,
            SpecificMedicine = p.SpecificMedicene,
            MicroChipCode = p.MicroChipCode,
            Picture = ToPicture(p.Picture)
        };

        private static PictureVDto ToPicture(Entities.Entities.Picture picture) => picture == null ? null : new PictureVDto
        {
            Id = picture.Id,
            BaseUrl = picture.Url,
            Url = picture.Url + "/" + picture.Name,
            OrginalName = picture.OrginalName,
            GuidName = picture.GuidName,
            Extension = picture.Extension
        };
    }
}
