using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionAnswerSrv.Iface;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionAnswerSrv
{
    public class DiscussionAnswerService : CommonSrv<DiscussionAnswer, DiscussionAnswerDto>, IDiscussionAnswerService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IDiscussionQuestionService _topicService;
        public DiscussionAnswerService(IDataBaseContext _context, IMapper mapper, IDiscussionQuestionService topicService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._topicService = topicService;
        }
        public async Task<BaseResultDto<DiscussionAnswerVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.DiscussionAnswers.Include(s => s.User).Include(s => s.DiscussionQuestion).FirstOrDefaultAsync(s => s.Id == id && s.Deleted != true);
            if (item != null)
            {
                return new BaseResultDto<DiscussionAnswerVDto>(true, mapper.Map<DiscussionAnswerVDto>(item));
            }
            return new BaseResultDto<DiscussionAnswerVDto>(false, mapper.Map<DiscussionAnswerVDto>(item));
        }
        public DiscussionAnswerSearchDto Search(DiscussionAnswerInputDto searchDto)
        {
            var query = _context.DiscussionAnswers.Include(s => s.User).Where(s => !s.Deleted).IgnoreQueryFilters().AsQueryable();

            if (searchDto.Available.HasValue)
            {
                query = query.Where(s => s.Active == searchDto.Available.Value);
            }

            if (searchDto.DiscussionQuestionId.HasValue)
            {
                query = query.Where(s => s.DiscussionQuestionId == searchDto.DiscussionQuestionId.Value);
            }

            if (searchDto.UserId.HasValue)
            {
                query = query.Include(s => s.DiscussionAnswerLikes.Where(a => a.UserId == searchDto.UserId.Value)).Where(s => s.UserId == searchDto.UserId.Value);
            }

            switch (searchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Old:
                    query = query.OrderBy(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.New:
                default:
                    query = query.OrderByDescending(s => s.Id);
                    break;
            }

            return new DiscussionAnswerSearchDto(searchDto, query, mapper);
        }
        public override async Task<BaseResultDto<DiscussionAnswerDto>> InsertAsyncDto(DiscussionAnswerDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<DiscussionAnswerDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<DiscussionAnswer>(dto);
                    item.CreateDate = DateTime.Now;
                    item.Active = false;
                    await _context.DiscussionAnswers.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<DiscussionAnswerDto>(true, mapper.Map<DiscussionAnswerDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<DiscussionAnswerDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public BaseResultDto DiscussionAnswerActivation(DiscussionAnswerActiveDto dto)
        {
            try
            {
                var item = _context.DiscussionAnswers.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
                }

                item.Active = dto.Active;
                _context.SaveChanges();

                var updateCountResult = _topicService.UpdateAnswerCountDto(new DiscussionQuestionDto
                {
                    Id = item.DiscussionQuestionId
                });

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
    }
}
