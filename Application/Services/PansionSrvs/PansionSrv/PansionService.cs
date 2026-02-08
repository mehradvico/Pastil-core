using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
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

namespace Application.Services.PansionSrvs.PansionSrv
{
    public class PansionService : CommonSrv<Pansion, PansionDto>, IPansionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PansionService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<PansionVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).Include(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.PansionPets).ThenInclude(s => s.Pet).Include(s => s.PansionComments).Include(s => s.PansionPictures).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<PansionVDto>(true, mapper.Map<PansionVDto>(item));
            }
            return new BaseResultDto<PansionVDto>(false, mapper.Map<PansionVDto>(item));
        }

        public PansionSearchDto Search(PansionInputDto baseSearchDto)
        {
            var model = _context.Pansions.Include(s => s.Picture).Include(s => s.Companion).ThenInclude(s => s.Neighborhood).Include(s => s.City).ThenInclude(s => s.State).Include(s => s.PansionComments)
                .Include(s => s.PansionPictures).ThenInclude(s => s.Picture).AsQueryable();

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.IsSchool.HasValue)
            {
                var isSchool = baseSearchDto.IsSchool.Value;
                model = model.Where(s => s.IsSchool == null || s.IsSchool == isSchool);
            }
            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
            }
            if (baseSearchDto.Approve.HasValue)
            {
                model = model.Where(s => s.Approve == baseSearchDto.Approve.Value);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.StateId == baseSearchDto.StateId.Value);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.CityId == baseSearchDto.CityId.Value);
            }
            if (baseSearchDto.NeighborhoodIds != null && baseSearchDto.NeighborhoodIds.Any())
            {
                model = model.Where(s => s.Companion.NeighborhoodId != null &&
                    baseSearchDto.NeighborhoodIds.Contains(s.Companion.Neighborhood.Id)
                );
            }

            if (baseSearchDto.Suggested.HasValue)
            {
                model = model.Where(s => s.Suggested == baseSearchDto.Suggested.Value);
            }
            if (baseSearchDto.PetId.HasValue)
            {
                model = model.Where(s => s.PansionPets.Any(s => s.PetId == baseSearchDto.PetId.Value));
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
                case Common.Enumerable.SortEnum.MoreVisit:
                    {
                        model = model.OrderByDescending(s => s.RateAvg);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessVisit:
                    {
                        model = model.OrderBy(s => s.RateAvg);
                        break;
                    }
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        model = model.OrderByDescending(s =>
                            s.IsSchool == true
                                ? s.SchoolPrice
                                : s.PansionPrice
                        );
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        model = model.OrderBy(s =>
                            s.IsSchool == true
                                ? s.SchoolPrice
                                : s.PansionPrice
                        );
                        break;
                    }
                default:
                    break;
            }
            return new PansionSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PansionDto>> InsertAsyncDto(PansionDto dto)
        {
            try
            {
                var modelCheker = InsertCheckerAsync(dto);
                if (!modelCheker.IsSuccess)
                {
                    return new BaseResultDto<PansionDto>(false, dto);
                }
                else
                {
                    var item = mapper.Map<Pansion>(dto);
                    var companionId = dto.CompanionId;                  
                    var model = await _context.Companions.Include(s => s.Pansions).FirstOrDefaultAsync(s => s.Id == companionId && !s.Deleted);
                    if (model == null)
                    {
                        return new BaseResultDto<PansionDto>(false, Resource.Notification.NothingFound, dto);
                    }
                    await _context.Pansions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    var pansionId = item.Id;
                    await _context.SaveChangesAsync();
                    //await _notificationService.InsertNoticeAsync(item.Id, NoticeTypeEnum.NotifType_AddCompanion, NoticeUserTypeEnum.NoticeUserType_Admin);
                    return new BaseResultDto<PansionDto>(true, mapper.Map<PansionDto>(item));

                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PansionDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public BaseResultDto InsertCheckerAsync(PansionDto dto)
        {
            var errors = new List<Tuple<string, string>>();

            if (string.IsNullOrEmpty(dto.Name))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheName, nameof(dto.Name)));
            }
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            return new BaseResultDto(true);
        }
        public BaseResultDto UpdatePansionActiveDto(PansionActiveDto dto)
        {
            var item = _context.Pansions.FirstOrDefault(s => s.Id == dto.Id);
            item.Active = dto.Active;
            _context.Pansions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public BaseResultDto UpdatePansionApproveDto(PansionApproveDto dto)
        {
            var item = _context.Pansions.FirstOrDefault(s => s.Id == dto.Id);
            item.Approve = dto.Approve;
            _context.Pansions.Update(item);
            _context.SaveChanges();
            return new BaseResultDto(isSuccess: true);
        }

        public void UpdatePansionCommentCount(long pansionId)
        {
            var item = _context.Pansions.Include(s => s.PansionComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == pansionId);
            item.CommentCount = item.PansionComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Pansions.Update(item);
            _context.SaveChanges();
        }
    }
}
