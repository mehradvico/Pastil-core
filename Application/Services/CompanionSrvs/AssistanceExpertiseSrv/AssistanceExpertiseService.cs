using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Iface;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceExpertiseSrv
{
    // تخصص‌های مرتبط با هر «خدمت» (مثلاً زایمان). فقط برای پیشنهاد همکار مناسب هنگام تخصیص رزرو استفاده می‌شود.
    public class AssistanceExpertiseService : IAssistanceExpertiseService
    {
        private const int MaxExpertises = 20;
        private readonly IDataBaseContext _context;

        public AssistanceExpertiseService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<AssistanceExpertiseVDto>> GetAsync(long assistanceId)
        {
            var exists = await _context.Assistances.AsNoTracking()
                .AnyAsync(item => item.Id == assistanceId && !item.Deleted);
            if (!exists)
                return new BaseResultDto<AssistanceExpertiseVDto>(false, Resource.Notification.NothingFound, null);

            return new BaseResultDto<AssistanceExpertiseVDto>(true, await BuildAsync(assistanceId));
        }

        public async Task<BaseResultDto<AssistanceExpertiseVDto>> SaveAsync(AssistanceExpertiseDto dto)
        {
            if (dto == null || dto.AssistanceId <= 0)
                return new BaseResultDto<AssistanceExpertiseVDto>(false, Resource.Notification.InvalidData, null);

            var exists = await _context.Assistances.AsNoTracking()
                .AnyAsync(item => item.Id == dto.AssistanceId && !item.Deleted);
            if (!exists)
                return new BaseResultDto<AssistanceExpertiseVDto>(false, Resource.Notification.NothingFound, null);

            var ids = (dto.ExpertiseIds ?? new List<long>()).Where(id => id > 0).Distinct().ToList();
            if (ids.Count > MaxExpertises)
                return new BaseResultDto<AssistanceExpertiseVDto>(false, Resource.Notification.InvalidData, null);

            var validCount = await _context.Expertises.AsNoTracking()
                .CountAsync(item => ids.Contains(item.Id) && !item.Deleted && item.Active);
            if (validCount != ids.Count)
                return new BaseResultDto<AssistanceExpertiseVDto>(false, Resource.Notification.NothingFound, null);

            var current = await _context.AssistanceExpertises
                .AsTracking()
                .Where(row => row.AssistanceId == dto.AssistanceId)
                .ToListAsync();

            _context.AssistanceExpertises.RemoveRange(current.Where(row => !ids.Contains(row.ExpertiseId)));
            var currentIds = current.Select(row => row.ExpertiseId).ToHashSet();
            foreach (var expertiseId in ids.Where(id => !currentIds.Contains(id)))
            {
                await _context.AssistanceExpertises.AddAsync(new AssistanceExpertise
                {
                    AssistanceId = dto.AssistanceId,
                    ExpertiseId = expertiseId
                });
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto<AssistanceExpertiseVDto>(true, await BuildAsync(dto.AssistanceId));
        }

        private async Task<AssistanceExpertiseVDto> BuildAsync(long assistanceId)
        {
            var rows = await _context.AssistanceExpertises.AsNoTracking()
                .Where(row => row.AssistanceId == assistanceId && !row.Expertise.Deleted)
                .OrderBy(row => row.Expertise.Priority)
                .ThenBy(row => row.Expertise.Name)
                .Select(row => new AssistanceExpertiseItemVDto { Id = row.ExpertiseId, Name = row.Expertise.Name })
                .ToListAsync();

            return new AssistanceExpertiseVDto
            {
                AssistanceId = assistanceId,
                ExpertiseIds = rows.Select(item => item.Id).ToList(),
                Expertises = rows
            };
        }
    }
}
