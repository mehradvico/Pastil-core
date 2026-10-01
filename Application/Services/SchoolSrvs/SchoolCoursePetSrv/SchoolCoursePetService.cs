using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCoursePetSrv
{
    public class SchoolCoursePetService : CommonSrv<SchoolCoursePet, SchoolCoursePetDto>, ISchoolCoursePetService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public SchoolCoursePetService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<SchoolCoursePetVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.SchoolCoursePets.Include(s => s.Pet).Include(s => s.PetBreed).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<SchoolCoursePetVDto>(true, mapper.Map<SchoolCoursePetVDto>(item));
            return new BaseResultDto<SchoolCoursePetVDto>(false, mapper.Map<SchoolCoursePetVDto>(item));
        }

        public SchoolCoursePetSearchDto Search(SchoolCoursePetInputDto baseSearchDto)
        {
            var model = _context.SchoolCoursePets.Include(s => s.Pet).Include(s => s.PetBreed).Include(s => s.SchoolCourse).ThenInclude(s => s.School)
                .AsQueryable().Where(s => !s.Deleted && !s.SchoolCourse.School.Deleted);

            if (baseSearchDto.SchoolCourseId.HasValue)
                model = model.Where(s => s.SchoolCourseId == baseSearchDto.SchoolCourseId.Value);
            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(s => s.SchoolCourse.School.CompanionId == baseSearchDto.CompanionId.Value);

            return new SchoolCoursePetSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<SchoolCoursePetDto>> InsertAsyncDto(SchoolCoursePetDto dto)
        {
            try
            {
                bool existed = await _context.SchoolCoursePets.AnyAsync(x =>
                    x.SchoolCourseId == dto.SchoolCourseId && x.PetId == dto.PetId && x.PetBreedId == dto.PetBreedId && !x.Deleted);
                if (existed)
                    return new BaseResultDto<SchoolCoursePetDto>(false, Resource.Notification.DuplicateValue, dto);

                var item = mapper.Map<SchoolCoursePet>(dto);
                await _context.SchoolCoursePets.AddAsync(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto<SchoolCoursePetDto>(true, mapper.Map<SchoolCoursePetDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<SchoolCoursePetDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
    }
}
