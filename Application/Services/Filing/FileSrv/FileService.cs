using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Filing.FileSrv
{
    public class FileService : IFileService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public FileService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public virtual async Task<BaseResultDto> InsertAsyncDto(FileDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<FileDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<File>(dto);
                    await _context.Files.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<FileVDto>(isSuccess: true, mapper.Map<FileVDto>(item));
                }
            }
            catch (Exception ex)
            {

                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }

        }
        public async Task<BaseResultDto> FindVDtoAsync(long id)
        {
            var item = await _context.Files.FindAsync(id);
            if (item != null)
            {
                return new BaseResultDto<FileVDto>(true, mapper.Map<FileVDto>(item));
            }
            return new BaseResultDto(false);
        }
        public BaseSearchDto<FileVDto> Search(BaseInputDto baseInputDto)
        {
            var query = _context.Files.AsQueryable();
            if (!string.IsNullOrEmpty(baseInputDto.Q))
            {
                query = query.Where(s => s.OrginalName.Contains(baseInputDto.Q));
            }
            switch (baseInputDto.SortBy)
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
            return new BaseSearchDto<File, FileVDto>(baseInputDto, query, mapper);
        }
    }
}
