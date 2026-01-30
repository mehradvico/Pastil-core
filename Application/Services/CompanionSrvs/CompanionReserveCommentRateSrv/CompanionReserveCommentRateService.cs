using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveCommentRateSrv
{
    public class CompanionReserveCommentRateService : CommonSrv<CompanionReserveCommentRate, CompanionReserveCommentRateDto>, ICompanionReserveCommentRateService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly string connectionString;

        public CompanionReserveCommentRateService(IDataBaseContext _context, IConfiguration config, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.connectionString = config.GetValue<string>(
         "connection");
        }

        public async Task<BaseResultDto<CompanionReserveCommentRateVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionReserveCommentRates.Include(s => s.AssistanceQuestionnaire).Include(s => s.CompanionReserveCommentId).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionReserveCommentRateVDto>(true, mapper.Map<CompanionReserveCommentRateVDto>(item));
            }
            return new BaseResultDto<CompanionReserveCommentRateVDto>(false, mapper.Map<CompanionReserveCommentRateVDto>(item));
        }

        public CompanionReserveCommentRateSearchDto Search(CompanionReserveCommentRateInputDto baseSearchDto)
        {
            var model = _context.CompanionReserveCommentRates.Include(s => s.AssistanceQuestionnaire).Include(s => s.CompanionReserveCommentId).AsQueryable();

            if (baseSearchDto.AssistanceQuestionnaireId.HasValue)
            {
                model = model.Where(s => s.AssistanceQuestionnaireId == baseSearchDto.AssistanceQuestionnaireId.Value);
            }
            if (baseSearchDto.CompanionReserveCommentId.HasValue)
            {
                model = model.Where(s => s.CompanionReserveCommentId == baseSearchDto.CompanionReserveCommentId.Value);
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
            return new CompanionReserveCommentRateSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReserveCommentRateDto>> InsertAsyncDto(CompanionReserveCommentRateDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionReserveCommentRateDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionReserveCommentRate>(dto);
                    if (dto.Rate > 5 || dto.Rate < 1)
                    {
                        return new BaseResultDto<CompanionReserveCommentRateDto>(false, val1: Resource.Notification.TheRangeEnteredIsNotCorrect, val2: nameof(dto.Rate), data: dto);
                    }

                    await _context.CompanionReserveCommentRates.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<CompanionReserveCommentRateDto>(true, mapper.Map<CompanionReserveCommentRateDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReserveCommentRateDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

    }
}
