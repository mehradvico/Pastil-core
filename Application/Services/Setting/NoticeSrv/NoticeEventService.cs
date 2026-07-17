using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv
{
    public class NoticeEventService : INoticeEventService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        private readonly INoticeRealtimePublisher _realtimePublisher;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<NoticeEventService> _logger;

        public NoticeEventService(IDataBaseContext context, IMapper mapper, INoticeRealtimePublisher realtimePublisher, IPushNotificationService pushNotificationService, ILogger<NoticeEventService> logger)
        {
            _context = context;
            _mapper = mapper;
            _realtimePublisher = realtimePublisher;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        public async Task<BaseResultDto<NoticeDto>> CreateAsync(NoticeCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Label))
                return new BaseResultDto<NoticeDto>(false, "Notice label is required.", null);
            var noticeType = await _context.NoticeTypes.FirstOrDefaultAsync(x => x.Label == dto.Label && x.IsActive);
            if (noticeType == null)
                return new BaseResultDto<NoticeDto>(false, "Active notice type was not found.", null);
            var metadata = GetMetadata(dto);
            var metadataJson = JsonSerializer.Serialize(metadata);
            var deduplicationKey = GetDeduplicationKey(dto, metadataJson);
            var duplicate = await NoticeQuery().FirstOrDefaultAsync(x => x.DeduplicationKey == deduplicationKey);
            if (duplicate != null)
                return new BaseResultDto<NoticeDto>(true, _mapper.Map<NoticeDto>(duplicate));
            var now = DateTime.UtcNow;
            var notice = new Notice
            {
                NoticeTypeId = noticeType.Id,
                ActorUserId = dto.ActorUserId,
                ReferenceType = string.IsNullOrWhiteSpace(dto.ReferenceType) ? dto.Label : dto.ReferenceType.Trim(),
                ReferenceId = dto.ReferenceId,
                Title = Render(noticeType.Title, metadata),
                Message = Render(noticeType.Name, metadata),
                NavigationUrl = Render(noticeType.NavigationTemplate, metadata),
                MetadataJson = metadataJson,
                DeduplicationKey = deduplicationKey,
                CreateDateUtc = now,
                ArchiveDueAtUtc = now.AddDays(7)
            };
            await _context.Notices.AddAsync(notice);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(notice).State = EntityState.Detached;
                duplicate = await NoticeQuery().FirstOrDefaultAsync(x => x.DeduplicationKey == deduplicationKey);
                return new BaseResultDto<NoticeDto>(duplicate != null, _mapper.Map<NoticeDto>(duplicate));
            }
            var created = await NoticeQuery().FirstAsync(x => x.Id == notice.Id);
            await DispatchAsync(created);
            return new BaseResultDto<NoticeDto>(true, _mapper.Map<NoticeDto>(created));
        }

        private async Task DispatchAsync(Notice notice)
        {
            if (notice.NoticeType.Importance == NoticeImportance.Normal)
                return;
            var dto = _mapper.Map<NoticeVDto>(notice);
            try
            {
                await _realtimePublisher.PublishAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Realtime notice delivery failed for NoticeId {NoticeId}", notice.Id);
            }
            if (notice.NoticeType.Importance != NoticeImportance.Critical)
                return;
            try
            {
                await _pushNotificationService.SendNoticeToAdminsAsync(notice.Id, notice.Title, notice.Message, notice.NavigationUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push notice delivery failed for NoticeId {NoticeId}", notice.Id);
            }
        }

        private IQueryable<Notice> NoticeQuery()
        {
            return _context.Notices.Include(x => x.NoticeType).Include(x => x.ActorUser).Include(x => x.Read).ThenInclude(x => x.Admin);
        }

        private static Dictionary<string, string> GetMetadata(NoticeCreateDto dto)
        {
            var metadata = new Dictionary<string, string>(dto.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            if (dto.ReferenceId.HasValue)
                metadata["referenceId"] = dto.ReferenceId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(dto.ReferenceType))
                metadata["referenceType"] = dto.ReferenceType.Trim();
            return metadata;
        }

        private static string GetDeduplicationKey(NoticeCreateDto dto, string metadataJson)
        {
            if (!string.IsNullOrWhiteSpace(dto.DeduplicationKey))
            {
                var key = dto.DeduplicationKey.Trim();
                return key.Length <= 450 ? key : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
            }
            var source = $"{dto.Label}:{dto.ReferenceType}:{dto.ReferenceId}:{metadataJson}";
            return $"{dto.Label}:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))}";
        }

        private static string Render(string template, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;
            foreach (var item in metadata)
                template = template.Replace($"{{{item.Key}}}", item.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            return template;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}
