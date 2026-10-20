using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageSrv.Dto;
using AutoMapper;
using System.Linq;
using Entities.Entities.CoachField;

namespace Application.Maping
{
    /// <summary>نگاشت‌های مربی (Coach*)؛ جدا از AllMap تا آن فایل بزرگ‌تر نشود. Pointها از نگاشت مشترک AllMap می‌آیند.</summary>
    public class CoachMap : Profile
    {
        public CoachMap()
        {
            CreateMap<CoachDto, Coach>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<Coach, CoachMinVDto>();
            CreateMap<Coach, CoachVDto>()
                .ForMember(d => d.PetIds, o => o.Ignore());
            CreateMap<Coach, SearchCoachDto>();
            CreateMap<Entities.Entities.Security.User, CoachOwnerVDto>();
            CreateMap<Entities.Entities.ProviderField.Provider, CoachProviderVDto>();
            // گفتگوی رزرو مربی
            CreateMap<CoachReserveMessage, CoachReserveMessageDto>();
            CreateMap<CoachReserveMessageDto, CoachReserveMessage>()
                .ForMember(x => x.DeliveredDate, x => x.Ignore()).ForMember(x => x.ReadDate, x => x.Ignore())
                .ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.CreateDate, x => x.Ignore()).ForMember(x => x.CoachReserve, x => x.Ignore())
                .ForMember(x => x.SenderUser, x => x.Ignore()).ForMember(x => x.CoachReserveMessageType, x => x.Ignore()).ForMember(x => x.ReplyToMessage, x => x.Ignore())
                .ForMember(x => x.Attachments, x => x.Ignore()).ForMember(x => x.Reactions, x => x.Ignore());
            CreateMap<CoachReserveMessage, CoachReserveMessageVDto>()
                .ForMember(x => x.Attachments, x => x.MapFrom(s => s.Attachments.Where(a => !a.Deleted).OrderBy(a => a.Order)))
                .ForMember(x => x.Reactions, x => x.MapFrom(s => s.Reactions.Where(r => !r.Deleted)));
            CreateMap<CoachReserveMessage, CoachReserveMessageReplyVDto>().ForMember(x => x.Content, x => x.MapFrom(s => s.Deleted ? null : s.Content));
            CreateMap<CoachReserveMessageAttachment, CoachReserveMessageAttachmentItemVDto>();
            CreateMap<CoachReserveMessageReaction, CoachReserveMessageReactionItemVDto>();
            CreateMap<CoachReserveMessageAttachment, CoachReserveMessageAttachmentDto>();
            CreateMap<CoachReserveMessageAttachmentDto, CoachReserveMessageAttachment>().ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.CoachReserveMessage, x => x.Ignore());
            CreateMap<CoachReserveMessageAttachment, CoachReserveMessageAttachmentVDto>();
            CreateMap<CoachReserveMessageReaction, CoachReserveMessageReactionDto>();
            CreateMap<CoachReserveMessageReactionDto, CoachReserveMessageReaction>()
                .ForMember(x => x.ReactorUserId, x => x.Ignore()).ForMember(x => x.Deleted, x => x.Ignore())
                .ForMember(x => x.CoachReserveMessage, x => x.Ignore()).ForMember(x => x.ReactorUser, x => x.Ignore());
            CreateMap<CoachReserveMessageReaction, CoachReserveMessageReactionVDto>();
            CreateMap<CoachServicePackage, Application.Services.CoachSrvs.CoachCatalogSrv.Dto.CoachServicePackageVDto>()
                .ForMember(d => d.Types, o => o.Ignore());
        }
    }
}
