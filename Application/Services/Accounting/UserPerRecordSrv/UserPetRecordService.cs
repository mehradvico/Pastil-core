using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Iface;
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

namespace Application.Services.Accounting.UserPerRecordSrv
{
    public class UserPetRecordService : CommonSrv<UserPetRecord, UserPetRecordDto>, IUserPetRecordService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public UserPetRecordService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<UserPetRecordVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.UserPetRecords.Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Operator).Include(s => s.UserPet).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<UserPetRecordVDto>(true, mapper.Map<UserPetRecordVDto>(item));
            return new BaseResultDto<UserPetRecordVDto>(false, mapper.Map<UserPetRecordVDto>(item));
        }

        public override async Task<BaseResultDto<UserPetRecordDto>> FindAsyncDto(long id)
        {
            var item = await _context.UserPetRecords.Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Operator).Include(s => s.UserPet).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<UserPetRecordDto>(true, mapper.Map<UserPetRecordDto>(item));
            return new BaseResultDto<UserPetRecordDto>(false, mapper.Map<UserPetRecordDto>(item));
        }

        public UserPetRecordSearchDto Search(UserPetRecordInputDto baseSearchDto)
        {
            var model = _context.UserPetRecords.Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.Operator).Include(s => s.UserPet).ThenInclude(s => s.Picture).AsQueryable();
            if (baseSearchDto.UserPetId.HasValue)
            {
                model = model.Where(s => s.UserPetId == baseSearchDto.UserPetId);
            }
            if (baseSearchDto.OperatorId.HasValue)
            {
                model = model.Where(s => s.OperatorId == baseSearchDto.OperatorId.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserPet.UserId == baseSearchDto.UserId.Value);
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
                case Common.Enumerable.SortEnum.Name:
                    {
                        model = model.OrderByDescending(s => s.Name);
                        break;
                    }
                default:
                    break;
            }
            return new UserPetRecordSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<bool> CanOperateOnPetAsync(long userPetId, long operatorUserId)
        {
            return await _context.CompanionReserves.AsNoTracking().AnyAsync(r =>
                r.IsReserved && !r.IsCancel &&
                r.UserPets.Any(p => p.Id == userPetId) &&
                ((r.CompanionAssistanceUser != null &&
                  r.CompanionAssistanceUser.Active && !r.CompanionAssistanceUser.Deleted &&
                  r.CompanionAssistanceUser.UserId == operatorUserId) ||
                 r.CompanionAssistance.Companion.OwnerId == operatorUserId));
        }

        public override async Task<BaseResultDto<UserPetRecordDto>> InsertAsyncDto(UserPetRecordDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserPetRecordDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<UserPetRecord>(dto);
                    item.CreateDate = DateTime.Now;
                    await _context.UserPetRecords.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<UserPetRecordDto>(true, mapper.Map<UserPetRecordDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<UserPetRecordDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
    }
}
