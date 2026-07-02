using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionQuestionSrv
{
    public class DiscussionQuestionService : CommonSrv<DiscussionQuestion, DiscussionQuestionDto>, IDiscussionQuestionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;

        public DiscussionQuestionService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUser;
        }
        public async Task<BaseResultDto<DiscussionQuestionVDto>> FindAsyncVDto(long id, bool visit = true)
        {
            var item = await _context.DiscussionQuestions.Include(s => s.DiscussionAnswers.Where(s => s.Active)).ThenInclude(s => s.User).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<DiscussionQuestionVDto>(true, mapper.Map<DiscussionQuestionVDto>(item));
            }
            return new BaseResultDto<DiscussionQuestionVDto>(false, mapper.Map<DiscussionQuestionVDto>(item));
        }

        public async Task<BaseResultDto<DiscussionQuestionVDto>>FindAdminAsyncVDto(long id)
        {
            var item = await _context.DiscussionQuestions.Include(s => s.User).Include(s => s.Product).Include(s => s.DiscussionAnswers.Where(answer => !answer.Deleted))
                .ThenInclude(answer => answer.User).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (item != null)
            {
                return new BaseResultDto<DiscussionQuestionVDto>(true, mapper.Map<DiscussionQuestionVDto>(item));
            }
            return new BaseResultDto<DiscussionQuestionVDto>(false, mapper.Map<DiscussionQuestionVDto>(item));
        }

        public DiscussionQuestionSearchDto Search(DiscussionQuestionInputDto baseSearchDto)
        {
            var query = _context.DiscussionQuestions.Include(s => s.DiscussionAnswers.Where(s => s.Active)).ThenInclude(s => s.User).Include(s => s.User).Include(s => s.Product).Where(s => !s.Deleted).AsQueryable();
            DateTime now = DateTime.Now;

            if (baseSearchDto.Available == true)
            {
                query = query.Where(s => s.Active && s.AdminConfirm == true);
            }
            if (baseSearchDto.ProductId.HasValue)
            {
                query = query.Where(s => s.ProductId == baseSearchDto.ProductId.Value);
            }
            if (baseSearchDto.AdminConfirm.HasValue)
            {
                query = query.Where(s => s.AdminConfirm == baseSearchDto.AdminConfirm.Value);
            }
            if (baseSearchDto.FromDate.HasValue)
            {
                query = query.Where(s => s.CreateDate.Date >= baseSearchDto.FromDate.Value.Date);
            }
            if (baseSearchDto.ToDate.HasValue)
            {
                query = query.Where(s => s.CreateDate.Date <= baseSearchDto.ToDate.Value.Date);
            }
            if (baseSearchDto.Active.HasValue)
            {
                query = query.Where(s => s.Active == baseSearchDto.Active.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Old:
                    query = query.OrderBy(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.New:
                default:
                    query = query.OrderByDescending(s => s.Id);
                    break;
            }
            return new DiscussionQuestionSearchDto(baseSearchDto, query, mapper);
        }

        public override async Task<BaseResultDto<DiscussionQuestionDto>> InsertAsyncDto(DiscussionQuestionDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<DiscussionQuestionDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<DiscussionQuestion>(dto);
                    item.CreateDate = DateTime.Now;
                    await _context.DiscussionQuestions.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<DiscussionQuestionDto>(true, mapper.Map<DiscussionQuestionDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<DiscussionQuestionDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public override BaseResultDto UpdateDto(DiscussionQuestionDto dto)
        {
            try
            {
                var item = _context.DiscussionQuestions.FirstOrDefault(s => s.Id == dto.Id &&!s.Deleted);

                if (item == null)
                {
                    return new BaseResultDto(isSuccess: false,val: Resource.Notification.NothingFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.Content))
                {
                    item.Content = dto.Content.Trim();
                }

                if (dto.ProductId > 0)
                {
                    item.ProductId = dto.ProductId;
                }

                if (dto.UserId > 0)
                {
                    item.UserId = dto.UserId;
                }

                item.AdminConfirm = dto.AdminConfirm;
                item.Active = dto.Active;

                _context.DiscussionQuestions.Update(item);
                _context.SaveChanges();

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false,val: ex.Message);
            }
        }

        public BaseResultDto UpdateAnswerCountDto(DiscussionQuestionDto dto)
        {
            try
            {
                var question = _context.DiscussionQuestions.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);

                if (question == null)
                {
                    return new BaseResultDto( isSuccess: false, val: Resource.Notification.NothingFound);
                }

                question.AnswerCount = _context.DiscussionAnswers.Count(s => s.DiscussionQuestionId == dto.Id && s.Active &&!s.Deleted);
                _context.SaveChanges();

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false,val: ex.Message);
            }
        }
    }
}
