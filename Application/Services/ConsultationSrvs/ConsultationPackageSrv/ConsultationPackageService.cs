using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
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

        public async Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetMatrixAsync(long companionId)
        {
            try
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(true, await LoadMatrixAsync(companionId));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationPackageItemDto>>> SaveMatrixAsync(long companionId, ConsultationPackageSaveDto dto)
        {
            try
            {
                switch (ConsultationPackageRules.Validate(dto?.Items))
                {
                    case ConsultationPackageRules.Problem.None:
                        break;
                    case ConsultationPackageRules.Problem.InvalidPrice:
                    case ConsultationPackageRules.Problem.ActiveWithoutPrice:
                        return new BaseResultDto<List<ConsultationPackageItemDto>>(false, Resource.Notification.AmountNotCorrect, null);
                    default:
                        return new BaseResultDto<List<ConsultationPackageItemDto>>(false, Resource.Notification.InvalidData, null);
                }

                var companionExists = await _context.Companions.AnyAsync(s => s.Id == companionId && !s.Deleted);
                if (!companionExists)
                    return new BaseResultDto<List<ConsultationPackageItemDto>>(false, Resource.Notification.NothingFound, null);

                var existing = await _context.ConsultationPackages
                    .Where(s => s.CompanionId == companionId && !s.Deleted)
                    .ToListAsync();

                var now = DateTime.Now;
                foreach (var item in dto.Items)
                {
                    var row = existing.FirstOrDefault(s => s.ChannelId == item.ChannelId && s.DurationMinutes == item.DurationMinutes);
                    if (row == null)
                    {
                        // ردیفی که هم‌چنان قیمت ۰ و غیرفعال است ذخیره نمی‌شود؛ ماتریس همیشه با مقدار پیش‌فرض کامل می‌شود
                        if (item.Price <= 0 && !item.Active)
                            continue;

                        await _context.ConsultationPackages.AddAsync(new ConsultationPackage
                        {
                            CompanionId = companionId,
                            ChannelId = item.ChannelId,
                            DurationMinutes = item.DurationMinutes,
                            Price = item.Price,
                            Active = item.Active,
                            CreateDate = now
                        });
                    }
                    else
                    {
                        row.Price = item.Price;
                        row.Active = item.Active;
                        row.UpdateDate = now;
                        // کانتکست پروژه پیش‌فرض NoTracking است؛ Update صریح لازم است
                        _context.ConsultationPackages.Update(row);
                    }
                }

                await _context.SaveChangesAsync();
                return new BaseResultDto<List<ConsultationPackageItemDto>>(true, await LoadMatrixAsync(companionId));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageItemDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
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
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.DurationMinutes)
                    .Select(s => new ConsultationPackagePublicVDto
                    {
                        Id = s.Id,
                        CompanionId = s.CompanionId,
                        ChannelId = s.ChannelId,
                        DurationMinutes = s.DurationMinutes,
                        Price = s.Price
                    })
                    .ToListAsync();

                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        private async Task<List<ConsultationPackageItemDto>> LoadMatrixAsync(long companionId)
        {
            var stored = await _context.ConsultationPackages.AsNoTracking()
                .Where(s => s.CompanionId == companionId && !s.Deleted)
                .Select(s => new ConsultationPackageItemDto
                {
                    Id = s.Id,
                    ChannelId = s.ChannelId,
                    DurationMinutes = s.DurationMinutes,
                    Price = s.Price,
                    Active = s.Active
                })
                .ToListAsync();

            return ConsultationPackageRules.BuildMatrix(stored);
        }
    }
}
