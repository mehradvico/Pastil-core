using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Filing.PictureSrv
{
    public class PictureService : IPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public PictureService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }


        public async Task<BaseResultDto> FindVDtoAsync(long id)
        {
            var item = await _context.Pictures.FindAsync(id);
            if (item != null)
            {
                return new BaseResultDto<PictureVDto>(true, mapper.Map<PictureVDto>(item));
            }
            return new BaseResultDto(false);
        }

        public List<Picture> GetAll()
        {
            var items = _context.Pictures.ToList();

            return items;
        }

        public virtual async Task<BaseResultDto> InsertAsyncDto(PictureDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<PictureDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<Picture>(dto);
                    await _context.Pictures.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<PictureVDto>(true, mapper.Map<PictureVDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }
        public BaseSearchDto<PictureVDto> Search(BaseInputDto baseSearchDto)
        {
            var query = _context.Pictures.AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.OrginalName.Contains(baseSearchDto.Q));
            }
            switch (baseSearchDto.SortBy)
            {

                case Common.Enumerable.SortEnum.Default:
                    {
                        query = query.OrderByDescending(s => s.Id);

                        break;
                    }
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        query = query.OrderBy(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Name:
                    {
                        query = query.OrderByDescending(s => s.Name);
                        break;
                    }


                default:
                    break;
            }
            return new BaseSearchDto<Picture, PictureVDto>(baseSearchDto, query, mapper);
        }
    }
}
