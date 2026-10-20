using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv
{
    public class ProviderRoleAssignmentService : CommonSrv<ProviderRoleAssignment, ProviderRoleAssignmentDto>, IProviderRoleAssignmentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public ProviderRoleAssignmentService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<ProviderRoleAssignmentDto>> FindAsyncDto(long id)
        {
            var item = await _context.ProviderRoleAssignments.Include(x => x.ProviderRole).FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            return new BaseResultDto<ProviderRoleAssignmentDto>(item != null, mapper.Map<ProviderRoleAssignmentDto>(item));
        }

        public ProviderRoleAssignmentSearchDto Search(ProviderRoleAssignmentInputDto input)
        {
            var model = _context.ProviderRoleAssignments.Include(x => x.ProviderRole).Where(x => !x.Deleted);
            if (input.ProviderId.HasValue)
                model = model.Where(x => x.ProviderId == input.ProviderId.Value);
            if (input.ProviderRoleId.HasValue)
                model = model.Where(x => x.ProviderRoleId == input.ProviderRoleId.Value);
            if (input.Approved.HasValue)
                model = model.Where(x => x.Approved == input.Approved.Value);
            model = model.OrderBy(x => x.ProviderRoleId);
            return new ProviderRoleAssignmentSearchDto(input, model, mapper);
        }

        public override async Task<BaseResultDto<ProviderRoleAssignmentDto>> InsertAsyncDto(ProviderRoleAssignmentDto dto)
        {
            if (!await _context.Providers.AnyAsync(x => x.Id == dto.ProviderId && !x.Deleted))
                return new BaseResultDto<ProviderRoleAssignmentDto>(false, Resource.Notification.ProviderNotFound, dto);
            if (!await _context.ProviderRoles.AnyAsync(x => x.Id == dto.ProviderRoleId && !x.Deleted && x.Active))
                return new BaseResultDto<ProviderRoleAssignmentDto>(false, Resource.Notification.ProviderRoleNotFound, dto);
            if (await _context.ProviderRoleAssignments.AnyAsync(x => x.ProviderId == dto.ProviderId && x.ProviderRoleId == dto.ProviderRoleId && !x.Deleted))
                return new BaseResultDto<ProviderRoleAssignmentDto>(false, Resource.Notification.DuplicateValue, dto);
            return await base.InsertAsyncDto(dto);
        }

        /// <summary>فقط وضعیت تأیید قابل ویرایش است؛ ProviderId و ProviderRoleId بعد از ثبت ثابت می‌مانند.</summary>
        public override BaseResultDto UpdateDto(ProviderRoleAssignmentDto dto)
        {
            var item = _context.ProviderRoleAssignments.FirstOrDefault(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);
            item.Approved = dto.Approved;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }
    }
}
