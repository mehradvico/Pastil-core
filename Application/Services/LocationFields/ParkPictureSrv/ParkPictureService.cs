using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.ParkPictureSrv.Dto;
using Application.Services.LocationFields.ParkPictureSrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkPictureSrv
{
    public class ParkPictureService : CommonSrv<ParkPicture, ParkPictureDto>, IParkPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public ParkPictureService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<ParkPictureVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ParkPictures.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<ParkPictureVDto>(true, mapper.Map<ParkPictureVDto>(item));
            return new BaseResultDto<ParkPictureVDto>(false, mapper.Map<ParkPictureVDto>(item));
        }

        public ParkPictureSearchDto Search(ParkPictureInputDto searchDto)
        {
            var model = _context.ParkPictures.Include(s => s.Picture).AsQueryable().Where(s => !s.Deleted);
            if (searchDto.ParkId.HasValue)
            {
                model = model.Where(s => s.ParkId.Equals(searchDto.ParkId));
            }
            return new ParkPictureSearchDto(searchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ParkPictureDto>> InsertAsyncDto(ParkPictureDto dto)
        {
            try
            {
                if (!await _context.Parks.AnyAsync(s => s.Id == dto.ParkId) ||
                    !await _context.Pictures.AnyAsync(s => s.Id == dto.PictureId))
                {
                    return new BaseResultDto<ParkPictureDto>(false, Resource.Notification.NothingFound, dto);
                }

                var existing = await _context.ParkPictures
                    .FirstOrDefaultAsync(s => s.ParkId == dto.ParkId && s.PictureId == dto.PictureId);

                if (existing != null)
                {
                    existing.Deleted = false;
                    existing.Label = dto.Label;
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<ParkPictureDto>(true, mapper.Map<ParkPictureDto>(existing));
                }

                var item = mapper.Map<ParkPicture>(dto);
                item.Deleted = false;
                await _context.ParkPictures.AddAsync(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto<ParkPictureDto>(true, mapper.Map<ParkPictureDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ParkPictureDto>(false, ex.Message, dto);
            }
        }
    }
}
