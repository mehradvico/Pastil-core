using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv
{
    public class PastilMatchReportService : CommonSrv<PastilMatchReport, PastilMatchReportDto>, IPastilMatchReportService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeService _noticeService;

        public PastilMatchReportService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser, INoticeService noticeService) : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
            _currentUser = currentUser;
            _noticeService = noticeService;
        }

        public async Task<BaseResultDto<PastilMatchReportVDto>> FindAsyncVDto(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                var item = await GetReportQuery().FirstOrDefaultAsync(s => s.Id == id);

                if (item == null)
                {
                    return new BaseResultDto<PastilMatchReportVDto>(false, Resource.Notification.NothingFound, null);
                }

                if (!isAdmin && item.ReporterUserId != userId)
                {
                    return new BaseResultDto<PastilMatchReportVDto>(false, Resource.Notification.AccessDenied, null);
                }

                return new BaseResultDto<PastilMatchReportVDto>(true, mapper.Map<PastilMatchReportVDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchReportVDto>(false, ex.Message, null);
            }
        }

        public PastilMatchReportSearchDto Search(PastilMatchReportInputDto dto)
        {
            var userId = _currentUser.CurrentUser.UserId;
            var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

            var model = GetReportQuery();

            if (!isAdmin)
            {
                model = model.Where(s => s.ReporterUserId == userId);
            }

            if (dto.ReporterUserId.HasValue)
            {
                model = model.Where(s => s.ReporterUserId == dto.ReporterUserId.Value);
            }

            if (dto.ReportedUserId.HasValue)
            {
                model = model.Where(s => s.ReportedUserId == dto.ReportedUserId.Value);
            }

            if (dto.ReportedProfileId.HasValue)
            {
                model = model.Where(s => s.ReportedProfileId == dto.ReportedProfileId.Value);
            }

            if (dto.PastilMatchId.HasValue)
            {
                model = model.Where(s => s.PastilMatchId == dto.PastilMatchId.Value);
            }

            if (dto.PastilMatchMessageId.HasValue)
            {
                model = model.Where(s => s.PastilMatchMessageId == dto.PastilMatchMessageId.Value);
            }

            if (dto.PastilMatchReportReasonId.HasValue)
            {
                model = model.Where(s => s.PastilMatchReportReasonId == dto.PastilMatchReportReasonId.Value);
            }

            if (dto.IsReviewed.HasValue)
            {
                model = dto.IsReviewed.Value ? model.Where(s => s.ReviewDate.HasValue) : model.Where(s => !s.ReviewDate.HasValue);
            }

            switch (dto.SortBy)
            {
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
            }

            return new PastilMatchReportSearchDto(dto, model, mapper);
        }

        public override async Task<BaseResultDto<PastilMatchReportDto>> InsertAsyncDto(PastilMatchReportDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<PastilMatchReportDto>.ModelErrors(dto);

                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                var reporterUserId = _currentUser.CurrentUser.UserId;

                if (reporterUserId == dto.ReportedUserId)
                {
                    return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.PastilMatchCannotReportYourself, dto);
                }

                var reportedUserExists = await _context.Users.AnyAsync(s => s.Id == dto.ReportedUserId);

                if (!reportedUserExists)
                {
                    return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.NothingFound, dto);
                }

                var reportReason = await _context.PastilMatchReportReasons.FirstOrDefaultAsync(s => s.Id == dto.PastilMatchReportReasonId && s.Active && !s.Deleted);

                if (reportReason == null)
                {
                    return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.InvalidPastilMatchReportReason, dto);
                }

                if (reportReason.IsDescriptionRequired && string.IsNullOrWhiteSpace(dto.Description))
                {
                    return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.PastilMatchReportDescriptionRequired, dto);
                }

                if (dto.ReportedProfileId.HasValue)
                {
                    var reportedProfile = await _context.PastilMatchProfiles.Include(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == dto.ReportedProfileId.Value && !s.Deleted);

                    if (reportedProfile == null)
                    {
                        return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.NothingFound, dto);
                    }

                    if (reportedProfile.UserPet.UserId != dto.ReportedUserId)
                    {
                        return new BaseResultDto<PastilMatchReportDto>(false, Resource.Notification.PastilMatchReportTargetMismatch, dto);
                    }
                }

                if (dto.PastilMatchId.HasValue)
                {
                    var matchValidation = await ValidateMatchAsync(dto.PastilMatchId.Value, reporterUserId, dto.ReportedUserId);

                    if (!matchValidation.IsSuccess)
                    {
                        return new BaseResultDto<PastilMatchReportDto>(false, matchValidation.Messages, dto);
                    }
                }

                if (dto.PastilMatchMessageId.HasValue)
                {
                    var messageValidation = await ValidateMessageAsync(dto.PastilMatchMessageId.Value, dto.PastilMatchId, reporterUserId, dto.ReportedUserId);

                    if (!messageValidation.IsSuccess)
                    {
                        return new BaseResultDto<PastilMatchReportDto>(false, messageValidation.Messages, dto);
                    }
                }

                var item = mapper.Map<PastilMatchReport>(dto);

                item.ReporterUserId = reporterUserId;
                item.AdminDescription = null;
                item.ReviewDate = null;
                item.CreateDate = DateTime.Now;

                await _context.PastilMatchReports.AddAsync(item);
                await _context.SaveChangesAsync();
                var metadata = new Dictionary<string, string>
                {
                    { "reportedUserId", item.ReportedUserId.ToString() },
                    { "reasonId", item.PastilMatchReportReasonId.ToString() }
                };

                if (item.ReportedProfileId.HasValue)
                {
                    metadata["reportedProfileId"] = item.ReportedProfileId.Value.ToString();
                }

                if (item.PastilMatchId.HasValue)
                {
                    metadata["pastilMatchId"] = item.PastilMatchId.Value.ToString();
                }

                if (item.PastilMatchMessageId.HasValue)
                {
                    metadata["pastilMatchMessageId"] = item.PastilMatchMessageId.Value.ToString();
                }

                await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.PastilMatchReportSubmitted,
                    ActorUserId = reporterUserId,
                    ReferenceType = "PastilMatchReport",
                    ReferenceId = item.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.PastilMatchReportSubmitted}:{item.Id}",
                    Metadata = metadata
                });

                return new BaseResultDto<PastilMatchReportDto>(true, mapper.Map<PastilMatchReportDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PastilMatchReportDto>(false, ex.Message, dto);
            }
        }

        public async Task<BaseResultDto> UpdateReviewDto(PastilMatchReportReviewDto dto)
        {
            try
            {
                var isAdmin = _currentUser.CurrentUser.RoleEnum == RoleEnum.Admin.ToString();

                if (!isAdmin)
                {
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);
                }

                if (string.IsNullOrWhiteSpace(dto.AdminDescription))
                {
                    return new BaseResultDto(false, Resource.Notification.PastilMatchReportAdminDescriptionRequired);
                }

                var item = await _context.PastilMatchReports.FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (item == null)
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                item.AdminDescription = dto.AdminDescription;
                item.ReviewDate = DateTime.Now;

                _context.PastilMatchReports.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        private async Task<BaseResultDto> ValidateMatchAsync(long pastilMatchId, long reporterUserId, long reportedUserId)
        {
            var match = await _context.PastilMatches.Include(s => s.FirstProfile).ThenInclude(s => s.UserPet).Include(s => s.SecondProfile).ThenInclude(s => s.UserPet).FirstOrDefaultAsync(s => s.Id == pastilMatchId);

            if (match == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            var firstUserId = match.FirstProfile.UserPet.UserId;
            var secondUserId = match.SecondProfile.UserPet.UserId;
            var reporterIsParticipant = firstUserId == reporterUserId || secondUserId == reporterUserId;
            var reportedIsParticipant = firstUserId == reportedUserId || secondUserId == reportedUserId;

            if (!reporterIsParticipant || !reportedIsParticipant)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }

            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto> ValidateMessageAsync(long messageId, long? pastilMatchId, long reporterUserId, long reportedUserId)
        {
            var message = await _context.PastilMatchMessages
                .Include(s => s.SenderProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatch).ThenInclude(s => s.FirstProfile).ThenInclude(s => s.UserPet)
                .Include(s => s.PastilMatch).ThenInclude(s => s.SecondProfile).ThenInclude(s => s.UserPet)
                .FirstOrDefaultAsync(s => s.Id == messageId && !s.Deleted);

            if (message == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            if (pastilMatchId.HasValue && message.PastilMatchId != pastilMatchId.Value)
            {
                return new BaseResultDto(false, Resource.Notification.PastilMatchReportTargetMismatch);
            }

            if (message.SenderProfile.UserPet.UserId != reportedUserId)
            {
                return new BaseResultDto(false, Resource.Notification.PastilMatchReportTargetMismatch);
            }

            var firstUserId = message.PastilMatch.FirstProfile.UserPet.UserId;
            var secondUserId = message.PastilMatch.SecondProfile.UserPet.UserId;

            if (firstUserId != reporterUserId && secondUserId != reporterUserId)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }

            return new BaseResultDto(true);
        }

        private IQueryable<PastilMatchReport> GetReportQuery()
        {
            return _context.PastilMatchReports
                .Include(s => s.ReporterUser)
                .Include(s => s.ReportedUser)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Picture)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.UserPet).ThenInclude(s => s.Pet)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.EnergyLevel)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.SocialLevel)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.City)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.Neighborhood)
                .Include(s => s.ReportedProfile).ThenInclude(s => s.PastilMatchProfileGoals).ThenInclude(s => s.PastilMatchGoal)
                .Include(s => s.PastilMatch)
                .Include(s => s.PastilMatchReportReason)
                .AsQueryable();
        }
    }
}
