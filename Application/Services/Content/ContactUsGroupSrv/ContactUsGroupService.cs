using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Content.ContactUsGroupSrv.Dto;
using Application.Services.Content.ContactUsGroupSrv.Iface;
using Application.Services.Dto;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Content.ContactUsGroupSrv
{
    public class ContactUsGroupService : CommonSrv<ContactUsGroup, ContactUsGroupDto>, IContactUsGroupService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto _currentUserDto;
        public ContactUsGroupService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUserHelper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUserDto = currentUserHelper.CurrentUser;
        }

        public BaseSearchDto<ContactUsGroupDto> Search(BaseInputDto searchDto)
        {
            var query = _context.ContactUsGroups.AsQueryable();

            if (searchDto.Available == true)
            {
                query = query.Where(s => s.Active);
            }

            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(searchDto.Q));
            }
            switch (searchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    query = query.OrderByDescending(s => s.Id);
                    break;
                case Common.Enumerable.SortEnum.Old:
                    query = query.OrderBy(s => s.Id);
                    break;
                case Common.Enumerable.SortEnum.Name:
                    query = query.OrderBy(s => s.Name);
                    break;
                case Common.Enumerable.SortEnum.MorePriority:
                    query = query.OrderByDescending(s => s.Priority);
                    break;
                default:
                    query = query.OrderBy(s => s.Priority);
                    break;
            }
            var result = new BaseSearchDto<ContactUsGroup, ContactUsGroupDto>(searchDto, query, mapper);
            SetFormFields(result.List);
            return result;
        }
        public BaseResultDto GetForRole()
        {
            var model = _context.ContactUsGroups.Where(s => s.Active).AsQueryable();

            if (_currentUserDto.RoleEnum != RoleEnum.Admin.ToString())
            {
                model = model.Where(s => s.Roles != null && s.Roles.Contains(_currentUserDto.RoleEnum));
            }
            model = model.OrderBy(s => s.Priority);
            var items = mapper.Map<List<ContactUsGroupDto>>(model);
            SetFormFields(items);
            return new BaseResultDto<List<ContactUsGroupDto>>(true, items);
        }

        private static void SetFormFields(List<ContactUsGroupDto> items)
        {
            foreach (var item in items)
            {
                item.FormFields = ContactUsGroupFormSchema.GetFields(item.Label);
            }
        }

    }
}
