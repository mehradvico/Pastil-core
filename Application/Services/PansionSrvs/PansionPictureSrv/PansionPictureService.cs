using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.PansionSrvs.PansionPictureSrv.Dto;
using Application.Services.PansionSrvs.PansionPictureSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPictureSrv
{
    public class PansionPictureService : CommonSrv<PansionPicture, PansionPictureDto>, IPansionPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PansionPictureService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<PansionPictureVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PansionPictures.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<PansionPictureVDto>(true, mapper.Map<PansionPictureVDto>(item));
            return new BaseResultDto<PansionPictureVDto>(false, mapper.Map<PansionPictureVDto>(item));
        }

        public PansionPictureSearchDto Search(PansionPictureInputDto searchDto)
        {
            var model = _context.PansionPictures.Include(s => s.Picture).AsQueryable().Where(s => !s.Deleted);
            if (searchDto.PansionId.HasValue)
            {
                model = model.Where(s => s.PansionId.Equals(searchDto.PansionId));
            }
            if (searchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.Pansion.CompanionId == searchDto.CompanionId.Value);
            }
            return new PansionPictureSearchDto(searchDto, model, mapper);
        }
    }
}
