using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv
{
    public class ConsultationPackageService : IConsultationPackageService
    {
        private readonly IDataBaseContext _context;

        public ConsultationPackageService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetListAsync(long companionId)
        {
            try
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(true, await LoadListAsync(companionId));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> CreateAsync(long companionId, ConsultationPackageItemDto dto)
        {
            try
            {
                var invalid = await ValidateAsync<ConsultationPackageItemDto>(companionId, dto, isUpdate: false);
                if (invalid != null)
                    return invalid;

                var now = DateTime.Now;
                var nextOrder = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.CompanionId == companionId && !s.Deleted && s.ChannelId == dto.ChannelId)
                    .Select(s => (int?)s.SortOrder)
                    .MaxAsync() ?? 0;

                var row = new ConsultationPackage
                {
                    CompanionId = companionId,
                    ChannelId = dto.ChannelId,
                    DurationMinutes = dto.DurationMinutes,
                    Price = dto.Price,
                    Active = dto.Active,
                    Name = ConsultationPackageRules.NormalizeName(dto.Name),
                    Description = ConsultationPackageRules.NormalizeDescription(dto.Description),
                    PictureId = dto.PictureId is > 0 ? dto.PictureId : null,
                    SortOrder = dto.SortOrder > 0 ? dto.SortOrder : nextOrder + 1,
                    CreateDate = now
                };
                await _context.ConsultationPackages.AddAsync(row);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ConsultationPackageItemDto>(true, await LoadOneAsync(companionId, row.Id));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPackageItemDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> UpdateAsync(long companionId, ConsultationPackageItemDto dto)
        {
            try
            {
                if (dto == null || dto.Id <= 0)
                    return new BaseResultDto<ConsultationPackageItemDto>(false, Resource.Notification.InvalidData, null);

                var invalid = await ValidateAsync<ConsultationPackageItemDto>(companionId, dto, isUpdate: true);
                if (invalid != null)
                    return invalid;

                var row = await _context.ConsultationPackages.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.Id && s.CompanionId == companionId && !s.Deleted);
                if (row == null)
                    return new BaseResultDto<ConsultationPackageItemDto>(false, Resource.Notification.NothingFound, null);

                row.ChannelId = dto.ChannelId;
                row.DurationMinutes = dto.DurationMinutes;
                row.Price = dto.Price;
                row.Active = dto.Active;
                row.Name = ConsultationPackageRules.NormalizeName(dto.Name);
                row.Description = ConsultationPackageRules.NormalizeDescription(dto.Description);
                row.PictureId = dto.PictureId is > 0 ? dto.PictureId : null;
                if (dto.SortOrder > 0)
                    row.SortOrder = dto.SortOrder;
                row.UpdateDate = DateTime.Now;
                // کانتکست پروژه پیش‌فرض NoTracking است؛ Update صریح لازم است
                _context.ConsultationPackages.Update(row);
                await _context.SaveChangesAsync();

                return new BaseResultDto<ConsultationPackageItemDto>(true, await LoadOneAsync(companionId, row.Id));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPackageItemDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> DeleteAsync(long companionId, long id)
        {
            try
            {
                var affected = await _context.ConsultationPackages
                    .Where(s => s.Id == id && s.CompanionId == companionId && !s.Deleted)
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

        public async Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(long companionId)
        {
            try
            {
                var companionOpen = await _context.Companions.AsNoTracking()
                    .AnyAsync(s => s.Id == companionId && !s.Deleted && s.Active && s.Approved);
                if (!companionOpen)
                    return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, new List<ConsultationPackagePublicVDto>());

                var list = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.CompanionId == companionId && !s.Deleted && s.Active && s.Price > 0)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder).ThenBy(s => s.DurationMinutes).ThenBy(s => s.Id)
                    .Select(s => new ConsultationPackagePublicVDto
                    {
                        Id = s.Id,
                        CompanionId = s.CompanionId,
                        ChannelId = s.ChannelId,
                        DurationMinutes = s.DurationMinutes,
                        Price = s.Price,
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

                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        // null یعنی معتبر؛ غیر از آن پاسخ خطای آماده‌ی برگشتی
        private async Task<BaseResultDto<T>> ValidateAsync<T>(long companionId, ConsultationPackageItemDto dto, bool isUpdate) where T : class
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

            var companionExists = await _context.Companions.AsNoTracking().AnyAsync(s => s.Id == companionId && !s.Deleted);
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
                .Where(s => s.CompanionId == companionId && !s.Deleted && s.ChannelId == dto.ChannelId && (!isUpdate || s.Id != dto.Id))
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

        private async Task<List<ConsultationPackageItemDto>> LoadListAsync(long companionId) =>
            await ProjectItems(_context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.CompanionId == companionId && !s.Deleted)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder).ThenBy(s => s.DurationMinutes).ThenBy(s => s.Id))
                .ToListAsync();

        private async Task<ConsultationPackageItemDto> LoadOneAsync(long companionId, long id) =>
            await ProjectItems(_context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.Id == id && s.CompanionId == companionId))
                .FirstOrDefaultAsync();
    }
}
