using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv
{
    public class CompanionInstantCallRequestService : CommonSrv<CompanionInstantCallRequest, CompanionInstantCallRequestDto>, ICompanionInstantCallRequestService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<CompanionInstantCallRequestService> _logger;

        public CompanionInstantCallRequestService(
            IDataBaseContext _context,
            IMapper mapper,
            IPushNotificationService pushNotificationService,
            ILogger<CompanionInstantCallRequestService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._pushNotificationService = pushNotificationService;
            this._logger = logger;
        }

        public async Task<BaseResultDto<CompanionInstantCallRequestVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionInstantCallRequests
                .Include(r => r.Booker)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackage).ThenInclude(p => p.CompanionAssistance)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackageOnline)
                .FirstOrDefaultAsync(r => r.Id == id && !r.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionInstantCallRequestVDto>(true, mapper.Map<CompanionInstantCallRequestVDto>(item));
            }
            return new BaseResultDto<CompanionInstantCallRequestVDto>(false, mapper.Map<CompanionInstantCallRequestVDto>(item));
        }

        public CompanionInstantCallRequestSearchDto Search(CompanionInstantCallRequestInputDto baseSearchDto)
        {
            var model = _context.CompanionInstantCallRequests
                .Include(r => r.Booker)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackage).ThenInclude(p => p.CompanionAssistance)
                .Include(r => r.CompanionAssistancePackageOnlineSelection).ThenInclude(s => s.CompanionAssistancePackageOnline)
                .AsQueryable().Where(r => !r.Deleted);

            if (baseSearchDto.CompanionAssistancePackageOnlineSelectionId.HasValue)
            {
                model = model.Where(r => r.CompanionAssistancePackageOnlineSelectionId == baseSearchDto.CompanionAssistancePackageOnlineSelectionId.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(r => r.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(r => r.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionInstantCallRequestSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<CompanionInstantCallRequestDto>> InsertAsyncDto(CompanionInstantCallRequestDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionInstantCallRequestDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                var selection = await _context.CompanionAssistancePackageOnlineSelections
                    .Include(s => s.CompanionAssistancePackage).ThenInclude(p => p.CompanionAssistance).ThenInclude(a => a.Companion).ThenInclude(c => c.Owner)
                    .Include(s => s.CompanionAssistancePackageOnline)
                    .FirstOrDefaultAsync(s => s.Id == dto.CompanionAssistancePackageOnlineSelectionId && s.Active && !s.Deleted);

                if (selection?.CompanionAssistancePackage?.CompanionAssistance?.Companion?.Owner == null)
                {
                    return new BaseResultDto<CompanionInstantCallRequestDto>(false, Resource.Notification.NothingFound, dto);
                }

                var booker = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.BookerId && !u.Deleted);
                if (booker == null)
                {
                    return new BaseResultDto<CompanionInstantCallRequestDto>(false, Resource.Notification.NothingFound, dto);
                }

                dto.CreateDate = DateTime.Now;
                var item = mapper.Map<CompanionInstantCallRequest>(dto);
                await _context.CompanionInstantCallRequests.AddAsync(item);
                await _context.SaveChangesAsync();

                var bookerName = $"{booker.FirstName} {booker.LastName}".Trim();
                var onlineTypeName = selection.CompanionAssistancePackageOnline?.Name ?? "";
                var owner = selection.CompanionAssistancePackage.CompanionAssistance.Companion.Owner;

                try
                {
                    await _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushInstantCallRequestCompanion,
                        owner.Id,
                        token1: bookerName,
                        token2: onlineTypeName,
                        token3: item.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send instant call request push for request {RequestId}.", item.Id);
                }

                return new BaseResultDto<CompanionInstantCallRequestDto>(true, mapper.Map<CompanionInstantCallRequestDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionInstantCallRequestDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
