using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionCommentSrv
{
    public class PansionCommentService : CommonSrv<PansionComment, PansionCommentDto>, IPansionCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly string connectionString;
        private readonly INoticeService _noticeService;


        public PansionCommentService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICodeService codeService, INoticeService noticeService) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            this.connectionString = config.GetValue<string>(
            "connection");
            this._noticeService = noticeService;
        }

        public override async Task<BaseResultDto<PansionCommentDto>> InsertAsyncDto(PansionCommentDto dto)
        {
            try
            {
                dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<PansionCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.PansionReserveId <= 0)
                    {
                        return new BaseResultDto<PansionCommentDto>(
                            false,
                            Resource.Notification.PansionCommentReserveIdRequired,
                            dto);
                    }

                    if (dto.Rate.HasValue && (dto.Rate.Value > 5 || dto.Rate.Value < 1))
                    {
                        return new BaseResultDto<PansionCommentDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }

                    var reserve = await _context.PansionReserves
                        .AsNoTracking()
                        .FirstOrDefaultAsync(reserve =>
                            reserve.Id == dto.PansionReserveId &&
                            reserve.BookerId == dto.UserId &&
                            reserve.IsReserved &&
                            !reserve.IsCancel &&
                            reserve.StatusId == (long)PansionReserveStatusEnum.PansionReserveState_Complete);
                    if (reserve == null)
                    {
                        return new BaseResultDto<PansionCommentDto>(
                            false,
                            Resource.Notification.PansionCommentOnlyForYourCompletedReserve,
                            dto);
                    }

                    if (reserve.PansionId != dto.PansionId)
                    {
                        return new BaseResultDto<PansionCommentDto>(false, Resource.Notification.InvalidData, dto);
                    }

                    if (await _context.PansionComments.AnyAsync(comment =>
                            comment.PansionReserveId == dto.PansionReserveId))
                    {
                        return new BaseResultDto<PansionCommentDto>(false, Resource.Notification.DuplicateValue, dto);
                    }

                    var item = mapper.Map<PansionComment>(dto);
                    item.PansionId = reserve.PansionId;
                    item.PansionReserveId = reserve.Id;
                    item.IsReserved = true;
                    var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                    if (commentStatus != null)
                    {
                        item.StatusId = commentStatus.Id;
                        item.CreateDate = DateTime.Now;
                        item.Answer = null;
                        await _context.PansionComments.AddAsync(item);
                        await _context.SaveChangesAsync();
                        await _noticeService.CreateAsync(new NoticeCreateDto
                        {
                            Label = NoticeTypeLabels.PansionCommentSubmitted,
                            ActorUserId = item.UserId,
                            ReferenceType = "PansionComment",
                            ReferenceId = item.Id,
                            DeduplicationKey = $"{NoticeTypeLabels.PansionCommentSubmitted}:{item.Id}",
                            Metadata = new Dictionary<string, string>
                            {
                                { "pansionId", item.PansionId.ToString() }
                            }
                        });
                        return new BaseResultDto<PansionCommentDto>(true, mapper.Map<PansionCommentDto>(item));
                    }
                    return new BaseResultDto<PansionCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
                }
            }
            catch
            {
                return new BaseResultDto<PansionCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }
        public async Task<BaseResultDto> UpdateDtoAsync(PansionCommentDto dto)
        {

            try
            {

                var item = _context.PansionComments.Find(dto.Id);
                item.Answer = dto.Answer;
                item.StatusId = dto.StatusId;
                _context.PansionComments.Update(item);
                await _context.SaveChangesAsync();
                await UpdatePansionCommentRateAsync(item.PansionId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public PansionCommentSearchDto Search(PansionCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new PansionCommentSearchDto(baseSearchDto, model, mapper);
        }

        private IQueryable<PansionComment> BaseSaerch(PansionCommentInputDto searchDto)
        {
            var query = _context.PansionComments.Include(s => s.Pansion).Include(s => s.User).AsQueryable();
            if (searchDto.PansionId.HasValue)
                query = query.Where(s => s.PansionId == searchDto.PansionId);
            if (searchDto.UserId.HasValue)
                query = query.Include(s => s.CommentLikes.Where(a => a.UserId == searchDto.UserId)).Where(s => s.UserId == searchDto.UserId);
            if (searchDto.AllStatus == false)
            {
                switch (searchDto.Available)
                {
                    case null:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_NotChecked.ToString());
                            break;
                        }
                    case true:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_Accept.ToString());
                            break;
                        }
                    case false:
                        {
                            query = query.Where(s => s.Status.Label == CommentEnum.Comment_Reject.ToString());
                            break;
                        }
                    default:
                }
            }

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

            return query;
        }

        public async Task UpdatePansionCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdatePansionComments", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
