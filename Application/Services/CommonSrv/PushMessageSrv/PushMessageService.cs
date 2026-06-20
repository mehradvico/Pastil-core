using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CommonSrv.NeighborhoodSrv.Dto;
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
            var item = await _context.PushMessages.Include(s => s.Picture).Include(s => s.PushMessageType).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
                return new BaseResultDto<PushMessageVDto>(true, _mapper.Map<PushMessageVDto>(item));
            return new BaseResultDto<PushMessageVDto>(false, _mapper.Map<PushMessageVDto>(item));
        }

        public PushMessageSearchDto Search(PushMessageInputDto dto)
        {
            var query = _context.PushMessages.Include(s => s.Picture).Include(s => s.PushMessageType).AsQueryable().Where(s => !s.Deleted);
            
            if (dto.PushMessageTypeId.HasValue)
            {
                query = query.Where(s => s.PushMessageTypeId == dto.PushMessageTypeId.Value);
            }
            return new PushMessageSearchDto(dto, query, _mapper);
        }
    }
}
