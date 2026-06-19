using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.ClubRewardSrv.Dto;
using Application.Services.Accounting.ClubRewardSrv.Iface;
using Application.Services.Accounting.UserSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv
{
    public class ClubRewardService : CommonSrv<ClubReward, ClubRewardDto>, IClubRewardService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public ClubRewardService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<ClubRewardVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ClubRewards.Include(s => s.Rebate).ThenInclude(s => s.Type).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ClubRewardVDto>(true, mapper.Map<ClubRewardVDto>(item));
            }
            return new BaseResultDto<ClubRewardVDto>(false, mapper.Map<ClubRewardVDto>(item));
        }

        public ClubRewardSearchDto Search(ClubRewardInputDto baseSearchDto)
        {
            var model = _context.ClubRewards.Include(s => s.Rebate).ThenInclude(s => s.Type).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.RebateTypeId.HasValue)
            {
                model = model.Where(s => s.Rebate.TypeId == baseSearchDto.RebateTypeId);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new ClubRewardSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ClubRewardDto>> InsertAsyncDto(ClubRewardDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<ClubRewardDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var rebate = await _context.Rebate.FirstOrDefaultAsync(s => s.Id == dto.RebateId && !s.Deleted);
                    if (rebate == null)
                    {
                        return new BaseResultDto<ClubRewardDto>(isSuccess: false, val: Resource.Notification.NothingFound, data: dto);
                    }
                    var item = mapper.Map<ClubReward>(dto);
  
                    await _context.ClubRewards.AddAsync(item);
                    await _context.SaveChangesAsync();

                    rebate.ClubRewardId = item.Id;
                    _context.Rebate.Update(rebate);
                    await _context.SaveChangesAsync();

                    return new BaseResultDto<ClubRewardDto>(true, mapper.Map<ClubRewardDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ClubRewardDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
