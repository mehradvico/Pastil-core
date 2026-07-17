using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv
{
    public class NoticeService : CommonSrv<Notice, NoticeDto>, INoticeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserHelper _currentUser;
        private readonly INoticeEventService _noticeEventService;

        public NoticeService(IDataBaseContext context, IMapper mapper, ICurrentUserHelper currentUser, INoticeEventService noticeEventService) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
            _noticeEventService = noticeEventService;
        }

        public NoticeSearchDto Search(NoticeInputDto dto)
        {
            EnsureAdmin();
            var now = DateTime.UtcNow;
            var model = NoticeQuery();
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var query = dto.Q.Trim();
                model = model.Where(x => x.Title.Contains(query) || x.Message.Contains(query) || x.NoticeType.Label.Contains(query) || x.NoticeType.Name.Contains(query) || (x.ReferenceType != null && x.ReferenceType.Contains(query)) || (x.MetadataJson != null && x.MetadataJson.Contains(query)) || (x.ActorUser != null && (x.ActorUser.FirstName.Contains(query) || x.ActorUser.LastName.Contains(query) || x.ActorUser.Mobile.Contains(query))) || (x.Read != null && x.Read.AdminNameSnapshot.Contains(query)));
            }
            if (dto.ActorUserId.HasValue)
                model = model.Where(x => x.ActorUserId == dto.ActorUserId.Value);
            if (dto.ReadByAdminId.HasValue)
                model = model.Where(x => x.Read != null && x.Read.AdminId == dto.ReadByAdminId.Value);
            if (dto.NoticeTypeId.HasValue)
                model = model.Where(x => x.NoticeTypeId == dto.NoticeTypeId.Value);
            if (dto.Importance.HasValue)
                model = model.Where(x => x.NoticeType.Importance == dto.Importance.Value);
            if (!string.IsNullOrWhiteSpace(dto.ReferenceType))
                model = model.Where(x => x.ReferenceType == dto.ReferenceType);
            if (dto.ReferenceId.HasValue)
                model = model.Where(x => x.ReferenceId == dto.ReferenceId.Value);
            if (dto.IsRead.HasValue)
                model = dto.IsRead.Value ? model.Where(x => x.Read != null) : model.Where(x => x.Read == null);
            if (dto.IsArchived.HasValue)
                model = dto.IsArchived.Value ? model.Where(x => x.ArchivedAtUtc.HasValue || x.ArchiveDueAtUtc <= now) : model.Where(x => !x.ArchivedAtUtc.HasValue && x.ArchiveDueAtUtc > now);
            if (dto.FromDateUtc.HasValue)
                model = model.Where(x => x.CreateDateUtc >= dto.FromDateUtc.Value);
            if (dto.ToDateUtc.HasValue)
                model = model.Where(x => x.CreateDateUtc <= dto.ToDateUtc.Value);
            model = dto.SortBy == SortEnum.Old ? model.OrderBy(x => x.CreateDateUtc).ThenBy(x => x.Id) : model.OrderByDescending(x => x.CreateDateUtc).ThenByDescending(x => x.Id);
            return new NoticeSearchDto(dto, model, _mapper);
        }

        public override async Task<BaseResultDto<NoticeDto>> FindAsyncDto(long id)
        {
            EnsureAdmin();
            var item = await NoticeQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return new BaseResultDto<NoticeDto>(false, null);
            return new BaseResultDto<NoticeDto>(true, _mapper.Map<NoticeDto>(item));
        }

        public async Task<BaseResultDto<NoticeDto>> ReadAsync(long id)
        {
            EnsureAdmin();
            var item = await NoticeQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
                return new BaseResultDto<NoticeDto>(false, null);
            if (item.Read == null)
            {
                await SaveReadsAsync(new List<long> { id }, NoticeReadMode.Single);
                item = await NoticeQuery().FirstOrDefaultAsync(x => x.Id == id);
            }
            return new BaseResultDto<NoticeDto>(true, _mapper.Map<NoticeDto>(item));
        }

        public Task<BaseResultDto<NoticeDto>> CreateAsync(NoticeCreateDto dto)
        {
            if (dto != null && !dto.ActorUserId.HasValue)
                dto.ActorUserId = _currentUser.CurrentUser?.UserId;
            return _noticeEventService.CreateAsync(dto);
        }

        public async Task<BaseResultDto<NoticeBulkReadVDto>> ReadBulkAsync(NoticeBulkReadDto dto)
        {
            EnsureAdmin();
            if (dto == null || !dto.Confirmed)
                return new BaseResultDto<NoticeBulkReadVDto>(false, "Bulk read confirmation is required.", null);
            var now = DateTime.UtcNow;
            var requestedIds = dto.All ? await _context.Notices.Where(x => !x.ArchivedAtUtc.HasValue && x.ArchiveDueAtUtc > now).Select(x => x.Id).ToListAsync() : (dto.NoticeIds ?? new List<long>()).Where(x => x > 0).Distinct().ToList();
            if (requestedIds.Count == 0)
                return dto.All ? new BaseResultDto<NoticeBulkReadVDto>(true, new NoticeBulkReadVDto { AdminName = GetAdminName() }) : new BaseResultDto<NoticeBulkReadVDto>(false, "NoticeIds is required.", null);
            var validIds = await _context.Notices.Where(x => requestedIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
            var readCount = await SaveReadsAsync(validIds, NoticeReadMode.BulkConfirmed);
            var result = new NoticeBulkReadVDto
            {
                RequestedCount = requestedIds.Count,
                ReadCount = readCount,
                AlreadyReadCount = validIds.Count - readCount,
                NotFoundCount = requestedIds.Count - validIds.Count,
                AdminName = GetAdminName()
            };
            return new BaseResultDto<NoticeBulkReadVDto>(true, result);
        }

        public async Task<List<NoticeTypeVDto>> GetTypesAsync(bool activeOnly = true)
        {
            EnsureAdmin();
            var query = _context.NoticeTypes.AsQueryable();
            if (activeOnly)
                query = query.Where(x => x.IsActive);
            return _mapper.Map<List<NoticeTypeVDto>>(await query.OrderBy(x => x.Importance).ThenBy(x => x.Id).ToListAsync());
        }

        public Task<int> GetUnreadCountAsync()
        {
            EnsureAdmin();
            var now = DateTime.UtcNow;
            return _context.Notices.CountAsync(x => x.Read == null && !x.ArchivedAtUtc.HasValue && x.ArchiveDueAtUtc > now);
        }

        public Task<int> ArchiveExpiredAsync()
        {
            var now = DateTime.UtcNow;
            return _context.Notices.Where(x => !x.ArchivedAtUtc.HasValue && x.ArchiveDueAtUtc <= now).ExecuteUpdateAsync(x => x.SetProperty(p => p.ArchivedAtUtc, now));
        }

        public override Task<BaseResultDto<NoticeDto>> InsertAsyncDto(NoticeDto dto)
        {
            return Task.FromResult(new BaseResultDto<NoticeDto>(false, "Use CreateAsync for notices.", dto));
        }

        public override BaseResultDto UpdateDto(NoticeDto dto) => new BaseResultDto(false, "Notices are immutable.");
        public override BaseResultDto UpdateRangeDto(List<NoticeDto> dtoList) => new BaseResultDto(false, "Notices are immutable.");
        public override BaseResultDto DeleteDto(long id) => new BaseResultDto(false, "Notices must be archived.");
        public override BaseResultDto DeleteDto(NoticeDto dto) => new BaseResultDto(false, "Notices must be archived.");

        private async Task<int> SaveReadsAsync(List<long> noticeIds, NoticeReadMode readMode)
        {
            if (noticeIds.Count == 0)
                return 0;
            var admin = _currentUser.CurrentUser;
            var adminName = GetAdminName();
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var readIds = await _context.NoticeReads.Where(x => noticeIds.Contains(x.NoticeId)).Select(x => x.NoticeId).ToListAsync();
                var unreadIds = noticeIds.Except(readIds).ToList();
                if (unreadIds.Count == 0)
                    return 0;
                var reads = unreadIds.Select(x => new NoticeRead { NoticeId = x, AdminId = admin.UserId, AdminNameSnapshot = adminName, ReadAtUtc = DateTime.UtcNow, ReadMode = readMode }).ToList();
                await _context.NoticeReads.AddRangeAsync(reads);
                try
                {
                    await _context.SaveChangesAsync();
                    return reads.Count;
                }
                catch (DbUpdateException ex) when (attempt == 0 && IsUniqueConstraintViolation(ex))
                {
                    foreach (var read in reads)
                        _context.Entry(read).State = EntityState.Detached;
                }
            }
            return 0;
        }

        private IQueryable<Notice> NoticeQuery()
        {
            return _context.Notices.Include(x => x.NoticeType).Include(x => x.ActorUser).Include(x => x.Read).ThenInclude(x => x.Admin);
        }

        private string GetAdminName()
        {
            var admin = _currentUser.CurrentUser;
            return string.IsNullOrWhiteSpace(admin.FullName) ? $"{admin.FirstName} {admin.LastName}".Trim() : admin.FullName;
        }

        private void EnsureAdmin()
        {
            if (_currentUser.CurrentUser?.RoleEnum != RoleEnum.Admin.ToString())
                throw new UnauthorizedAccessException("Notice access is restricted to admins.");
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}
