using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.SubscriptionPlanSrv
{
    public class SubscriptionPlanService : CommonSrv<SubscriptionPlan, SubscriptionPlanDto>, ISubscriptionPlanService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public SubscriptionPlanService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<SubscriptionPlanDto>> FindAsyncDto(long id)
        {
            var item = await _context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            return new BaseResultDto<SubscriptionPlanDto>(item != null, mapper.Map<SubscriptionPlanDto>(item));
        }

        public SubscriptionPlanSearchDto Search(SubscriptionPlanInputDto input)
        {
            var model = _context.SubscriptionPlans.Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(input.Q))
                model = model.Where(x => x.Name.Contains(input.Q));
            if (input.Active.HasValue)
                model = model.Where(x => x.Active == input.Active.Value);
            model = input.SortBy == SortEnum.Old ? model.OrderBy(x => x.Level) : model.OrderByDescending(x => x.Level);
            return new SubscriptionPlanSearchDto(input, model, mapper);
        }

        public override async Task<BaseResultDto<SubscriptionPlanDto>> InsertAsyncDto(SubscriptionPlanDto dto)
        {
            if (await _context.SubscriptionPlans.AnyAsync(x => x.Name == dto.Name && !x.Deleted))
                return new BaseResultDto<SubscriptionPlanDto>(false, Resource.Notification.DuplicateValue, dto);
            return await base.InsertAsyncDto(dto);
        }

        public override BaseResultDto UpdateDto(SubscriptionPlanDto dto)
        {
            if (_context.SubscriptionPlans.Any(x => x.Name == dto.Name && x.Id != dto.Id && !x.Deleted))
                return new BaseResultDto<SubscriptionPlanDto>(false, Resource.Notification.DuplicateValue, dto);
            return base.UpdateDto(dto);
        }
    }
}
