using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Iface;
using Entities.Entities.PastilClubField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointSrv
{
    public class ClubPointRuleService : IClubPointRuleService
    {
        private readonly IDataBaseContext _context;

        public ClubPointRuleService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<ClubPointRuleDto>> FindAsync(long id, CancellationToken cancellationToken = default)
        {
            var item = await _context.ClubPointRules.AsNoTracking()
                .FirstOrDefaultAsync(rule => rule.Id == id, cancellationToken);
            return item == null
                ? new BaseResultDto<ClubPointRuleDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<ClubPointRuleDto>(true, Map(item));
        }

        public async Task<ClubPointRuleSearchDto> SearchAsync(ClubPointRuleInputDto dto, CancellationToken cancellationToken = default)
        {
            dto.PageIndex = Math.Max(1, dto.PageIndex);
            dto.PageSize = Math.Clamp(dto.PageSize, 1, 100);
            var query = _context.ClubPointRules.AsNoTracking().AsQueryable();
            if (dto.EventType.HasValue)
                query = query.Where(item => item.EventType == dto.EventType.Value);
            if (dto.Available.HasValue)
                query = query.Where(item => item.Active == dto.Available.Value);
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var text = dto.Q.Trim();
                query = query.Where(item => item.Name.Contains(text) || item.Description.Contains(text));
            }
            query = dto.SortBy == SortEnum.Old
                ? query.OrderBy(item => item.Id)
                : query.OrderByDescending(item => item.Id);

            return new ClubPointRuleSearchDto(dto)
            {
                TotalCount = await query.CountAsync(cancellationToken),
                List = await query.Skip((dto.PageIndex - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .Select(item => Map(item))
                    .ToListAsync(cancellationToken)
            };
        }

        public async Task<BaseResultDto<ClubPointRuleDto>> InsertAsync(ClubPointRuleDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await ValidateAsync(dto, null, cancellationToken);
            if (validation != null)
                return validation;
            var item = new ClubPointRule { CreateDate = DateTime.UtcNow };
            Apply(item, dto);
            await _context.ClubPointRules.AddAsync(item, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto<ClubPointRuleDto>(true, Map(item));
        }

        public async Task<BaseResultDto<ClubPointRuleDto>> UpdateAsync(ClubPointRuleDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await ValidateAsync(dto, dto.Id, cancellationToken);
            if (validation != null)
                return validation;
            var item = await _context.ClubPointRules.AsTracking()
                .FirstOrDefaultAsync(rule => rule.Id == dto.Id, cancellationToken);
            if (item == null)
                return new BaseResultDto<ClubPointRuleDto>(false, Resource.Notification.NothingFound, null);
            Apply(item, dto);
            item.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto<ClubPointRuleDto>(true, Map(item));
        }

        private async Task<BaseResultDto<ClubPointRuleDto>> ValidateAsync(ClubPointRuleDto dto, long? currentId, CancellationToken cancellationToken)
        {
            if (dto.PointAmount <= 0 ||
                !Enum.IsDefined(dto.EventType) ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                return new BaseResultDto<ClubPointRuleDto>(false, Resource.Notification.InvalidData, dto);
            if (await _context.ClubPointRules.AsNoTracking().AnyAsync(
                    item => item.EventType == dto.EventType && item.Id != currentId,
                    cancellationToken))
                return new BaseResultDto<ClubPointRuleDto>(false, "برای این رویداد قبلاً قانون امتیاز تعریف شده است.", dto);
            return null;
        }

        private static void Apply(ClubPointRule item, ClubPointRuleDto dto)
        {
            item.Name = dto.Name.Trim();
            item.EventType = dto.EventType;
            item.PointAmount = dto.PointAmount;
            item.DailyLimit = dto.DailyLimit;
            item.MonthlyLimit = dto.MonthlyLimit;
            item.LifetimeLimit = dto.LifetimeLimit;
            item.Active = dto.Active;
            item.StartDate = dto.StartDate;
            item.EndDate = dto.EndDate;
            item.Description = dto.Description?.Trim();
        }

        private static ClubPointRuleDto Map(ClubPointRule item)
        {
            return new ClubPointRuleDto
            {
                Id = item.Id,
                Name = item.Name,
                EventType = item.EventType,
                PointAmount = item.PointAmount,
                DailyLimit = item.DailyLimit,
                MonthlyLimit = item.MonthlyLimit,
                LifetimeLimit = item.LifetimeLimit,
                Active = item.Active,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Description = item.Description
            };
        }
    }
}
