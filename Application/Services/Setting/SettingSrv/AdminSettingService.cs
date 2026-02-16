using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Setting.SettingSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System.Linq;

namespace Application.Services.Setting.SettingSrv
{
    public class AdminSettingService : CommonSrv<AdminSetting, AdminSettingDto>, IAdminSettingService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public AdminSettingService(IDataBaseContext _context, IMapper mapper) : base(_context: _context, mapper: mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public AdminSetting GetByLabel(string label)
        {
            return _context.AdminSettings.FirstOrDefault(s => s.Label == label);
        }
        public BaseSearchDto<AdminSettingDto> Search(BaseInputDto baseSearchDto)
        {
            var model = _context.AdminSettings.AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Label.Contains(baseSearchDto.Q) || s.Name.Contains(baseSearchDto.Q)).OrderByDescending(o => o.Id);
            }
            return new BaseSearchDto<AdminSetting, AdminSettingDto>(baseSearchDto, model, mapper);
        }


    }
}
