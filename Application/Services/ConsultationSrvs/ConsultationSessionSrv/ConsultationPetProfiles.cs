using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationSessionSrv
{
    // پروفایل کامل و سابقه‌ی پزشکی پت(های) یک خرید مشاوره؛ مشترک بین نماینده‌ی کلینیک/مربی و ادمین.
    // بررسی دسترسی با فراخواننده است؛ اینجا فقط داده ساخته می‌شود.
    public static class ConsultationPetProfiles
    {
        private const int MaxPetHistoryItems = 20;

        public static async Task<List<ConsultationPetProfileVDto>> BuildAsync(IDataBaseContext context, long purchaseId, long userId)
        {
            var petIds = await context.ConsultationPurchasePets.AsNoTracking()
                .Where(p => p.ConsultationPurchaseId == purchaseId)
                .Select(p => p.UserPetId)
                .ToListAsync();
            // خرید قدیمی بدون پت انتخاب‌شده: همه‌ی پت‌های فعال مشتری
            var petQuery = context.UserPets.AsNoTracking().Where(p => p.UserId == userId);
            petQuery = petIds.Count > 0 ? petQuery.Where(p => petIds.Contains(p.Id)) : petQuery.Where(p => !p.Deleted);
            var pets = await petQuery
                .Include(p => p.Pet).Include(p => p.PetBreed).Include(p => p.PetBreed2).Include(p => p.Picture)
                .ToListAsync();
            var ids = pets.Select(p => p.Id).ToList();

            var records = (await context.UserPetRecords.AsNoTracking()
                    .Where(r => ids.Contains(r.UserPetId))
                    .OrderByDescending(r => r.CreateDate)
                    .Select(r => new { r.UserPetId, r.Id, r.Name, r.CreateDate, OperatorName = (r.Operator.FirstName + " " + r.Operator.LastName).Trim() })
                    .ToListAsync())
                .GroupBy(r => r.UserPetId)
                .ToDictionary(g => g.Key, g => g.Take(MaxPetHistoryItems).Select(r => new ConsultationPetRecordVDto
                {
                    Id = r.Id, Text = r.Name, OperatorName = r.OperatorName, CreateDate = r.CreateDate
                }).ToList());

            var done = (int)Application.Services.ClinicSrvs.ClinicReserveSrv.ClinicReserveStatusEnum.Complete;
            var visits = (await context.ClinicReserves.AsNoTracking()
                    .Where(r => ids.Contains(r.UserPetId) && r.Status == done && !r.IsCancel)
                    .OrderByDescending(r => r.DoDate)
                    .Select(r => new { r.UserPetId, r.Id, r.DoDate, VenueName = r.Clinic.Name, Title = r.ClinicService.Assistance.Name, r.CompleteDetail })
                    .ToListAsync())
                .GroupBy(r => r.UserPetId)
                .ToDictionary(g => g.Key, g => g.Take(MaxPetHistoryItems).Select(r => new ConsultationPetVisitVDto
                {
                    Id = r.Id, Date = r.DoDate, VenueName = r.VenueName, Title = r.Title, Note = r.CompleteDetail
                }).ToList());

            var completed = (int)ConsultationPurchaseStatusEnum.Completed;
            var consultations = (await context.ConsultationPurchasePets.AsNoTracking()
                    .Where(p => ids.Contains(p.UserPetId) && p.ConsultationPurchaseId != purchaseId && p.ConsultationPurchase.Status == completed)
                    .OrderByDescending(p => p.ConsultationPurchase.StartDate ?? p.ConsultationPurchase.CreateDate)
                    .Select(p => new
                    {
                        p.UserPetId,
                        p.ConsultationPurchase.Id,
                        Date = p.ConsultationPurchase.StartDate ?? p.ConsultationPurchase.CreateDate,
                        VenueName = p.ConsultationPurchase.Companion != null ? p.ConsultationPurchase.Companion.Name
                            : p.ConsultationPurchase.Clinic != null ? p.ConsultationPurchase.Clinic.Name : p.ConsultationPurchase.Coach.Name,
                        p.ConsultationPurchase.PackageName
                    })
                    .ToListAsync())
                .GroupBy(r => r.UserPetId)
                .ToDictionary(g => g.Key, g => g.Take(MaxPetHistoryItems).Select(r => new ConsultationPetVisitVDto
                {
                    Id = r.Id, Date = r.Date, VenueName = r.VenueName, Title = r.PackageName
                }).ToList());

            var now = DateTime.Now;
            var list = pets.Select(p => new ConsultationPetProfileVDto
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
                AgeMonths = Application.Services.CompanionSrvs.CompanionDayScheduleSrv.CompanionDayScheduleRules.AgeMonths(p.Birthday, now),
                Size = p.Size,
                Weight = p.Weight,
                MicroChipCode = p.MicroChipCode,
                SpecificDisease = p.SpecificDisease,
                SpecificMedicine = p.SpecificMedicene,
                Picture = ToPicture(p.Picture),
                Records = records.TryGetValue(p.Id, out var rs) ? rs : new List<ConsultationPetRecordVDto>(),
                ClinicVisits = visits.TryGetValue(p.Id, out var vs) ? vs : new List<ConsultationPetVisitVDto>(),
                PastConsultations = consultations.TryGetValue(p.Id, out var cs) ? cs : new List<ConsultationPetVisitVDto>()
            }).ToList();
            return list;
        }

        private static PictureVDto ToPicture(Picture picture) => picture == null ? null : new PictureVDto
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
