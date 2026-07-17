using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceReportSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using AutoMapper;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionAssistanceSrvs.CompanionAssistanceAssistanceReportSrv
{
    public class CompanionAssistanceReportService : CommonSrv<CompanionAssistanceReport, CompanionAssistanceReportDto>, ICompanionAssistanceReportService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        public CompanionAssistanceReportService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, INoticeService notificationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._notificationService = notificationService;
        }

        public override async Task<BaseResultDto<CompanionAssistanceReportDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionAssistanceReports.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceReportDto>(true, mapper.Map<CompanionAssistanceReportDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceReportDto>(false, mapper.Map<CompanionAssistanceReportDto>(item));
        }

        public async Task<BaseResultDto<CompanionAssistanceReportVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistanceReports.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceReportVDto>(true, mapper.Map<CompanionAssistanceReportVDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceReportVDto>(false, mapper.Map<CompanionAssistanceReportVDto>(item));
        }

        public CompanionAssistanceReportSearchDto Search(CompanionAssistanceReportInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistanceReports.Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).Include(s => s.CompanionAssistance).ThenInclude(s => s.Companion).Include(s => s.User).AsQueryable();

            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserId == baseSearchDto.UserId.Value);
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
            return new CompanionAssistanceReportSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionAssistanceReportDto>> InsertAsyncDto(CompanionAssistanceReportDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistanceReportDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionAssistanceReport>(dto);
                    bool existed = await _context.CompanionAssistanceReports.AnyAsync(s => s.CompanionAssistanceId == item.CompanionAssistanceId && s.UserId == item.UserId);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionAssistanceReportDto>(false, val1: Resource.Notification.YourReportHasBeenSubmitedForThisCompanionAssistanceBefore, val2: nameof(dto.CompanionAssistanceId), data: dto);
                    }
                    item.CreateDate = DateTime.Now;
                    await _context.CompanionAssistanceReports.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionAssistanceReportSubmitted, ReferenceType = "CompanionAssistanceReport", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionAssistanceReportSubmitted}:{item.Id}" });
                    return new BaseResultDto<CompanionAssistanceReportDto>(true, mapper.Map<CompanionAssistanceReportDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistanceReportDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
