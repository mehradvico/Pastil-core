using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Iface;
using AutoMapper;
using Entities.Entities.CompanionField;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.ExpertiseSrv
{
    public class ExpertiseService : CommonSrv<Expertise, ExpertiseDto>, IExpertiseService
    {
        private readonly IDataBaseContext context;
        private readonly IMapper mapper;

        public ExpertiseService(IDataBaseContext context, IMapper mapper)
            : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<ExpertiseVDto>> FindAsyncVDto(long id)
        {
            var item = await context.Expertises
                .AsNoTracking()
                .Include(x => x.ProviderRole)
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return new BaseResultDto<ExpertiseVDto>(
                item != null,
                mapper.Map<ExpertiseVDto>(item));
        }

        public ExpertiseSearchDto Search(ExpertiseInputDto dto)
        {
            IQueryable<Expertise> query = context.Expertises
                .AsNoTracking()
                .Include(x => x.ProviderRole)
                .Where(x => !x.Deleted);

            if (dto.ProviderRoleId == 0)
                query = query.Where(x => x.ProviderRoleId == null);
            else if (dto.ProviderRoleId.HasValue)
                query = query.Where(x => x.ProviderRoleId == dto.ProviderRoleId.Value);

            if (dto.Available.HasValue)
                query = query.Where(x => x.Active == dto.Available.Value);

            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                query = query.Where(x => x.Name.Contains(q));
            }

            query = dto.SortBy switch
            {
                SortEnum.Old => query.OrderBy(x => x.Id),
                SortEnum.Name => query.OrderBy(x => x.Name),
                SortEnum.MorePriority => query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Id),
                SortEnum.LessPriority => query.OrderBy(x => x.Priority).ThenByDescending(x => x.Id),
                _ => query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Id)
            };

            return new ExpertiseSearchDto(dto, query, mapper);
        }

        public Task<List<ExpertiseGroupDto>> GroupsAsync() => context.ProviderRoles
            .AsNoTracking()
            .Where(x => !x.Deleted && x.Active &&
                (x.Label == ProviderRoleLabels.Groomer || x.Label == ProviderRoleLabels.Clinic ||
                 x.Label == ProviderRoleLabels.Coach || x.Label == ProviderRoleLabels.Pansion ||
                 x.Label == ProviderRoleLabels.School))
            .OrderBy(x => x.Id)
            .Select(x => new ExpertiseGroupDto { Id = x.Id, Name = x.Name, Label = x.Label })
            .ToListAsync();

        public async Task<BaseResultDto<ExpertiseDto>> InsertValidatedAsync(ExpertiseDto dto)
        {
            dto.Name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto<ExpertiseDto>(false, Resource.Notification.CompanionExpertiseNameRequired, nameof(dto.Name), dto);

            if (!await ValidGroupAsync(dto.ProviderRoleId))
                return new BaseResultDto<ExpertiseDto>(false, Resource.Notification.CompanionExpertiseDataInvalid, nameof(dto.ProviderRoleId), dto);

            if (await NameExistsAsync(dto.Name, dto.ProviderRoleId, null))
                return new BaseResultDto<ExpertiseDto>(false, Resource.Notification.CompanionExpertiseNameAlreadyRegistered, nameof(dto.Name), dto);

            return await InsertAsyncDto(dto);
        }

        public async Task<BaseResultDto> UpdateValidatedAsync(ExpertiseDto dto)
        {
            dto.Name = dto.Name?.Trim();
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto(false, Resource.Notification.CompanionExpertiseDataInvalid);

            if (!await ValidGroupAsync(dto.ProviderRoleId))
                return new BaseResultDto(false, Resource.Notification.CompanionExpertiseDataInvalid, nameof(dto.ProviderRoleId));

            if (await NameExistsAsync(dto.Name, dto.ProviderRoleId, dto.Id))
                return new BaseResultDto(false, Resource.Notification.CompanionExpertiseNameAlreadyRegistered, nameof(dto.Name));

            return UpdateDto(dto);
        }

        public async Task<BaseResultDto> DeleteValidatedAsync(long id)
        {
            var isUsed = await context.CompanionUsers
                .AsNoTracking()
                .AnyAsync(x => x.ExpertiseId == id && !x.Deleted);

            if (isUsed)
                return new BaseResultDto(false, Resource.Notification.CompanionExpertiseInUseCannotDelete);

            return DeleteDto(id);
        }

        private Task<bool> ValidGroupAsync(long? id) =>
            id.HasValue && id.Value > 0
                ? context.ProviderRoles.AsNoTracking().AnyAsync(x => x.Id == id.Value && !x.Deleted && x.Active &&
                    (x.Label == ProviderRoleLabels.Groomer || x.Label == ProviderRoleLabels.Clinic ||
                     x.Label == ProviderRoleLabels.Coach || x.Label == ProviderRoleLabels.Pansion ||
                     x.Label == ProviderRoleLabels.School))
                : Task.FromResult(false);

        private Task<bool> NameExistsAsync(string name, long? providerRoleId, long? exceptId) =>
            context.Expertises
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.Deleted &&
                    x.Name == name &&
                    x.ProviderRoleId == providerRoleId &&
                    (!exceptId.HasValue || x.Id != exceptId.Value));
    }
}
