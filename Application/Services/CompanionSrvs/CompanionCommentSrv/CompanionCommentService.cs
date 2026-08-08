using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionCommentSrv
{
    public class CompanionCommentService : CommonSrv<CompanionComment, CompanionCommentDto>, ICompanionCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly ICompanionService _CompanionService;
        private readonly INoticeService _noticeService;
        public CompanionCommentService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, ICompanionService CompanionService, INoticeService noticeService) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            this._CompanionService = CompanionService;
            this._noticeService = noticeService;
        }

        public override async Task<BaseResultDto<CompanionCommentDto>> InsertAsyncDto(CompanionCommentDto dto)
        {
            try
            {
                dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<CompanionCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (string.IsNullOrEmpty(dto.Name))
                    {
                        return new BaseResultDto<CompanionCommentDto>(false, val1: Resource.Notification.PleaseEnterTheName, val2: nameof(dto.Name), data: dto);
                    }

                    if (dto.Rate.HasValue && (dto.Rate.Value > 5 || dto.Rate.Value < 1))
                    {
                        return new BaseResultDto<CompanionCommentDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }
                    var item = mapper.Map<CompanionComment>(dto);
                    var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                    if (commentStatus != null)
                    {
                        item.StatusId = commentStatus.Id;
                        item.CreateDate = DateTime.Now;
                        item.Answer = null;
                        await _context.CompanionComments.AddAsync(item);
                        await _context.SaveChangesAsync();
                        await _noticeService.CreateAsync(new NoticeCreateDto
                        {
                            Label = NoticeTypeLabels.CompanionCommentSubmitted,
                            ActorUserId = item.UserId,
                            ReferenceType = "CompanionComment",
                            ReferenceId = item.Id,
                            DeduplicationKey = $"{NoticeTypeLabels.CompanionCommentSubmitted}:{item.Id}",
                            Metadata = new Dictionary<string, string>
                            {
                                { "companionId", item.CompanionId.ToString() },
                                { "commentName", dto.Name }
                            }
                        });
                        return new BaseResultDto<CompanionCommentDto>(true, mapper.Map<CompanionCommentDto>(item));
                    }
                    return new BaseResultDto<CompanionCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
                }
            }
            catch
            {
                return new BaseResultDto<CompanionCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public override BaseResultDto UpdateDto(CompanionCommentDto dto)
        {
            try
            {
                var item = _context.CompanionComments.FirstOrDefault(s => s.Id.Equals(dto.Id));
                item.StatusId = dto.StatusId;
                item.Answer = dto.Answer;
                _context.CompanionComments.Update(item);
                _context.SaveChanges();
                _CompanionService.UpdateCompanionCommentRateAsync(item.CompanionId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }

        public CompanionCommentSearchDto Search(CompanionCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new CompanionCommentSearchDto(baseSearchDto, model, mapper);
        }
        private IQueryable<CompanionComment> BaseSaerch(CompanionCommentInputDto searchDto)
        {
            var query = _context.CompanionComments.Include(s => s.Companion).Include(s => s.User).Include(s => s.Status).AsQueryable();

            if (searchDto.CompanionId.HasValue)
            {
                query = query.Where(s => s.CompanionId == searchDto.CompanionId.Value);
            }

            if (searchDto.AllStatus != true)
            {
                switch (searchDto.SortBy)
                {
                    case SortEnum.Old:
                        query = query
                            .OrderBy(s => s.StatusId)
                            .ThenBy(s => s.Id);
                        break;

                    case SortEnum.Default:
                    case SortEnum.New:
                    default:
                        query = query
                            .OrderBy(s => s.StatusId)
                            .ThenByDescending(s => s.Id);
                        break;
                }
            }
            else
            {
                switch (searchDto.SortBy)
                {
                    case SortEnum.Old:
                        query = query.OrderBy(s => s.Id);
                        break;

                    case SortEnum.Default:
                    case SortEnum.New:
                    default:
                        query = query.OrderByDescending(s => s.Id);
                        break;
                }
            }

            return query;
        }
    }
}
