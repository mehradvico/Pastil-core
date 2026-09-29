using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Dto;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv
{
    public class SchoolPictureService : CommonSrv<SchoolPicture, SchoolPictureDto>, ISchoolPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public SchoolPictureService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<SchoolPictureVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolPictures.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<SchoolPictureVDto>(true, mapper.Map<SchoolPictureVDto>(item));
            return new BaseResultDto<SchoolPictureVDto>(false, mapper.Map<SchoolPictureVDto>(item));
        }

        public SchoolPictureSearchDto Search(SchoolPictureInputDto searchDto)
        {
            var model = _context.SchoolPictures.Include(s => s.Picture).AsQueryable().Where(s => !s.Deleted);
            if (searchDto.SchoolId.HasValue)
            {
                model = model.Where(s => s.SchoolId.Equals(searchDto.SchoolId));
            }
            if (searchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.School.CompanionId == searchDto.CompanionId.Value);
            }
            return new SchoolPictureSearchDto(searchDto, model, mapper);
        }
    }
}
