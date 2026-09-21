using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Security;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Dto;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.OnlineSessionSrv
{
    // جلسه‌ی آنلاین بدون رزرو. مجاز بودن رابطه‌محور است (فقط دو طرف جلسه)، نه نقش‌محور؛ شروع جلسه فقط برای نماینده/اپراتور.
    public class OnlineSessionService : IOnlineSessionService
    {
        private const int MaxContentLength = 4000;
        private const int MaxPageSize = 100;
        private const int MinSearchLength = 3;
        private const int MaxSearchResults = 10;
        private const int MaxPets = 20;
        private const int MaxGalleryPictures = 30;
        private static readonly TimeSpan ActiveWindow = TimeSpan.FromHours(24);

        private readonly IDataBaseContext _context;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPushNotificationService _pushNotificationService;

        public OnlineSessionService(
            IDataBaseContext context,
            ICurrentUserHelper currentUser,
            IPushNotificationService pushNotificationService)
        {
            _context = context;
            _currentUser = currentUser;
            _pushNotificationService = pushNotificationService;
        }

        public async Task<BaseResultDto<OnlineSessionVDto>> StartAsync(OnlineSessionStartDto dto)
        {
            try
            {
                var current = _currentUser.CurrentUser;
                var isAgent = (current.CompanionId ?? 0) > 0 || current.IsCompanionUser;

                // چت، تماس درون‌برنامه، تماس تصویری و تماس تلفنی معمولی.
                var channelImplemented = dto.ChannelId == (int)OnlineSessionChannelEnum.Chat
                    || dto.ChannelId == (int)OnlineSessionChannelEnum.InAppCall
                    || dto.ChannelId == (int)OnlineSessionChannelEnum.VideoCall
                    || dto.ChannelId == (int)OnlineSessionChannelEnum.Phone;
                if (!isAgent || !channelImplemented || dto.TargetUserId == current.UserId)
                    return new BaseResultDto<OnlineSessionVDto>(false, Resource.Notification.AccessDenied, null);

                var targetExists = await _context.Users.AnyAsync(s => s.Id == dto.TargetUserId && !s.Deleted && !s.Locked);
                if (!targetExists)
                    return new BaseResultDto<OnlineSessionVDto>(false, Resource.Notification.NothingFound, null);

                var since = DateTime.Now - ActiveWindow;
                var session = await _context.OnlineSessions
                    .Where(s => s.InitiatorUserId == current.UserId && s.TargetUserId == dto.TargetUserId
                        && s.ChannelId == dto.ChannelId && !s.EndDate.HasValue && s.CreateDate > since)
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();

                if (session == null)
                {
                    session = new OnlineSession
                    {
                        InitiatorUserId = current.UserId,
                        TargetUserId = dto.TargetUserId,
                        ChannelId = dto.ChannelId,
                        CreateDate = DateTime.Now
                    };
                    await _context.OnlineSessions.AddAsync(session);
                    await _context.SaveChangesAsync();
                }

                // اگر جلسه‌ی فعال قبلی هست همان استفاده می‌شود (گفتگو ادامه پیدا می‌کند) ولی به کاربر دوباره اطلاع داده می‌شود.
                // تماس صوتی/تصویری اینجا پوش ندارد: وقتی نماینده در هاب تماس وصل شد (CallHub.JoinSessionCall) پوش «زنگ خوردن» می‌رود.
                if (dto.ChannelId == (int)OnlineSessionChannelEnum.Chat)
                {
                    await _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushOnlineSessionChatInvite,
                        dto.TargetUserId,
                        current.FullName,
                        session.Id.ToString());
                }
                else if (dto.ChannelId == (int)OnlineSessionChannelEnum.Phone)
                {
                    // تماس تلفنی معمولی را دستگاه نماینده با شماره‌ی کاربر می‌گیرد (tel:)؛ این پوش فقط خبر می‌دهد تا کاربر
                    // تماس از شماره‌ی ناشناس را جواب بدهد. هیچ سیگنالینگ/صفحه‌ای ندارد.
                    await _pushNotificationService.SendPushAsync(
                        PushTypeEnum.PushOnlineSessionPhoneCallStarted,
                        dto.TargetUserId,
                        current.FullName,
                        session.Id.ToString());
                }

                return await FindAsync(session.Id);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<OnlineSessionVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<OnlineSessionVDto>> FindAsync(long id)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var item = await _context.OnlineSessions
                    .AsNoTracking()
                    .Include(s => s.InitiatorUser).ThenInclude(u => u.Picture)
                    .Include(s => s.TargetUser).ThenInclude(u => u.Picture)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (item == null)
                    return new BaseResultDto<OnlineSessionVDto>(false, Resource.Notification.NothingFound, null);

                if (!IsParticipant(item, userId))
                    return new BaseResultDto<OnlineSessionVDto>(false, Resource.Notification.AccessDenied, null);

                var result = ToVDto(item);
                result.InitiatorClinic = await FindClinicAsync(item.InitiatorUserId);

                // پت‌های کاربر فقط برای نمایندهای که جلسه را با او شروع کرده لازم و مجاز است
                if (item.InitiatorUserId == userId)
                    result.TargetPets = await FindPetsAsync(item.TargetUserId);

                return new BaseResultDto<OnlineSessionVDto>(true, result);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<OnlineSessionVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        // جست‌وجوی کاربر (نام یا موبایل) برای انتخاب طرف جلسه؛ فقط نماینده/اپراتور، حداقل ۳ کاراکتر و حداکثر ۱۰ نتیجه
        // تا از این endpoint برای استخراج انبوه اطلاعات کاربران استفاده نشود.
        public async Task<BaseResultDto<List<OnlineSessionUserVDto>>> SearchUsersAsync(string q)
        {
            try
            {
                var current = _currentUser.CurrentUser;
                if ((current.CompanionId ?? 0) <= 0 && !current.IsCompanionUser)
                    return new BaseResultDto<List<OnlineSessionUserVDto>>(false, Resource.Notification.AccessDenied, null);

                q = (await q.ToEnglishDigitsAsync())?.Trim();
                if (string.IsNullOrEmpty(q) || q.Length < MinSearchLength)
                    return new BaseResultDto<List<OnlineSessionUserVDto>>(true, new List<OnlineSessionUserVDto>());

                var users = await _context.Users
                    .AsNoTracking()
                    .Include(s => s.Picture)
                    .Where(s => !s.Deleted && !s.Locked && s.Id != current.UserId
                        && ((s.FirstName + " " + s.LastName).Contains(q) || s.Mobile.Contains(q)))
                    .OrderBy(s => s.Id)
                    .Take(MaxSearchResults)
                    .ToListAsync();

                return new BaseResultDto<List<OnlineSessionUserVDto>>(true, users.Select(ToUserVDto).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<OnlineSessionUserVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<OnlineSessionVDto>>> ActiveAsync()
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var since = DateTime.Now - ActiveWindow;
                var items = await _context.OnlineSessions
                    .AsNoTracking()
                    .Include(s => s.InitiatorUser)
                    .Include(s => s.TargetUser)
                    .Where(s => (s.InitiatorUserId == userId || s.TargetUserId == userId) && s.ChannelId == (int)OnlineSessionChannelEnum.Chat && !s.EndDate.HasValue && s.CreateDate > since)
                    .OrderByDescending(s => s.Id)
                    .Take(20)
                    .ToListAsync();

                return new BaseResultDto<List<OnlineSessionVDto>>(true, items.Select(ToVDto).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<OnlineSessionVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<OnlineSessionMessageVDto>>> MessagesAsync(OnlineSessionMessageInputDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var session = await _context.OnlineSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.OnlineSessionId);

                if (session == null)
                    return new BaseResultDto<List<OnlineSessionMessageVDto>>(false, Resource.Notification.NothingFound, null);

                if (!IsParticipant(session, userId))
                    return new BaseResultDto<List<OnlineSessionMessageVDto>>(false, Resource.Notification.AccessDenied, null);

                var pageSize = Math.Clamp(dto.PageSize, 1, MaxPageSize);
                var query = _context.OnlineSessionMessages.AsNoTracking()
                    .Where(s => s.OnlineSessionId == dto.OnlineSessionId && !s.Deleted);

                List<OnlineSessionMessageVDto> list;
                if (dto.AfterMessageId.HasValue)
                {
                    list = await query.Where(s => s.Id > dto.AfterMessageId.Value)
                        .OrderBy(s => s.Id).Take(pageSize).Select(ToMessageVDto).ToListAsync();
                }
                else
                {
                    if (dto.BeforeMessageId.HasValue)
                        query = query.Where(s => s.Id < dto.BeforeMessageId.Value);

                    list = await query.OrderByDescending(s => s.Id).Take(pageSize).Select(ToMessageVDto).ToListAsync();
                    list.Reverse();
                }

                return new BaseResultDto<List<OnlineSessionMessageVDto>>(true, list);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<OnlineSessionMessageVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<OnlineSessionMessageVDto>> SendMessageAsync(OnlineSessionMessageSendDto dto)
        {
            try
            {
                var current = _currentUser.CurrentUser;
                var content = dto.Content?.Trim();
                var imageUrl = dto.ImageUrl?.Trim();
                var imageThumbnailUrl = dto.ImageThumbnailUrl?.Trim();
                var hasImage = !string.IsNullOrEmpty(imageUrl);

                // پیام باید متن یا تصویر داشته باشد؛ کپشن تصویر اختیاری است
                if ((string.IsNullOrEmpty(content) && !hasImage) || (content?.Length ?? 0) > MaxContentLength)
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.CompanionReserveMessageContentRequired, null);

                // آدرس تصویر را کلاینت می‌فرستد و روی دستگاه طرف مقابل رندر می‌شود؛ مثل ضمیمه‌ی چت رزرو فقط دامنه‌ی خودمان مجاز است
                if (hasImage && (!AttachmentUrlPolicy.IsAllowed(imageUrl) || !AttachmentUrlPolicy.IsAllowedOptional(imageThumbnailUrl)))
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.InvalidData, null);

                var session = await _context.OnlineSessions.FirstOrDefaultAsync(s => s.Id == dto.OnlineSessionId);

                if (session == null)
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.NothingFound, null);

                // جلسه‌ی مشاوره‌ی مدت‌دار: بعد از پایان پنجره چت فقط‌خواندنی است (سرور، نه فقط UI؛ مستقل از تأخیر job)
                if (session.ExpireDate.HasValue && DateTime.Now >= session.ExpireDate.Value)
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.ConsultationWindowEnded, null);

                if (session.EndDate.HasValue)
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.NothingFound, null);

                if (!IsParticipant(session, current.UserId) || session.ChannelId != (int)OnlineSessionChannelEnum.Chat)
                    return new BaseResultDto<OnlineSessionMessageVDto>(false, Resource.Notification.AccessDenied, null);

                var message = new OnlineSessionMessage
                {
                    OnlineSessionId = session.Id,
                    SenderUserId = current.UserId,
                    Content = content,
                    ImageUrl = hasImage ? imageUrl : null,
                    ImageThumbnailUrl = hasImage && !string.IsNullOrEmpty(imageThumbnailUrl) ? imageThumbnailUrl : null,
                    CreateDate = DateTime.Now
                };
                await _context.OnlineSessionMessages.AddAsync(message);
                await _context.SaveChangesAsync();

                var receiverUserId = session.InitiatorUserId == current.UserId ? session.TargetUserId : session.InitiatorUserId;
                await _pushNotificationService.SendPushAsync(
                    PushTypeEnum.PushOnlineSessionNewMessage,
                    receiverUserId,
                    current.FullName,
                    // پیام فقط‌تصویر: متن پوش فقط یک آیکون است (متن فارسی داخل کد نمی‌آید)
                    string.IsNullOrEmpty(content) ? "📷" : (content.Length > 80 ? content.Substring(0, 80) : content),
                    session.Id.ToString());

                return new BaseResultDto<OnlineSessionMessageVDto>(true, ToMessageVDto.Compile()(message));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<OnlineSessionMessageVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> ReadAsync(OnlineSessionMessageReadDto dto)
        {
            try
            {
                var userId = _currentUser.CurrentUser.UserId;
                var session = await _context.OnlineSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.OnlineSessionId);

                if (session == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                if (!IsParticipant(session, userId))
                    return new BaseResultDto(false, Resource.Notification.AccessDenied);

                var now = DateTime.Now;
                var messages = await _context.OnlineSessionMessages
                    .Where(s => s.OnlineSessionId == dto.OnlineSessionId && s.Id <= dto.LastMessageId
                        && s.SenderUserId != userId && !s.Deleted && !s.ReadDate.HasValue)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.DeliveredDate ??= now;
                    message.ReadDate = now;
                }

                if (messages.Count > 0)
                {
                    // کانتکست پروژه پیش‌فرض NoTracking است (مثل CompanionReserveMessageService)، پس Update صریح لازم است.
                    _context.OnlineSessionMessages.UpdateRange(messages);
                    await _context.SaveChangesAsync();
                }

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        // کلینیکی که نماینده صاحب یا عضو فعال آن است (اولویت با کلینیکی که مالکش خود اوست)
        private async Task<OnlineSessionClinicVDto> FindClinicAsync(long userId)
        {
            var clinic = await _context.Companions
                .AsNoTracking()
                .Include(s => s.Picture)
                .Where(s => !s.Deleted && s.Active && s.Approved
                    && (s.OwnerId == userId
                        || s.CompanionUsers.Any(cu => cu.UserId == userId && !cu.Deleted && cu.Active && cu.UserAccept == true)))
                .OrderByDescending(s => s.OwnerId == userId)
                .ThenBy(s => s.Id)
                .FirstOrDefaultAsync();

            return clinic == null ? null : new OnlineSessionClinicVDto { Id = clinic.Id, Name = clinic.Name, Picture = ToPictureVDto(clinic.Picture) };
        }

        private async Task<List<OnlineSessionPetVDto>> FindPetsAsync(long userId)
        {
            var pets = await _context.UserPets
                .AsNoTracking()
                .Include(s => s.Picture)
                .Include(s => s.Pet)
                .Include(s => s.PetBreed)
                .Include(s => s.UserPetPictures).ThenInclude(p => p.Picture)
                .Where(s => s.UserId == userId && !s.Deleted && s.Active)
                .OrderBy(s => s.Id)
                .Take(MaxPets)
                .ToListAsync();

            return pets.Select(s => new OnlineSessionPetVDto
            {
                Id = s.Id,
                Name = s.Name,
                PetName = s.Pet?.Name,
                BreedName = s.PetBreed?.Name,
                Birthday = s.Birthday == default ? null : s.Birthday,
                Picture = ToPictureVDto(s.Picture),
                Gallery = s.UserPetPictures?
                    .Where(p => !p.Deleted && p.Picture != null)
                    .OrderBy(p => p.Id)
                    .Take(MaxGalleryPictures)
                    .Select(p => ToPictureVDto(p.Picture))
                    .ToList() ?? new List<PictureVDto>()
            }).ToList();
        }

        private static PictureVDto ToPictureVDto(Entities.Entities.Picture picture) => picture == null ? null : new PictureVDto
        {
            Id = picture.Id,
            // همان نگاشت AutoMapper (Picture → PictureVDto): BaseUrl پوشه است و Url مسیر کامل فایل
            BaseUrl = picture.Url,
            Url = picture.Url + "/" + picture.Name,
            OrginalName = picture.OrginalName,
            GuidName = picture.GuidName,
            Extension = picture.Extension
        };

        private static bool IsParticipant(OnlineSession session, long userId) =>
            session.InitiatorUserId == userId || session.TargetUserId == userId;

        private static OnlineSessionVDto ToVDto(OnlineSession item) => new()
        {
            Id = item.Id,
            ChannelId = item.ChannelId,
            CreateDate = item.CreateDate,
            EndDate = item.EndDate,
            ExpireDate = item.ExpireDate,
            ConsultationPurchaseId = item.ConsultationPurchaseId,
            ServerNow = DateTime.Now,
            InitiatorUser = ToUserVDto(item.InitiatorUser),
            TargetUser = ToUserVDto(item.TargetUser)
        };

        private static OnlineSessionUserVDto ToUserVDto(Entities.Entities.Security.User user) => user == null ? null : new()
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Mobile = user.Mobile,
            Picture = ToPictureVDto(user.Picture)
        };

        private static readonly Expression<Func<OnlineSessionMessage, OnlineSessionMessageVDto>> ToMessageVDto = s => new OnlineSessionMessageVDto
        {
            Id = s.Id,
            OnlineSessionId = s.OnlineSessionId,
            SenderUserId = s.SenderUserId,
            Content = s.Content,
            ImageUrl = s.ImageUrl,
            ImageThumbnailUrl = s.ImageThumbnailUrl,
            CreateDate = s.CreateDate,
            DeliveredDate = s.DeliveredDate,
            ReadDate = s.ReadDate
        };
    }
}
