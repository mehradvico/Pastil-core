using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageSrv.Dto;
using AutoMapper;
using System.Linq;
using Entities.Entities.ClinicField;

namespace Application.Maping
{
    /// <summary>نگاشت‌های کلینیک (Clinic*)؛ جدا از AllMap تا آن فایل بزرگ‌تر نشود. Pointها از نگاشت مشترک AllMap می‌آیند.</summary>
    public class ClinicMap : Profile
    {
        public ClinicMap()
        {
            CreateMap<ClinicDto, Clinic>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));

            CreateMap<Clinic, ClinicMinVDto>();
            CreateMap<Clinic, ClinicVDto>()
                .ForMember(d => d.PetIds, o => o.Ignore())
                .ForMember(d => d.TypeIds, o => o.Ignore());
            CreateMap<Clinic, SearchClinicDto>();
            CreateMap<Entities.Entities.Security.User, ClinicOwnerVDto>();
            CreateMap<Entities.Entities.ProviderField.Provider, ClinicProviderVDto>();
            // گفتگوی رزرو کلینیک
            CreateMap<ClinicReserveMessage, ClinicReserveMessageDto>();
            CreateMap<ClinicReserveMessageDto, ClinicReserveMessage>()
                .ForMember(x => x.DeliveredDate, x => x.Ignore()).ForMember(x => x.ReadDate, x => x.Ignore())
                .ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.CreateDate, x => x.Ignore()).ForMember(x => x.ClinicReserve, x => x.Ignore())
                .ForMember(x => x.SenderUser, x => x.Ignore()).ForMember(x => x.ClinicReserveMessageType, x => x.Ignore()).ForMember(x => x.ReplyToMessage, x => x.Ignore())
                .ForMember(x => x.Attachments, x => x.Ignore()).ForMember(x => x.Reactions, x => x.Ignore());
            CreateMap<ClinicReserveMessage, ClinicReserveMessageVDto>()
                .ForMember(x => x.Attachments, x => x.MapFrom(s => s.Attachments.Where(a => !a.Deleted).OrderBy(a => a.Order)))
                .ForMember(x => x.Reactions, x => x.MapFrom(s => s.Reactions.Where(r => !r.Deleted)));
            CreateMap<ClinicReserveMessage, ClinicReserveMessageReplyVDto>().ForMember(x => x.Content, x => x.MapFrom(s => s.Deleted ? null : s.Content));
            CreateMap<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentItemVDto>();
            CreateMap<ClinicReserveMessageReaction, ClinicReserveMessageReactionItemVDto>();
            CreateMap<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentDto>();
            CreateMap<ClinicReserveMessageAttachmentDto, ClinicReserveMessageAttachment>().ForMember(x => x.Deleted, x => x.Ignore()).ForMember(x => x.ClinicReserveMessage, x => x.Ignore());
            CreateMap<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentVDto>();
            CreateMap<ClinicReserveMessageReaction, ClinicReserveMessageReactionDto>();
            CreateMap<ClinicReserveMessageReactionDto, ClinicReserveMessageReaction>()
                .ForMember(x => x.ReactorUserId, x => x.Ignore()).ForMember(x => x.Deleted, x => x.Ignore())
                .ForMember(x => x.ClinicReserveMessage, x => x.Ignore()).ForMember(x => x.ReactorUser, x => x.Ignore());
            CreateMap<ClinicReserveMessageReaction, ClinicReserveMessageReactionVDto>();
            CreateMap<ClinicServicePackage, Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto.ClinicServicePackageVDto>()
                .ForMember(d => d.Types, o => o.Ignore());
        }
    }
}
