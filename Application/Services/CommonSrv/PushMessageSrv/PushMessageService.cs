using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Common.Helpers;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Entities.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebPush;

namespace Application.Services.CommonSrv.PushBroadcastSrv
{
    public class PushMessageService : CommonSrv<PushMessage, PushMessageDto>, IPushMessageService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;

        public PushMessageService(IDataBaseContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public virtual async Task<BaseResultDto<PushMessageVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PushMessages.Include(s => s.Picture).Include(s => s.PushMessageType).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<PushMessageVDto>(true, _mapper.Map<PushMessageVDto>(item));
            return new BaseResultDto<PushMessageVDto>(false, _mapper.Map<PushMessageVDto>(item));
        }

        public override async Task<BaseResultDto<PushMessageDto>> InsertAsyncDto(PushMessageDto dto)
        {
            var validation = ValidatePersianText(dto) ?? ValidateSchedule(dto);
            if (validation != null)
                return new BaseResultDto<PushMessageDto>(false, validation, dto);

            return await base.InsertAsyncDto(dto);
        }

        public override BaseResultDto UpdateDto(PushMessageDto dto)
        {
            var validation = ValidatePersianText(dto) ?? ValidateSchedule(dto);
            if (validation != null)
                return new BaseResultDto(false, validation);

            // base.UpdateDto maps the DTO straight onto a brand-new tracked entity and
            // marks the whole row Modified, so LastSentDate (not on the writable DTO)
            // is always cleared by it. That's exactly what we want when the schedule
            // itself changed - the next due occurrence should fire fresh. But editing
            // only the title/body of an already-fired schedule must NOT re-trigger an
            // immediate resend, so restore the previous LastSentDate whenever none of
            // the schedule-defining fields actually changed.
            var existing = _context.PushMessages.AsNoTracking().FirstOrDefault(x => x.Id == dto.Id);
            var scheduleUnchanged = existing != null
                && existing.SendDate == dto.SendDate
                && existing.AutoSend == dto.AutoSend
                && existing.RecurrenceType == (int)dto.RecurrenceType;

            var result = base.UpdateDto(dto);

            if (result.IsSuccess && scheduleUnchanged && existing.LastSentDate.HasValue)
            {
                var tracked = _context.PushMessages.AsTracking().FirstOrDefault(x => x.Id == dto.Id);
                if (tracked != null)
                {
                    tracked.LastSentDate = existing.LastSentDate;
                    _context.PushMessages.Update(tracked);
                    _context.SaveChanges();
                }
            }

            return result;
        }

        public PushMessageSearchDto Search(PushMessageInputDto dto)
        {
            var query = _context.PushMessages.Include(s => s.Picture).Include(s => s.PushMessageType).Include(s => s.User).AsQueryable().Where(s => !s.Deleted);
            
            if (dto.PushMessageTypeId.HasValue)
            {
                query = query.Where(s => s.PushMessageTypeId == dto.PushMessageTypeId.Value);
            }
            return new PushMessageSearchDto(dto, query, _mapper);
        }

        private static string ValidatePersianText(PushMessageDto dto)
        {
            if (dto == null)
                return Resource.Notification.PushMessageDataNotProvided;

            if (!PersianPushTextHelper.ContainsPersian(dto.Title))
                return Resource.Notification.PushMessageTitleMustBePersian;

            if (!PersianPushTextHelper.ContainsPersian(dto.Body))
                return Resource.Notification.PushMessageBodyMustBePersian;

            return null;
        }

        private static string ValidateSchedule(PushMessageDto dto)
        {
            if (!dto.AutoSend)
                return null;

            if (dto.RecurrenceType == Application.Common.Enumerable.Code.PushRecurrenceEnum.None)
                return Resource.Notification.PushMessageAutoSendRequiresRecurrenceType;

            if (!dto.SendDate.HasValue)
                return Resource.Notification.PushMessageAutoSendRequiresSendDate;

            return null;
        }
    }
}
