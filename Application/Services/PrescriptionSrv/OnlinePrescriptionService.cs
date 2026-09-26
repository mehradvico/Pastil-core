using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.PrescriptionSrv.Dto;
using Application.Services.PrescriptionSrv.Iface;
using Entities.Entities.PrescriptionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.PrescriptionSrv
{
    /// <summary>
    /// نسخه‌ی پزشک برای رزروهای آنلاین و مشاوره‌ها. مجوزها در همین سرویس بررسی می‌شوند (هیچ endpoint ای فقط به Authorize بسنده نمی‌کند):
    ///  - نوشتن/خواندن نماینده: مالک کلینیک، همکار فعال کلینیک، (برای مشاوره) نماینده‌ی شروع‌کننده؛
    ///  - خواندن کاربر: فقط رزروکننده / خریدار همان هدف.
    /// </summary>
    public class OnlinePrescriptionService : IOnlinePrescriptionService
    {
        private const int MaxTextLength = 4000;
        private const int MaxPictures = 10;

        private readonly IDataBaseContext _context;

        public OnlinePrescriptionService(IDataBaseContext context)
        {
            _context = context;
        }

        // هدف تشخیص‌داده‌شده: دقیقاً یکی از رزرو/خرید + مالکیت‌ها
        private sealed record Resolved(long? ReserveId, long? PurchaseId, long CompanionId, long BookerId, long? AgentUserId, bool Writable);

        private async Task<Resolved> ResolveAsync(PrescriptionTargetDto target)
        {
            if (target == null)
                return null;

            var given = (target.CompanionReserveId.HasValue ? 1 : 0) + (target.ConsultationPurchaseId.HasValue ? 1 : 0) + (target.OnlineSessionId.HasValue ? 1 : 0);
            if (given != 1)
                return null;

            if (target.CompanionReserveId.HasValue)
            {
                var r = await _context.CompanionReserves.AsNoTracking()
                    .Where(s => s.Id == target.CompanionReserveId.Value)
                    .Select(s => new { s.Id, s.BookerId, s.IsReserved, s.IsCancel, CompanionId = s.CompanionAssistance.CompanionId })
                    .FirstOrDefaultAsync();
                return r == null ? null : new Resolved(r.Id, null, r.CompanionId, r.BookerId, null, r.IsReserved && !r.IsCancel);
            }

            var purchasesQuery = _context.ConsultationPurchases.AsNoTracking();
            var purchaseFilter = target.ConsultationPurchaseId.HasValue
                ? purchasesQuery.Where(s => s.Id == target.ConsultationPurchaseId.Value)
                : purchasesQuery.Where(s => s.OnlineSessionId == target.OnlineSessionId.Value);
            var p = await purchaseFilter
                .Select(s => new { s.Id, s.UserId, s.CompanionId, s.AgentUserId, s.Status })
                .FirstOrDefaultAsync();
            if (p == null)
                return null;
            // نسخه‌ی مشاوره فقط بعد از شروع مشاوره توسط نماینده (در جریان یا پایان‌یافته) ثبت می‌شود
            var writable = p.Status == (int)ConsultationPurchaseStatusEnum.Active || p.Status == (int)ConsultationPurchaseStatusEnum.Completed;
            return new Resolved(null, p.Id, p.CompanionId, p.UserId, p.AgentUserId, writable);
        }

        private async Task<bool> IsCompanionSideAsync(Resolved target, long userId)
        {
            if (target.AgentUserId == userId)
                return true;
            var isOwner = await _context.Companions.AsNoTracking().AnyAsync(s => s.Id == target.CompanionId && s.OwnerId == userId);
            if (isOwner)
                return true;
            return await _context.CompanionUsers.AsNoTracking().AnyAsync(s =>
                s.CompanionId == target.CompanionId && s.UserId == userId && !s.Deleted && s.Active && s.UserAccept == true);
        }

        public async Task<BaseResultDto<PrescriptionVDto>> UpsertAsync(long userId, PrescriptionUpsertDto dto)
        {
            try
            {
                var text = string.IsNullOrWhiteSpace(dto?.Text) ? null : dto.Text.Trim();
                var pictureIds = (dto?.PictureIds ?? new List<long>()).Where(id => id > 0).Distinct().ToList();
                if (dto == null || (text == null && pictureIds.Count == 0) ||
                    (text != null && text.Length > MaxTextLength) || pictureIds.Count > MaxPictures)
                    return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.InvalidData, null);

                var target = await ResolveAsync(dto);
                if (target == null)
                    return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.NothingFound, null);
                if (!await IsCompanionSideAsync(target, userId))
                    return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.AccessDenied, null);
                if (!target.Writable)
                    return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.PleaseChangeTheStatus, null);

                if (pictureIds.Count > 0)
                {
                    var existing = await _context.Pictures.AsNoTracking().CountAsync(s => pictureIds.Contains(s.Id));
                    if (existing != pictureIds.Count)
                        return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.InvalidData, null);
                }

                var prescription = await _context.OnlinePrescriptions.AsTracking()
                    .Include(s => s.Pictures)
                    .FirstOrDefaultAsync(s => !s.Deleted &&
                        ((target.ReserveId.HasValue && s.CompanionReserveId == target.ReserveId) ||
                         (target.PurchaseId.HasValue && s.ConsultationPurchaseId == target.PurchaseId)));

                var now = DateTime.Now;
                if (prescription == null)
                {
                    prescription = new OnlinePrescription
                    {
                        CompanionReserveId = target.ReserveId,
                        ConsultationPurchaseId = target.PurchaseId,
                        AuthorUserId = userId,
                        CreateDate = now,
                        Pictures = new List<OnlinePrescriptionPicture>()
                    };
                    await _context.OnlinePrescriptions.AddAsync(prescription);
                }
                else
                {
                    prescription.UpdateDate = now;
                    _context.OnlinePrescriptionPictures.RemoveRange(prescription.Pictures);
                    prescription.Pictures.Clear();
                }

                prescription.Text = text;
                for (var i = 0; i < pictureIds.Count; i++)
                    prescription.Pictures.Add(new OnlinePrescriptionPicture { PictureId = pictureIds[i], SortOrder = i });

                await _context.SaveChangesAsync();
                return await LoadAsync(prescription.Id);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PrescriptionVDto>(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<PrescriptionVDto>> GetForCompanionAsync(long userId, PrescriptionTargetDto target)
        {
            var resolved = await ResolveAsync(target);
            if (resolved == null)
                return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.NothingFound, null);
            if (!await IsCompanionSideAsync(resolved, userId))
                return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.AccessDenied, null);
            return await LoadForTargetAsync(resolved);
        }

        public async Task<BaseResultDto<PrescriptionVDto>> GetForBookerAsync(long userId, PrescriptionTargetDto target)
        {
            var resolved = await ResolveAsync(target);
            if (resolved == null || resolved.BookerId != userId)
                return new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.NothingFound, null);
            return await LoadForTargetAsync(resolved);
        }

        private async Task<BaseResultDto<PrescriptionVDto>> LoadForTargetAsync(Resolved target)
        {
            var id = await _context.OnlinePrescriptions.AsNoTracking()
                .Where(s => !s.Deleted &&
                    ((target.ReserveId.HasValue && s.CompanionReserveId == target.ReserveId) ||
                     (target.PurchaseId.HasValue && s.ConsultationPurchaseId == target.PurchaseId)))
                .Select(s => (long?)s.Id)
                .FirstOrDefaultAsync();
            // هنوز نسخه‌ای ثبت نشده: موفق با data = null (کلاینت «بدون نسخه» نشان می‌دهد)
            if (!id.HasValue)
                return new BaseResultDto<PrescriptionVDto>(true, null);
            return await LoadAsync(id.Value);
        }

        private async Task<BaseResultDto<PrescriptionVDto>> LoadAsync(long id)
        {
            var item = await _context.OnlinePrescriptions.AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new PrescriptionVDto
                {
                    Id = s.Id,
                    CompanionReserveId = s.CompanionReserveId,
                    ConsultationPurchaseId = s.ConsultationPurchaseId,
                    Text = s.Text,
                    AuthorName = ((s.AuthorUser.FirstName ?? "") + " " + (s.AuthorUser.LastName ?? "")).Trim(),
                    CreateDate = s.CreateDate,
                    UpdateDate = s.UpdateDate,
                    Pictures = s.Pictures.OrderBy(p => p.SortOrder).Select(p => new PictureVDto
                    {
                        Id = p.Picture.Id,
                        Url = p.Picture.Url,
                        OrginalName = p.Picture.OrginalName,
                        GuidName = p.Picture.GuidName,
                        Extension = p.Picture.Extension
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            return item == null
                ? new BaseResultDto<PrescriptionVDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<PrescriptionVDto>(true, item);
        }
    }
}
