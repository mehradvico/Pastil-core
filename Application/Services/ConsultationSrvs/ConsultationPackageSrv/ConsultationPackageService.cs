using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv
{
    public class ConsultationPackageService : IConsultationPackageService
    {
        private readonly IDataBaseContext _context;
        private readonly Application.Services.CompanionSrvs.ConsultationOnlineSrv.IConsultationOnlineService _onlineService;

        public ConsultationPackageService(IDataBaseContext context, Application.Services.CompanionSrvs.ConsultationOnlineSrv.IConsultationOnlineService onlineService)
        {
            _context = context;
            _onlineService = onlineService;
        }

        public async Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetListAsync(ConsultationOwner owner)
        {
            try
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(true, await LoadListAsync(owner));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> CreateAsync(ConsultationOwner owner, ConsultationPackageItemDto dto)
        {
            try
            {
                var invalid = await ValidateAsync<ConsultationPackageItemDto>(owner, dto, isUpdate: false);
                if (invalid != null)
                    return invalid;

                var now = DateTime.Now;
                var nextOrder = await _context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner).Where(s => !s.Deleted && s.ChannelId == dto.ChannelId)
                    .Select(s => (int?)s.SortOrder)
                    .MaxAsync() ?? 0;

                var row = new ConsultationPackage
                {
                    ChannelId = dto.ChannelId,
                    DurationMinutes = dto.DurationMinutes,
                    Price = dto.Price,
                    Active = dto.Active,
                    // بسته‌ی کلینیک/مربی تا تأیید ادمین به کاربر نمایش داده نمی‌شود
                    Approved = owner.Kind == ConsultationOwnerKind.Companion,
                    Bookable = dto.Bookable ?? false,
                    Name = ConsultationPackageRules.NormalizeName(dto.Name),
                    Description = ConsultationPackageRules.NormalizeDescription(dto.Description),
                    PictureId = dto.PictureId is > 0 ? dto.PictureId : null,
                    SortOrder = dto.SortOrder > 0 ? dto.SortOrder : nextOrder + 1,
                    CreateDate = now
                };
                row.Assign(owner);
                await _context.ConsultationPackages.AddAsync(row);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ConsultationPackageItemDto>(true, await LoadOneAsync(owner, row.Id));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPackageItemDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> UpdateAsync(ConsultationOwner owner, ConsultationPackageItemDto dto)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return new BaseResultDto<ConsultationPackageItemDto>(false, Resource.Notification.InvalidData, null);

                var invalid = await ValidateAsync<ConsultationPackageItemDto>(owner, dto, isUpdate: true);
                if (invalid != null)
                    return invalid;

                var row = await _context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner).FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
                if (row == null)
                    return new BaseResultDto<ConsultationPackageItemDto>(false, Resource.Notification.NothingFound, null);

                // تغییر محتوایی بسته‌ی کلینیک/مربی دوباره به تأیید ادمین نیاز دارد (روشن/خاموش کردن و ترتیب نه)
                if (owner.Kind != ConsultationOwnerKind.Companion && row.Approved && (
                        row.ChannelId != dto.ChannelId || row.DurationMinutes != dto.DurationMinutes || row.Price != dto.Price
                        || row.Name != ConsultationPackageRules.NormalizeName(dto.Name)
                        || row.Description != ConsultationPackageRules.NormalizeDescription(dto.Description)
                        || row.PictureId != (dto.PictureId is > 0 ? dto.PictureId : null)
                        || (dto.Bookable.HasValue && row.Bookable != dto.Bookable.Value)))
                    row.Approved = false;
                row.ChannelId = dto.ChannelId;
                row.DurationMinutes = dto.DurationMinutes;
                row.Price = dto.Price;
                row.Active = dto.Active;
                if (dto.Bookable.HasValue)
                    row.Bookable = dto.Bookable.Value;
                row.Name = ConsultationPackageRules.NormalizeName(dto.Name);
                row.Description = ConsultationPackageRules.NormalizeDescription(dto.Description);
                row.PictureId = dto.PictureId is > 0 ? dto.PictureId : null;
                if (dto.SortOrder > 0)
                    row.SortOrder = dto.SortOrder;
                row.UpdateDate = DateTime.Now;
                // کانتکست پروژه پیش‌فرض NoTracking است؛ Update صریح لازم است
                _context.ConsultationPackages.Update(row);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ConsultationPackageItemDto>(true, await LoadOneAsync(owner, row.Id));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPackageItemDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> DeleteAsync(ConsultationOwner owner, long id)
        {
            try
            {
                var affected = await _context.ConsultationPackages
                    .ForOwner(owner).Where(s => s.Id == id && !s.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Deleted, true)
                        .SetProperty(s => s.Active, false)
                        .SetProperty(s => s.UpdateDate, (DateTime?)DateTime.Now));
                return affected == 0
                    ? new BaseResultDto(false, Resource.Notification.NothingFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public async Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(ConsultationOwner owner)
        {
            try
            {
                var companionOpen = await ConsultationOwners.IsOpenAsync(_context, owner);
                if (!companionOpen)
                    return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, new List<ConsultationPackagePublicVDto>());

                var list = await _context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner).Where(s => !s.Deleted && s.Active && s.Approved && s.Price > 0)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder).ThenBy(s => s.DurationMinutes).ThenBy(s => s.Id)
                    .Select(s => new ConsultationPackagePublicVDto
                    {
                        Id = s.Id,
                        CompanionId = s.CompanionId,
                        ClinicId = s.ClinicId,
                        CoachId = s.CoachId,
                        ChannelId = s.ChannelId,
                        DurationMinutes = s.DurationMinutes,
                        Price = s.Price,
                        Bookable = s.Bookable,
                        Name = s.Name,
                        Description = s.Description,
                        SortOrder = s.SortOrder,
                        Picture = s.Picture == null ? null : new PictureVDto
                        {
                            Id = s.Picture.Id,
                            Url = s.Picture.Url,
                            OrginalName = s.Picture.OrginalName,
                            GuidName = s.Picture.GuidName,
                            Extension = s.Picture.Extension
                        }
                    })
                    .ToListAsync();

                var online = (await _onlineService.GetPublicAsync(owner)).Online;
                list.ForEach(s => s.CompanionOnline = online);

                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<List<SearchConsultationPackageDto>> SearchMinAsync(SearchRequestDto request)
        {
            var predicate = SearchQueryHelper.ContainsAny<ConsultationPackage>(request.SearchTerms,
                item => item.Name, item => item.Description, item => item.Companion.Name, item => item.Clinic.Name, item => item.Coach.Name);
            var query = _context.ConsultationPackages.AsNoTracking()
                .Where(s => !s.Deleted && s.Active && s.Approved && s.Price > 0 &&
                            ((s.Companion != null && !s.Companion.Deleted && s.Companion.Active && s.Companion.Approved)
                             || (s.Clinic != null && !s.Clinic.Deleted && s.Clinic.Active && s.Clinic.Approved)
                             || (s.Coach != null && !s.Coach.Deleted && s.Coach.Active && s.Coach.Approved)));
            return await query.Where(predicate)
                .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder)
                .Take(SearchQueryHelper.CandidateCount(request.ConsultationPackageCount))
                .Select(s => new SearchConsultationPackageDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CompanionId = s.CompanionId,
                    ClinicId = s.ClinicId,
                    CoachId = s.CoachId,
                    CompanionName = s.Companion != null ? s.Companion.Name : s.Clinic != null ? s.Clinic.Name : s.Coach.Name,
                    ChannelId = s.ChannelId,
                    DurationMinutes = s.DurationMinutes,
                    Price = s.Price,
                    Description = s.Description,
                    Picture = s.Picture == null ? null : new Application.Services.Filing.PictureSrv.Dto.PictureVDto { Id = s.Picture.Id, Url = s.Picture.Url, OrginalName = s.Picture.OrginalName, GuidName = s.Picture.GuidName, Extension = s.Picture.Extension }
                })
                .ToListAsync();
        }

        // null یعنی معتبر؛ غیر از آن پاسخ خطای آماده‌ی برگشتی
        private async Task<BaseResultDto<T>> ValidateAsync<T>(ConsultationOwner owner, ConsultationPackageItemDto dto, bool isUpdate) where T : class
        {
            switch (ConsultationPackageRules.Validate(dto))
            {
                case ConsultationPackageRules.Problem.None:
                    break;
                case ConsultationPackageRules.Problem.InvalidName:
                    return new BaseResultDto<T>(false, Resource.Notification.ConsultationPackageNameInvalid, null);
                case ConsultationPackageRules.Problem.DescriptionTooLong:
                    return new BaseResultDto<T>(false, Resource.Notification.ConsultationPackageDescriptionTooLong, null);
                case ConsultationPackageRules.Problem.UnknownDuration:
                    return new BaseResultDto<T>(false, Resource.Notification.ConsultationPackageDurationInvalid, null);
                case ConsultationPackageRules.Problem.InvalidPrice:
                case ConsultationPackageRules.Problem.ActiveWithoutPrice:
                    return new BaseResultDto<T>(false, Resource.Notification.AmountNotCorrect, null);
                default:
                    return new BaseResultDto<T>(false, Resource.Notification.InvalidData, null);
            }

            var companionExists = await ConsultationOwners.ExistsAsync(_context, owner);
            if (!companionExists)
                return new BaseResultDto<T>(false, Resource.Notification.NothingFound, null);

            if (dto.PictureId is > 0)
            {
                var pictureExists = await _context.Pictures.AsNoTracking().AnyAsync(s => s.Id == dto.PictureId.Value);
                if (!pictureExists)
                    return new BaseResultDto<T>(false, Resource.Notification.NothingFound, null);
            }

            // نام تکراری زیر همان کانال (بدون حساسیت به حروف/فاصله) - ویرایش خودِ پکیج تکراری حساب نمی‌شود
            var key = ConsultationPackageRules.NameKey(dto.Name);
            var sameChannelNames = await _context.ConsultationPackages.AsNoTracking()
                .ForOwner(owner).Where(s => !s.Deleted && s.ChannelId == dto.ChannelId && (!isUpdate || s.Id != dto.Id))
                .Select(s => s.Name)
                .ToListAsync();
            if (sameChannelNames.Any(name => ConsultationPackageRules.NameKey(name) == key))
                return new BaseResultDto<T>(false, Resource.Notification.ConsultationPackageDuplicateName, null);

            return null;
        }

        private IQueryable<ConsultationPackageItemDto> ProjectItems(IQueryable<ConsultationPackage> query) =>
            query.Select(s => new ConsultationPackageItemDto
            {
                Id = s.Id,
                ChannelId = s.ChannelId,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                Active = s.Active,
                Approved = s.Approved,
                Bookable = s.Bookable,
                Name = s.Name,
                Description = s.Description,
                PictureId = s.PictureId,
                SortOrder = s.SortOrder,
                Picture = s.Picture == null ? null : new PictureVDto
                {
                    Id = s.Picture.Id,
                    Url = s.Picture.Url,
                    OrginalName = s.Picture.OrginalName,
                    GuidName = s.Picture.GuidName,
                    Extension = s.Picture.Extension
                }
            });

        private async Task<List<ConsultationPackageItemDto>> LoadListAsync(ConsultationOwner owner) =>
            await ProjectItems(_context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner).Where(s => !s.Deleted)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder).ThenBy(s => s.DurationMinutes).ThenBy(s => s.Id))
                .ToListAsync();

        private async Task<ConsultationPackageItemDto> LoadOneAsync(ConsultationOwner owner, long id) =>
            await ProjectItems(_context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner).Where(s => s.Id == id))
                .FirstOrDefaultAsync();
    }
}
