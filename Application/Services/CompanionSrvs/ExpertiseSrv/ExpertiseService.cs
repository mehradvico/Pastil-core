using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Iface;
using AutoMapper;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
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
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return new BaseResultDto<ExpertiseVDto>(
                item != null,
                mapper.Map<ExpertiseVDto>(item));
        }

        public ExpertiseSearchDto Search(ExpertiseInputDto dto)
        {
            var query = context.Expertises
                .AsNoTracking()
                .Where(x => !x.Deleted);

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

        public async Task<BaseResultDto<ExpertiseDto>> InsertValidatedAsync(ExpertiseDto dto)
        {
            dto.Name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto<ExpertiseDto>(false, "عنوان تخصص الزامی است.", nameof(dto.Name), dto);

            if (await NameExistsAsync(dto.Name, null))
                return new BaseResultDto<ExpertiseDto>(false, "این عنوان تخصص قبلاً ثبت شده است.", nameof(dto.Name), dto);

            return await InsertAsyncDto(dto);
        }

        public async Task<BaseResultDto> UpdateValidatedAsync(ExpertiseDto dto)
        {
            dto.Name = dto.Name?.Trim();
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto(false, "اطلاعات تخصص معتبر نیست.");

            if (await NameExistsAsync(dto.Name, dto.Id))
                return new BaseResultDto(false, "این عنوان تخصص قبلاً ثبت شده است.", nameof(dto.Name));

            return UpdateDto(dto);
        }

        public async Task<BaseResultDto> DeleteValidatedAsync(long id)
        {
            var isUsed = await context.CompanionUsers
                .AsNoTracking()
                .AnyAsync(x => x.ExpertiseId == id && !x.Deleted);

            if (isUsed)
                return new BaseResultDto(false, "این تخصص به کاربر نمایندگی متصل است و قابل حذف نیست؛ آن را غیرفعال کنید.");

            return DeleteDto(id);
        }

        private Task<bool> NameExistsAsync(string name, long? exceptId) =>
            context.Expertises
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.Deleted &&
                    x.Name == name &&
                    (!exceptId.HasValue || x.Id != exceptId.Value));
    }
}
