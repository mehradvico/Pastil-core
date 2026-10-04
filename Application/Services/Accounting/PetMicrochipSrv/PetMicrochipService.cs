using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Accounting.PetMicrochipSrv.Dto;
using Application.Services.Accounting.PetMicrochipSrv.Iface;
using Application.Services.CompanionSrvs.CompanionDayScheduleSrv;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetMicrochipSrv
{
    // جستجوی پت با میکروچیپ از «ارتباط با ما» + درخواست پیگیری توسط پاستیل. طراحی: backend/Docs/PET_MICROCHIP_LOOKUP_FA.md
    // قاعده‌ی امنیتی: هیچ مشخصات مالک (نام/موبایل/ایمیل/شناسه) و اطلاعات درمانی در پاسخ عمومی نیست؛ فقط ادمین مالک را می‌بیند.
    public class PetMicrochipService : IPetMicrochipService
    {
        private const int MaxFollowUpsPerMobilePerDay = 5;
        private const int MaxPictures = 6;

        private readonly IDataBaseContext _context;
        private readonly INoticeEventService _noticeService;

        public PetMicrochipService(IDataBaseContext context, INoticeEventService noticeService)
        {
            _context = context;
            _noticeService = noticeService;
        }

        public async Task<BaseResultDto<PetMicrochipSearchResultVDto>> SearchAsync(PetMicrochipSearchDto dto, long? userId, string clientIp)
        {
            try
            {
                var code = PetMicrochipRules.NormalizeMicrochip(dto?.MicrochipCode);
                if (code == null)
                    return new BaseResultDto<PetMicrochipSearchResultVDto>(false, Resource.Notification.MicrochipInvalid, null);

                var now = DateTime.Now;
                var pet = await _context.UserPets.AsNoTracking()
                    .Where(p => !p.Deleted && p.MicroChipCode != null
                        && p.MicroChipCode.Replace(" ", "").Replace("-", "") == code)
                    .OrderByDescending(p => p.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        PetName = p.Pet.Name,
                        BreedName = p.PetBreed != null ? p.PetBreed.Name : null,
                        Breed2Name = p.PetBreed2 != null ? p.PetBreed2.Name : null,
                        p.IsMixBreed,
                        p.IsMale,
                        p.IsSterile,
                        p.Birthday,
                        p.Size,
                        p.Weight,
                        p.Picture
                    })
                    .FirstOrDefaultAsync();
                var matchCount = pet == null ? 0 : await _context.UserPets.AsNoTracking()
                    .CountAsync(p => !p.Deleted && p.MicroChipCode != null && p.MicroChipCode.Replace(" ", "").Replace("-", "") == code);

                var request = new PetMicrochipRequest
                {
                    MicrochipCode = code,
                    PublicToken = pet == null ? null : PetMicrochipRules.NewToken(),
                    UserId = userId,
                    ClientIp = Truncate(clientIp, 64),
                    FoundUserPetId = pet?.Id,
                    MatchCount = matchCount,
                    CreateDate = now,
                    Status = (int)PetMicrochipRequestStatusEnum.Searched
                };
                _context.PetMicrochipRequests.Add(request);
                await _context.SaveChangesAsync();

                if (pet == null)
                    return new BaseResultDto<PetMicrochipSearchResultVDto>(true, new PetMicrochipSearchResultVDto { Found = false, MicrochipCode = code });

                var pictures = await GetPicturesAsync(pet.Id);
                return new BaseResultDto<PetMicrochipSearchResultVDto>(true, new PetMicrochipSearchResultVDto
                {
                    Found = true,
                    MicrochipCode = code,
                    Token = request.PublicToken,
                    TokenExpireDate = now.Add(PetMicrochipRules.TokenLifetime),
                    Pet = new PetMicrochipPublicPetVDto
                    {
                        Name = pet.Name,
                        PetName = pet.PetName,
                        BreedName = pet.BreedName,
                        Breed2Name = pet.IsMixBreed ? pet.Breed2Name : null,
                        IsMixBreed = pet.IsMixBreed,
                        IsMale = pet.IsMale,
                        IsSterile = pet.IsSterile,
                        AgeMonths = pet.Birthday == default ? (int?)null : CompanionDayScheduleRules.AgeMonths(pet.Birthday, now),
                        Size = pet.Size,
                        Weight = pet.Weight,
                        Picture = ToPicture(pet.Picture),
                        Pictures = pictures
                    }
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PetMicrochipSearchResultVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<PetMicrochipFollowUpResultVDto>> RequestFollowUpAsync(PetMicrochipFollowUpDto dto, long? userId)
        {
            try
            {
                var token = dto?.Token?.Trim();
                if (string.IsNullOrEmpty(token))
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipTokenInvalid, null);

                var request = await _context.PetMicrochipRequests.FirstOrDefaultAsync(r => r.PublicToken == token && r.FoundUserPetId != null);
                var now = DateTime.Now;
                if (request == null || !PetMicrochipRules.IsTokenFresh(request.CreateDate, now))
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipTokenInvalid, null);
                if (request.FollowUpRequested)
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipFollowUpAlready, null);

                var fullName = await SanitizeTextHelper.ToSanitizeAsync(dto.FullName?.Trim());
                var mobile = PetMicrochipRules.NormalizeMobile(dto.Mobile);
                var email = dto.Email?.Trim();
                if (userId.HasValue && (string.IsNullOrWhiteSpace(fullName) || mobile == null))
                {
                    var me = await _context.Users.AsNoTracking().Where(u => u.Id == userId.Value)
                        .Select(u => new { u.FirstName, u.LastName, u.Mobile, u.Email }).FirstOrDefaultAsync();
                    if (me != null)
                    {
                        if (string.IsNullOrWhiteSpace(fullName))
                            fullName = ((me.FirstName ?? "") + " " + (me.LastName ?? "")).Trim();
                        mobile ??= PetMicrochipRules.NormalizeMobile(me.Mobile);
                        if (string.IsNullOrWhiteSpace(email)) email = me.Email;
                    }
                }
                if (string.IsNullOrWhiteSpace(fullName))
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipFollowUpNameRequired, null);
                if (mobile == null)
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipMobileInvalid, null);

                var since = now.AddDays(-1);
                var recent = await _context.PetMicrochipRequests.AsNoTracking()
                    .CountAsync(r => r.FollowUpRequested && r.Mobile == mobile && r.FollowUpDate >= since);
                if (recent >= MaxFollowUpsPerMobilePerDay)
                    return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, Resource.Notification.MicrochipFollowUpLimit, null);

                request.FollowUpRequested = true;
                request.FollowUpDate = now;
                request.FullName = Truncate(fullName, 100);
                request.Mobile = mobile;
                request.Email = Truncate(email, 200);
                request.Message = Truncate(await SanitizeTextHelper.ToSanitizeAsync(dto.Message?.Trim()), 1000);
                request.UserId ??= userId;
                request.Status = (int)PetMicrochipRequestStatusEnum.FollowUpRequested;
                await _context.SaveChangesAsync();

                try
                {
                    // اعلان + پوش به ادمین‌ها؛ شکست اعلان نباید ثبت درخواست را خراب کند
                    await _noticeService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.PetMicrochipFollowUpRequested,
                        ActorUserId = request.UserId,
                        ReferenceType = "PetMicrochipRequest",
                        ReferenceId = request.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.PetMicrochipFollowUpRequested}:{request.Id}",
                        Metadata = new Dictionary<string, string> { ["microchipCode"] = request.MicrochipCode, ["requesterName"] = request.FullName ?? string.Empty }
                    });
                }
                catch (Exception) { }

                return new BaseResultDto<PetMicrochipFollowUpResultVDto>(true, Resource.Notification.MicrochipFollowUpDone,
                    new PetMicrochipFollowUpResultVDto { RequestId = request.Id, FollowUpDate = now });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PetMicrochipFollowUpResultVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<PetMicrochipRequestSearchVDto>> AdminSearchAsync(PetMicrochipRequestInputDto dto)
        {
            try
            {
                dto ??= new PetMicrochipRequestInputDto();
                var query = _context.PetMicrochipRequests.AsNoTracking().AsQueryable();
                if (dto.Status.HasValue)
                    query = query.Where(r => r.Status == dto.Status.Value);
                else if (!dto.IncludeSearches)
                    query = query.Where(r => r.Status != (int)PetMicrochipRequestStatusEnum.Searched);

                var code = PetMicrochipRules.NormalizeMicrochip(dto.MicrochipCode);
                if (code != null) query = query.Where(r => r.MicrochipCode == code);
                else if (!string.IsNullOrWhiteSpace(dto.Q))
                {
                    var q = dto.Q.Trim();
                    query = query.Where(r => r.MicrochipCode.Contains(q) || r.FullName.Contains(q) || r.Mobile.Contains(q));
                }

                var pageIndex = Math.Max(1, dto.PageIndex);
                var pageSize = Math.Clamp(dto.PageSize, 1, 100);
                var total = await query.CountAsync();
                query = dto.SortBy == SortEnum.Old ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id);
                var list = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                    .Select(r => new PetMicrochipRequestListItemVDto
                    {
                        Id = r.Id,
                        MicrochipCode = r.MicrochipCode,
                        Status = r.Status,
                        Found = r.FoundUserPetId != null,
                        FollowUpRequested = r.FollowUpRequested,
                        PetName = r.FoundUserPet != null ? r.FoundUserPet.Name : null,
                        RequesterName = r.FullName,
                        RequesterMobile = r.Mobile,
                        CreateDate = r.CreateDate,
                        FollowUpDate = r.FollowUpDate
                    }).ToListAsync();

                return new BaseResultDto<PetMicrochipRequestSearchVDto>(true, new PetMicrochipRequestSearchVDto
                {
                    TotalCount = total,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    List = list
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PetMicrochipRequestSearchVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<PetMicrochipRequestDetailVDto>> AdminGetAsync(long id)
        {
            try
            {
                var r = await _context.PetMicrochipRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (r == null)
                    return new BaseResultDto<PetMicrochipRequestDetailVDto>(false, Resource.Notification.MicrochipRequestNotFound, null);

                var detail = new PetMicrochipRequestDetailVDto
                {
                    Id = r.Id,
                    MicrochipCode = r.MicrochipCode,
                    Status = r.Status,
                    MatchCount = r.MatchCount,
                    CreateDate = r.CreateDate,
                    ClientIp = r.ClientIp,
                    RequesterUserId = r.UserId,
                    FollowUpRequested = r.FollowUpRequested,
                    FollowUpDate = r.FollowUpDate,
                    RequesterName = r.FullName,
                    RequesterMobile = r.Mobile,
                    RequesterEmail = r.Email,
                    Message = r.Message,
                    AdminNote = r.AdminNote,
                    ClosedDate = r.ClosedDate,
                    FoundUserPetId = r.FoundUserPetId,
                    AllowedNextStatuses = PetMicrochipRules.AllowedNext(r.Status)
                };

                if (r.FoundUserPetId.HasValue)
                {
                    var p = await _context.UserPets.AsNoTracking().Where(x => x.Id == r.FoundUserPetId.Value)
                        .Select(x => new
                        {
                            x.Id, x.Name, PetName = x.Pet.Name,
                            BreedName = x.PetBreed != null ? x.PetBreed.Name : null,
                            Breed2Name = x.PetBreed2 != null ? x.PetBreed2.Name : null,
                            x.IsMixBreed, x.IsMale, x.IsSterile, x.Birthday, x.Size, x.Weight, x.Picture,
                            OwnerId = x.UserId, x.User.FirstName, x.User.LastName, x.User.Mobile, x.User.Email
                        }).FirstOrDefaultAsync();
                    if (p != null)
                    {
                        detail.Pet = new PetMicrochipPublicPetVDto
                        {
                            Name = p.Name, PetName = p.PetName, BreedName = p.BreedName,
                            Breed2Name = p.IsMixBreed ? p.Breed2Name : null,
                            IsMixBreed = p.IsMixBreed, IsMale = p.IsMale, IsSterile = p.IsSterile,
                            AgeMonths = p.Birthday == default ? (int?)null : CompanionDayScheduleRules.AgeMonths(p.Birthday, DateTime.Now),
                            Size = p.Size, Weight = p.Weight,
                            Picture = ToPicture(p.Picture),
                            Pictures = await GetPicturesAsync(p.Id)
                        };
                        detail.Owner = new PetMicrochipOwnerVDto
                        {
                            UserId = p.OwnerId,
                            FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim(),
                            Mobile = p.Mobile,
                            Email = p.Email
                        };
                    }
                }
                return new BaseResultDto<PetMicrochipRequestDetailVDto>(true, detail);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<PetMicrochipRequestDetailVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> AdminUpdateAsync(long id, PetMicrochipRequestUpdateDto dto, long adminUserId)
        {
            try
            {
                if (dto == null || !PetMicrochipRules.IsValidStatus(dto.Status))
                    return new BaseResultDto(false, Resource.Notification.MicrochipStatusInvalid);
                var r = await _context.PetMicrochipRequests.FirstOrDefaultAsync(x => x.Id == id);
                if (r == null)
                    return new BaseResultDto(false, Resource.Notification.MicrochipRequestNotFound);

                var note = Truncate(await SanitizeTextHelper.ToSanitizeAsync(dto.AdminNote?.Trim()), 1000);
                if (dto.Status != r.Status)
                {
                    if (!PetMicrochipRules.CanTransition(r.Status, dto.Status))
                        return new BaseResultDto(false, Resource.Notification.MicrochipStatusTransitionNotAllowed);
                    r.Status = dto.Status;
                    r.ClosedDate = PetMicrochipRules.IsTerminal(dto.Status) ? DateTime.Now : (DateTime?)null;
                }
                if (note != null) r.AdminNote = note;
                r.HandledByUserId = adminUserId;
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        private async Task<List<PictureVDto>> GetPicturesAsync(long userPetId)
        {
            var pics = await _context.UserPetPictures.AsNoTracking()
                .Where(x => x.UserPetId == userPetId && !x.Deleted)
                .OrderBy(x => x.Id).Take(MaxPictures)
                .Select(x => x.Picture).ToListAsync();
            return pics.Where(p => p != null).Select(ToPicture).ToList();
        }

        private static PictureVDto ToPicture(Picture picture) => picture == null ? null : new PictureVDto
        {
            Id = picture.Id,
            BaseUrl = picture.Url,
            Url = picture.Url + "/" + picture.Name,
            OrginalName = picture.OrginalName,
            GuidName = picture.GuidName,
            Extension = picture.Extension
        };

        private static string Truncate(string s, int max) => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max));
    }
}
