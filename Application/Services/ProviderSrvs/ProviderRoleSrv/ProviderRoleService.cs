using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Dto;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv
{
    public class ProviderRoleService : CommonSrv<ProviderRole, ProviderRoleDto>, IProviderRoleService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public ProviderRoleService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<ProviderRoleDto>> FindAsyncDto(long id)
        {
            var item = await _context.ProviderRoles.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
            return new BaseResultDto<ProviderRoleDto>(item != null, mapper.Map<ProviderRoleDto>(item));
        }

        public ProviderRoleSearchDto Search(ProviderRoleInputDto input)
        {
            var model = _context.ProviderRoles.Where(x => !x.Deleted);
            if (!string.IsNullOrWhiteSpace(input.Q))
                model = model.Where(x => x.Name.Contains(input.Q) || x.Label.Contains(input.Q));
            if (input.Active.HasValue)
                model = model.Where(x => x.Active == input.Active.Value);
            model = input.SortBy == SortEnum.Old ? model.OrderByDescending(x => x.Id) : model.OrderBy(x => x.Id);
            return new ProviderRoleSearchDto(input, model, mapper);
        }

        public override async Task<BaseResultDto<ProviderRoleDto>> InsertAsyncDto(ProviderRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Label))
                return new BaseResultDto<ProviderRoleDto>(false, Resource.Notification.Unsuccess, dto);
            dto.Label = dto.Label.Trim();
            if (await _context.ProviderRoles.AnyAsync(x => (x.Label == dto.Label || x.Name == dto.Name) && !x.Deleted))
                return new BaseResultDto<ProviderRoleDto>(false, Resource.Notification.DuplicateValue, dto);
            return await base.InsertAsyncDto(dto);
        }

        /// <summary>فقط Name و Active قابل ویرایش‌اند؛ Label کلید کد است و ثابت می‌ماند.</summary>
        public override BaseResultDto UpdateDto(ProviderRoleDto dto)
        {
            var item = _context.ProviderRoles.FirstOrDefault(x => x.Id == dto.Id && !x.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.ProviderRoleNotFound);
            if (_context.ProviderRoles.Any(x => x.Name == dto.Name && x.Id != dto.Id && !x.Deleted))
                return new BaseResultDto<ProviderRoleDto>(false, Resource.Notification.DuplicateValue, dto);
            item.Name = dto.Name;
            item.Active = dto.Active;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }

        /// <summary>نقشی که به ارائه‌دهنده‌ای داده شده حذف نمی‌شود (به‌جایش غیرفعال کنید).</summary>
        public override BaseResultDto DeleteDto(long id)
        {
            if (_context.ProviderRoleAssignments.Any(x => x.ProviderRoleId == id && !x.Deleted) ||
                _context.Expertises.Any(x => x.ProviderRoleId == id && !x.Deleted))
                return new BaseResultDto(false, Resource.Notification.ProviderRoleInUse);
            return base.DeleteDto(id);
        }
    }
}
