using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.SchoolField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv
{
    public class SchoolCourseLiveService : ISchoolCourseLiveService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ILiveKitService _liveKit;
        private readonly SchoolLiveParticipantTracker _tracker;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<SchoolCourseLiveService> _logger;

        public SchoolCourseLiveService(
            IDataBaseContext context,
            IMapper mapper,
            ILiveKitService liveKit,
            SchoolLiveParticipantTracker tracker,
            IPushNotificationService pushNotificationService,
            ILogger<SchoolCourseLiveService> logger)
        {
            _context = context;
            _mapper = mapper;
            _liveKit = liveKit;
            _tracker = tracker;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        private async Task<SchoolCourseSession> LoadOwnedSessionAsync(long schoolCourseSessionId, long companionId)
        {
            return await _context.SchoolCourseSessions
                .Include(s => s.SchoolCourse).ThenInclude(c => c.School)
                .FirstOrDefaultAsync(s => s.Id == schoolCourseSessionId
                    && !s.Deleted
                    && s.SchoolCourse.School.CompanionId == companionId);
        }

        private async Task<SchoolCourseLiveSession> GetOrCreateRowAsync(long schoolCourseSessionId)
        {
            var item = await _context.SchoolCourseLiveSessions.AsTracking()
                .FirstOrDefaultAsync(l => l.SchoolCourseSessionId == schoolCourseSessionId);
            if (item != null)
                return item;

            item = new SchoolCourseLiveSession
            {
                SchoolCourseSessionId = schoolCourseSessionId,
                RoomName = $"school-live-{Guid.NewGuid():N}",
                StatusId = (int)SchoolLiveStatusEnum.NotStarted,
                RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.None
            };
            await _context.SchoolCourseLiveSessions.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<BaseResultDto<SchoolCourseLiveSessionVDto>> GetOrCreateForCompanionAsync(long schoolCourseSessionId, long companionId)
        {
            var session = await LoadOwnedSessionAsync(schoolCourseSessionId, companionId);
            if (session == null || session.SchoolCourse.CourseTypeId != (int)SchoolCourseTypeEnum.Live)
                return new BaseResultDto<SchoolCourseLiveSessionVDto>(false, Resource.Notification.NothingFound, null);

            var item = await GetOrCreateRowAsync(schoolCourseSessionId);
            return new BaseResultDto<SchoolCourseLiveSessionVDto>(true, _mapper.Map<SchoolCourseLiveSessionVDto>(item));
        }

        // برای کاربر: از schoolCourseSessionId (همان چیزی که در پوش «شروع شد» می‌آید) به liveSessionId
        // می‌رسد - این Endpoint عمداً لایو را نمی‌سازد (فقط مربی با Start می‌سازد)؛ اگر مربی هنوز حتی
        // یک‌بار هم لایو را باز نکرده، NothingFound می‌دهد و فرانت باید «هنوز شروع نشده» نشان دهد.
        public async Task<BaseResultDto<SchoolCourseLiveSessionVDto>> GetForEnrolledUserAsync(long schoolCourseSessionId, long userId)
        {
            var session = await _context.SchoolCourseSessions
                .Include(s => s.SchoolCourse)
                .FirstOrDefaultAsync(s => s.Id == schoolCourseSessionId && !s.Deleted);
            if (session == null)
                return new BaseResultDto<SchoolCourseLiveSessionVDto>(false, Resource.Notification.NothingFound, null);

            var enrolled = await _context.SchoolReserves.AnyAsync(r =>
                r.SchoolCourseId == session.SchoolCourseId && r.BookerId == userId && !r.IsCancel);
            if (!enrolled)
                return new BaseResultDto<SchoolCourseLiveSessionVDto>(false, Resource.Notification.AccessDenied, null);

            var item = await _context.SchoolCourseLiveSessions.AsNoTracking()
                .FirstOrDefaultAsync(l => l.SchoolCourseSessionId == schoolCourseSessionId);
            if (item == null)
                return new BaseResultDto<SchoolCourseLiveSessionVDto>(false, Resource.Notification.NothingFound, null);

            return new BaseResultDto<SchoolCourseLiveSessionVDto>(true, _mapper.Map<SchoolCourseLiveSessionVDto>(item));
        }

        public async Task<BaseResultDto<SchoolLiveTokenVDto>> StartAsync(SchoolLiveStartDto dto, long companionId)
        {
            // بخش آنلاین مدرسه فعلاً غیرفعال است - کد کامل است، فقط اجرا (شروع پخش واقعی) مسدود است.
            if (!Application.Common.FeatureFlags.SchoolOnlineCoursesEnabled)
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.FeatureTemporarilyDisabled, null);

            var session = await LoadOwnedSessionAsync(dto.SchoolCourseSessionId, companionId);
            if (session == null || session.SchoolCourse.CourseTypeId != (int)SchoolCourseTypeEnum.Live)
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.NothingFound, null);

            if (dto.WantsRecording && string.IsNullOrWhiteSpace(dto.RecordingName))
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.SchoolLiveRecordingNameRequired, null);

            var item = await GetOrCreateRowAsync(dto.SchoolCourseSessionId);

            if (item.StatusId == (int)SchoolLiveStatusEnum.Ended)
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.InvalidData, null);

            if (item.StatusId != (int)SchoolLiveStatusEnum.Live)
            {
                await _liveKit.CreateRoomAsync(item.RoomName);

                item.StatusId = (int)SchoolLiveStatusEnum.Live;
                item.StartedDate = DateTime.Now;
                item.WantsRecording = dto.WantsRecording;
                item.RecordingName = dto.WantsRecording ? dto.RecordingName.Trim() : null;
                item.RecordingDescription = dto.WantsRecording ? dto.RecordingDescription?.Trim() : null;

                if (dto.WantsRecording)
                {
                    try
                    {
                        item.EgressId = await _liveKit.StartRecordingAsync(item.RoomName);
                        item.RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.Requested;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start LiveKit recording for school live session {LiveSessionId}.", item.Id);
                        item.RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.Failed;
                    }
                }

                _context.SchoolCourseLiveSessions.Update(item);
                await _context.SaveChangesAsync();
            }

            var companion = await _context.Companions.FirstOrDefaultAsync(c => c.Id == companionId);
            var token = _liveKit.GenerateAccessToken(item.RoomName, $"companion-{companionId}", companion?.Name, canPublish: true, canSubscribe: false);

            return new BaseResultDto<SchoolLiveTokenVDto>(true, new SchoolLiveTokenVDto
            {
                LiveSessionId = item.Id,
                RoomName = item.RoomName,
                Token = token,
                StatusId = item.StatusId
            });
        }

        public async Task<BaseResultDto> EndAsync(long liveSessionId, long companionId)
        {
            var item = await _context.SchoolCourseLiveSessions.AsTracking()
                .Include(l => l.SchoolCourseSession).ThenInclude(s => s.SchoolCourse).ThenInclude(c => c.School)
                .FirstOrDefaultAsync(l => l.Id == liveSessionId);
            if (item == null || item.SchoolCourseSession.SchoolCourse.School.CompanionId != companionId)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            if (item.StatusId != (int)SchoolLiveStatusEnum.Live)
                return new BaseResultDto(true);

            if (item.WantsRecording && !string.IsNullOrWhiteSpace(item.EgressId))
            {
                try
                {
                    await _liveKit.StopRecordingAsync(item.EgressId);
                    item.RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.Processing;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to stop LiveKit recording for school live session {LiveSessionId}.", item.Id);
                }
            }

            try
            {
                await _liveKit.DeleteRoomAsync(item.RoomName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete LiveKit room for school live session {LiveSessionId}.", item.Id);
            }

            item.StatusId = (int)SchoolLiveStatusEnum.Ended;
            item.EndedDate = DateTime.Now;
            _context.SchoolCourseLiveSessions.Update(item);
            await _context.SaveChangesAsync();

            _tracker.RemoveLiveSession(liveSessionId);

            return new BaseResultDto(true);
        }

        public async Task<bool> IsCompanionOwnerAsync(long liveSessionId, long companionId)
        {
            return await _context.SchoolCourseLiveSessions
                .Include(l => l.SchoolCourseSession).ThenInclude(s => s.SchoolCourse).ThenInclude(c => c.School)
                .AnyAsync(l => l.Id == liveSessionId && l.SchoolCourseSession.SchoolCourse.School.CompanionId == companionId);
        }

        public async Task<bool> IsCompanionOwnerByUserIdAsync(long liveSessionId, long userId)
        {
            return await _context.SchoolCourseLiveSessions
                .Include(l => l.SchoolCourseSession).ThenInclude(s => s.SchoolCourse).ThenInclude(c => c.School).ThenInclude(sc => sc.Companion)
                .AnyAsync(l => l.Id == liveSessionId && l.SchoolCourseSession.SchoolCourse.School.Companion.OwnerId == userId);
        }

        public async Task<bool> IsEnrolledViewerAsync(long liveSessionId, long userId, long userPetId)
        {
            var courseId = await _context.SchoolCourseLiveSessions
                .Where(l => l.Id == liveSessionId)
                .Select(l => (long?)l.SchoolCourseSession.SchoolCourseId)
                .FirstOrDefaultAsync();
            if (!courseId.HasValue)
                return false;

            return await _context.SchoolReserves.AnyAsync(r =>
                r.SchoolCourseId == courseId.Value && r.BookerId == userId && r.UserPetId == userPetId && !r.IsCancel);
        }

        public async Task<BaseResultDto<SchoolLiveTokenVDto>> GetViewerTokenAsync(long liveSessionId, long userId)
        {
            var item = await _context.SchoolCourseLiveSessions
                .Include(l => l.SchoolCourseSession).ThenInclude(s => s.SchoolCourse)
                .FirstOrDefaultAsync(l => l.Id == liveSessionId);
            if (item == null)
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.NothingFound, null);

            var courseId = item.SchoolCourseSession.SchoolCourseId;
            var enrolled = await _context.SchoolReserves.AnyAsync(r => r.SchoolCourseId == courseId && r.BookerId == userId && !r.IsCancel);
            if (!enrolled)
                return new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.AccessDenied, null);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var displayName = $"{user?.FirstName} {user?.LastName}".Trim();
            var token = _liveKit.GenerateAccessToken(item.RoomName, $"user-{userId}", displayName, canPublish: false, canSubscribe: true);

            return new BaseResultDto<SchoolLiveTokenVDto>(true, new SchoolLiveTokenVDto
            {
                LiveSessionId = item.Id,
                RoomName = item.RoomName,
                Token = token,
                StatusId = item.StatusId
            });
        }

        public async Task<BaseResultDto<List<SchoolLiveParticipantVDto>>> GetParticipantsAsync(long liveSessionId, long companionId)
        {
            if (!await IsCompanionOwnerAsync(liveSessionId, companionId))
                return new BaseResultDto<List<SchoolLiveParticipantVDto>>(false, Resource.Notification.AccessDenied, null);

            var courseId = await _context.SchoolCourseLiveSessions
                .Where(l => l.Id == liveSessionId)
                .Select(l => l.SchoolCourseSession.SchoolCourseId)
                .FirstOrDefaultAsync();

            var reserves = await _context.SchoolReserves
                .Include(r => r.UserPet).ThenInclude(p => p.Pet)
                .Include(r => r.UserPet).ThenInclude(p => p.PetBreed)
                .Include(r => r.UserPet).ThenInclude(p => p.Picture)
                .Include(r => r.Booker)
                .Where(r => r.SchoolCourseId == courseId && !r.IsCancel)
                .ToListAsync();

            var presentIds = _tracker.GetPresentUserPetIds(liveSessionId);

            var result = reserves.Select(r => new SchoolLiveParticipantVDto
            {
                UserPetId = r.UserPetId,
                PetName = r.UserPet?.Name,
                PetPictureId = r.UserPet?.PictureId,
                PetPictureUrl = r.UserPet?.Picture?.Url,
                PetTypeName = r.UserPet?.Pet?.Name,
                PetBreedName = r.UserPet?.PetBreed?.Name,
                OwnerId = r.BookerId,
                OwnerName = $"{r.Booker?.FirstName} {r.Booker?.LastName}".Trim(),
                OwnerMobile = r.Booker?.Mobile,
                Present = presentIds.Contains(r.UserPetId)
            }).ToList();

            return new BaseResultDto<List<SchoolLiveParticipantVDto>>(true, result);
        }

        public async Task<BaseResultDto<SchoolLiveCommentDto>> PostCommentAsync(long liveSessionId, long userId, string message)
        {
            message = message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return new BaseResultDto<SchoolLiveCommentDto>(false, Resource.Notification.InvalidData, null);
            if (message.Length > 500)
                message = message.Substring(0, 500);

            var live = await _context.SchoolCourseLiveSessions.FirstOrDefaultAsync(l => l.Id == liveSessionId);
            if (live == null)
                return new BaseResultDto<SchoolLiveCommentDto>(false, Resource.Notification.NothingFound, null);

            // دسترسی: یا مربی صاحبِ دوره است، یا بیننده‌ای که حداقل یک پت فعال در این دوره ثبت‌نام کرده
            var courseId = await _context.SchoolCourseLiveSessions.Where(l => l.Id == liveSessionId)
                .Select(l => l.SchoolCourseSession.SchoolCourseId).FirstOrDefaultAsync();
            var isCompanionOwner = await IsCompanionOwnerByUserIdAsync(liveSessionId, userId);
            var isEnrolled = await _context.SchoolReserves.AnyAsync(r => r.SchoolCourseId == courseId && r.BookerId == userId && !r.IsCancel);
            if (!isCompanionOwner && !isEnrolled)
                return new BaseResultDto<SchoolLiveCommentDto>(false, Resource.Notification.AccessDenied, null);

            var comment = new SchoolLiveComment
            {
                SchoolCourseLiveSessionId = liveSessionId,
                UserId = userId,
                Message = message,
                CreateDate = DateTime.Now,
                Deleted = false
            };
            await _context.SchoolLiveComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return new BaseResultDto<SchoolLiveCommentDto>(true, new SchoolLiveCommentDto
            {
                Id = comment.Id,
                SchoolCourseLiveSessionId = liveSessionId,
                UserId = userId,
                Message = message,
                CreateDate = DateTime.Now,
                UserName = $"{user?.FirstName} {user?.LastName}".Trim(),
                UserPicture = user?.Picture?.Url
            });
        }

        public async Task<BaseResultDto<List<SchoolLiveCommentDto>>> GetCommentsAsync(long liveSessionId, long userId)
        {
            var courseId = await _context.SchoolCourseLiveSessions.Where(l => l.Id == liveSessionId)
                .Select(l => l.SchoolCourseSession.SchoolCourseId).FirstOrDefaultAsync();
            var isCompanionOwner = await IsCompanionOwnerByUserIdAsync(liveSessionId, userId);
            var isEnrolled = await _context.SchoolReserves.AnyAsync(r => r.SchoolCourseId == courseId && r.BookerId == userId && !r.IsCancel);
            if (!isCompanionOwner && !isEnrolled)
                return new BaseResultDto<List<SchoolLiveCommentDto>>(false, Resource.Notification.AccessDenied, null);

            var comments = await _context.SchoolLiveComments
                .Include(c => c.User)
                .Where(c => c.SchoolCourseLiveSessionId == liveSessionId && !c.Deleted)
                .OrderBy(c => c.Id)
                .Select(c => new SchoolLiveCommentDto
                {
                    Id = c.Id,
                    SchoolCourseLiveSessionId = c.SchoolCourseLiveSessionId,
                    UserId = c.UserId,
                    Message = c.Message,
                    CreateDate = c.CreateDate,
                    UserName = (c.User.FirstName + " " + c.User.LastName).Trim(),
                    UserPicture = c.User.Picture.Url
                })
                .ToListAsync();

            return new BaseResultDto<List<SchoolLiveCommentDto>>(true, comments);
        }

        public async Task HandleEgressWebhookAsync(string egressId, bool succeeded, string fileUrl)
        {
            var item = await _context.SchoolCourseLiveSessions.AsTracking()
                .Include(l => l.SchoolCourseSession)
                .FirstOrDefaultAsync(l => l.EgressId == egressId);
            if (item == null)
                return;

            if (!succeeded || string.IsNullOrWhiteSpace(fileUrl))
            {
                item.RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.Failed;
                _context.SchoolCourseLiveSessions.Update(item);
                await _context.SaveChangesAsync();
                return;
            }

            var file = new Entities.Entities.File
            {
                Name = item.RecordingName ?? "ضبط لایو مدرسه",
                OrginalName = item.RecordingName ?? "school-live-recording.mp4",
                Url = fileUrl,
                DirectUrl = fileUrl,
                Extension = "mp4",
                ContentType = "video/mp4",
                CreateDate = DateTime.Now
            };
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();

            var video = new SchoolCourseVideo
            {
                SchoolCourseId = item.SchoolCourseSession.SchoolCourseId,
                Name = item.RecordingName ?? "ضبط لایو مدرسه",
                FileId = file.Id,
                SortOrder = 0,
                Active = true,
                Deleted = false
            };
            await _context.SchoolCourseVideos.AddAsync(video);
            await _context.SaveChangesAsync();

            item.SchoolCourseVideoId = video.Id;
            item.RecordingStatusId = (int)SchoolLiveRecordingStatusEnum.Ready;
            _context.SchoolCourseLiveSessions.Update(item);
            await _context.SaveChangesAsync();
        }

        // Job زمان‌بندی‌شده (Hangfire، هر دقیقه): ۵ دقیقه مانده به شروع یک جلسه‌ی زنده‌ی مدرسه، به همه‌ی
        // ثبت‌نام‌کنندگانِ فعال آن دوره پوش یادآوری می‌فرستد. جدا از StartingPushSentDate (که سرِ لحظه‌ی
        // شروع، با دکمه‌ی «ورود به جلسه»، در SchoolReserveService.SendClassStartingPushesAsync می‌رود).
        public async Task SendClassReminderPushesAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var windowEnd = now.AddMinutes(5);

            var dueSessions = await _context.SchoolCourseSessions
                .Include(s => s.SchoolCourse)
                .AsTracking()
                .Where(s =>
                    !s.Deleted &&
                    s.Active &&
                    s.ReminderPushSentDate == null &&
                    s.SchoolCourse.CourseTypeId == (int)SchoolCourseTypeEnum.Live &&
                    s.SessionDate.Date == now.Date)
                .ToListAsync(cancellationToken);

            foreach (var session in dueSessions)
            {
                if (!TimeSpan.TryParse(session.StartTime, out var startTime))
                    continue;

                var sessionStart = session.SessionDate.Date + startTime;
                if (sessionStart <= now || sessionStart > windowEnd)
                    continue;

                var enrolledBookerIds = await _context.SchoolReserves
                    .Where(r => r.SchoolCourseId == session.SchoolCourseId && !r.IsCancel)
                    .Select(r => r.BookerId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var bookerId in enrolledBookerIds)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushSchoolClassReminder5Min,
                            bookerId,
                            token1: session.SchoolCourse.Name,
                            token2: session.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send 5-min class reminder push for session {SessionId} to booker {BookerId}.", session.Id, bookerId);
                    }
                }

                session.ReminderPushSentDate = now;
            }

            if (dueSessions.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
