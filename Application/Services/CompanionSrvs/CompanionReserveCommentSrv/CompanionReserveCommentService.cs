using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv.Iface;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveCommentSrv
{
    public class CompanionReserveCommentService : CommonSrv<CompanionReserveComment, CompanionReserveCommentDto>, ICompanionReserveCommentService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService codeService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly ICompanionReserveCommentRateService _commentRate;
        private readonly string connectionString;


        public CompanionReserveCommentService(IDataBaseContext _context, IConfiguration config, IMapper mapper, ICodeService codeService, ICurrentUserHelper currentUserHelper, ICompanionReserveCommentRateService commentRate) : base(_context, mapper)
        {
            this.codeService = codeService;
            this._context = _context;
            this.mapper = mapper;
            _currentUserHelper = currentUserHelper;
            _commentRate = commentRate;
            this.connectionString = config.GetValue<string>("connection");
        }

        public async Task<BaseResultDto<CompanionReserveCommentVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionReserveComments.Include(s => s.CompanionReserve).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionReserve).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Assistance)
                .Include(s => s.User).Include(s => s.CompanionReserveCommentRates).ThenInclude(s => s.AssistanceQuestionnaire).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<CompanionReserveCommentVDto>(true, mapper.Map<CompanionReserveCommentVDto>(item));
            return new BaseResultDto<CompanionReserveCommentVDto>(false, mapper.Map<CompanionReserveCommentVDto>(item));
        }

        public override async Task<BaseResultDto<CompanionReserveCommentDto>> InsertAsyncDto(CompanionReserveCommentDto dto)
        {
            try
            {
                dto.Text = await SanitizeTextHelper.ToSanitizeAsync(dto.Text);
                var modelCheker = ModelHelper<CompanionReserveCommentDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                bool existed = await _context.CompanionReserveComments.AnyAsync(x => x.CompanionReserveId == dto.CompanionReserveId);
                if (existed)
                {
                    return new BaseResultDto<CompanionReserveCommentDto>(false, val: Resource.Notification.DuplicateValue, data: dto);
                }

                if (!dto.CompanionReserveCommentRates.Any())
                {
                    return new BaseResultDto<CompanionReserveCommentDto>(false, val: Resource.Notification.PleaseAnswerTheQuestions, data: dto);
                }

                var item = mapper.Map<CompanionReserveComment>(dto);
                var commentStatus = await codeService.GetByLabelAsync(CommentEnum.Comment_NotChecked.ToString());
                if (commentStatus != null)
                {
                    item.Rate = (int)(dto.CompanionReserveCommentRates.Sum(s => s.Rate) / (float)dto.CompanionReserveCommentRates.Count());
                    item.StatusId = commentStatus.Id;
                    item.CreateDate = DateTime.Now;
                    item.Answer = null;
                    item.UserId = _currentUserHelper.CurrentUser.UserId;

                    await _context.CompanionReserveComments.AddAsync(item);
                    await _context.SaveChangesAsync();

                    foreach (var rateDto in dto.CompanionReserveCommentRates)
                    {
                        var rate = new CompanionReserveCommentRateDto
                        {
                            Rate = rateDto.Rate,
                            AssistanceQuestionnaireId = rateDto.AssistanceQuestionnaireId,
                            CompanionReserveCommentId = item.Id
                        };
                        await _commentRate.InsertAsyncDto(rate);
                    }
                    return new BaseResultDto<CompanionReserveCommentDto>(true, mapper.Map<CompanionReserveCommentDto>(item));
                }

                return new BaseResultDto<CompanionReserveCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
            catch
            {
                return new BaseResultDto<CompanionReserveCommentDto>(false, val: Resource.Notification.Unsuccess, data: dto);
            }
        }

        public async Task<BaseResultDto> UpdateDtoAsync(CompanionReserveCommentDto dto)
        {

            try
            {

                var item = _context.CompanionReserveComments.Include(s => s.CompanionReserve).ThenInclude(s => s.CompanionAssistance).FirstOrDefault(s => s.Id == dto.Id);
                item.Answer = dto.Answer;
                item.StatusId = dto.StatusId;
                _context.CompanionReserveComments.Update(item);
                await _context.SaveChangesAsync();
                await UpdateCompanionCommentRateAsync(item.CompanionReserve.CompanionAssistance.CompanionId);
                return new BaseResultDto(isSuccess: true);

            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public CompanionReserveCommentSearchDto Search(CompanionReserveCommentInputDto baseSearchDto)
        {
            var model = BaseSaerch(baseSearchDto);
            return new CompanionReserveCommentSearchDto(baseSearchDto, model, mapper);
        }
        private IQueryable<CompanionReserveComment> BaseSaerch(CompanionReserveCommentInputDto searchDto)
        {
            var query = _context.CompanionReserveComments.Include(s => s.CompanionReserve).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Companion)
                .Include(s => s.CompanionReserve).ThenInclude(s => s.CompanionAssistance).ThenInclude(s => s.Assistance)
                .Include(s => s.User).AsQueryable();
            if (searchDto.CompanionReserveId.HasValue)
                query = query.Where(s => s.CompanionReserveId == searchDto.CompanionReserveId);
            if (searchDto.UserId.HasValue)
                query = query.Where(s => s.UserId == searchDto.UserId);
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

        public async Task UpdateCompanionCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateCompanionCommentsRate", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}