using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionReportSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReportSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReportSrv
{
    public class CompanionReportService : CommonSrv<CompanionReport, CompanionReportDto>, ICompanionReportService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly INoticeService _notificationService;
        public CompanionReportService(IDataBaseContext _context, IMapper mapper, ICodeService codeService, INoticeService notificationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._notificationService = notificationService;
        }

        public override async Task<BaseResultDto<CompanionReportDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionReports.Include(s => s.Companion).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionReportDto>(true, mapper.Map<CompanionReportDto>(item));
            }
            return new BaseResultDto<CompanionReportDto>(false, mapper.Map<CompanionReportDto>(item));
        }

        public async Task<BaseResultDto<CompanionReportVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionReports.Include(s => s.Companion).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionReportVDto>(true, mapper.Map<CompanionReportVDto>(item));
            }
            return new BaseResultDto<CompanionReportVDto>(false, mapper.Map<CompanionReportVDto>(item));
        }

        public CompanionReportSearchDto Search(CompanionReportInputDto baseSearchDto)
        {
            var model = _context.CompanionReports.Include(s => s.Companion).Include(s => s.User).AsQueryable();

            if (baseSearchDto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);
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
            return new CompanionReportSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionReportDto>> InsertAsyncDto(CompanionReportDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionReportDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionReport>(dto);
                    bool existed = await _context.CompanionReports.AnyAsync(s => s.CompanionId == item.CompanionId && s.UserId == item.UserId);
                    if (existed)
                    {
                        return new BaseResultDto<CompanionReportDto>(false, val1: Resource.Notification.YourReportHasBeenSubmitedForThisCompanionBefore, val2: nameof(dto.CompanionId), data: dto);
                    }
                    item.CreateDate = DateTime.Now;
                    await _context.CompanionReports.AddAsync(item);
                    await _context.SaveChangesAsync();
                    await _notificationService.CreateAsync(new NoticeCreateDto { Label = NoticeTypeLabels.CompanionReportSubmitted, ReferenceType = "CompanionReport", ReferenceId = item.Id, DeduplicationKey = $"{NoticeTypeLabels.CompanionReportSubmitted}:{item.Id}", Metadata = new Dictionary<string, string> { { "companionId", item.CompanionId.ToString() } } });
                    return new BaseResultDto<CompanionReportDto>(true, mapper.Map<CompanionReportDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionReportDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
