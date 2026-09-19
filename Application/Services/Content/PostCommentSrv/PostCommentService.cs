using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Content.PostCommentSrv.Dto;
using Application.Services.Content.PostCommentSrv.Iface;
using Application.Services.Content.PostSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.GallerySrv
{
    public class PostCommentService : CommonSrv<PostComment, PostCommentDto>, IPostCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly IPostService _postService;
        public PostCommentService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, IPostService postService) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            this._postService = postService;
        }

        public override async Task<BaseResultDto<PostCommentDto>> InsertAsyncDto(PostCommentDto dto)
        {
            try
            {
                dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<PostCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {

                    if (dto.UserId == null && string.IsNullOrEmpty(dto.Name))
                    {
                        return new BaseResultDto<PostCommentDto>(false, val1: Resource.Notification.PleaseEnterTheName, val2: nameof(dto.Name), data: dto);

                    }
                    if (dto.Rate.HasValue && (dto.Rate.Value > 5 || dto.Rate.Value < 1))
                    {
                        return new BaseResultDto<PostCommentDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    var item = mapper.Map<PostComment>(dto);
                    var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                    if (commentStatus != null)
                    {
                        item.StatusId = commentStatus.Id;
                        item.CreateDate = DateTime.Now;
                        item.Answer = null;
                        await _context.PostComments.AddAsync(item);
                        _context.SaveChanges();
                        return new BaseResultDto<PostCommentDto>(true, mapper.Map<PostCommentDto>(item));
                    }
                    return new BaseResultDto<PostCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
                }
            }
            catch
            {
                return new BaseResultDto<PostCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public override BaseResultDto UpdateDto(PostCommentDto dto)
        {
            try
            {
                var item = _context.PostComments.FirstOrDefault(s => s.Id.Equals(dto.Id));
                item.StatusId = dto.StatusId;
                item.Answer = dto.Answer;
                _context.PostComments.Update(item);
                _context.SaveChanges();
                _postService.UpdatePostCommentCount(item.PostId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        public PostCommentSearchDto Search(PostCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new PostCommentSearchDto(baseSearchDto, model, mapper);
        }
        private IQueryable<PostComment> BaseSaerch(PostCommentInputDto searchDto)
        {
            var query = _context.PostComments.Include(s => s.Post).AsQueryable();
            if (searchDto.PostId.HasValue)
                query = query.Where(s => s.PostId == searchDto.PostId).Include(s => s.User);
            if (searchDto.AllStatus == false)
            {

                switch (searchDto.SortBy)
                {

                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.StatusId).ThenBy(s => s.Id);
                            break;
                        }

                    default:
                        query = query.OrderBy(s => s.StatusId).ThenByDescending(s => s.Id);

                        break;
                }
            }


            return query;
        }

    }
}
