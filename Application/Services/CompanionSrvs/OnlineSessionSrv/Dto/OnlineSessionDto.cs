using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.OnlineSessionSrv.Dto
{
    public class OnlineSessionStartDto
    {
        public long TargetUserId { get; set; }
        public int ChannelId { get; set; }
    }

    public class OnlineSessionUserVDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public PictureVDto Picture { get; set; }
    }

    // کلینیک نماینده‌ی شروع‌کننده؛ به کاربر به‌جای عکس شخصی نماینده نمایش داده می‌شود
    public class OnlineSessionClinicVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public PictureVDto Picture { get; set; }
    }

    // پت کاربر برای نماینده: عکس اصلی + گالری
    public class OnlineSessionPetVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PetName { get; set; }
        public string BreedName { get; set; }
        public DateTime? Birthday { get; set; }
        public PictureVDto Picture { get; set; }
        public List<PictureVDto> Gallery { get; set; }
    }

    public class OnlineSessionVDto
    {
        public long Id { get; set; }
        public int ChannelId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
        public OnlineSessionUserVDto InitiatorUser { get; set; }
        public OnlineSessionUserVDto TargetUser { get; set; }
        public OnlineSessionClinicVDto InitiatorClinic { get; set; }
        // جلسه‌ی مشاوره‌ی مدت‌دار: پایان پنجره (نال = جلسه‌ی آزاد) و ساعت سرور برای شمارش معکوس هم‌گام
        public DateTime? ExpireDate { get; set; }
        public long? ConsultationPurchaseId { get; set; }
        public DateTime ServerNow { get; set; }
        // فقط برای شروع‌کننده‌ی جلسه (نماینده) پر می‌شود
        public List<OnlineSessionPetVDto> TargetPets { get; set; }
    }

    public class OnlineSessionMessageSendDto
    {
        public long OnlineSessionId { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ImageThumbnailUrl { get; set; }
    }

    public class OnlineSessionMessageReadDto
    {
        public long OnlineSessionId { get; set; }
        public long LastMessageId { get; set; }
    }

    public class OnlineSessionMessageVDto
    {
        public long Id { get; set; }
        public long OnlineSessionId { get; set; }
        public long SenderUserId { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ImageThumbnailUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class OnlineSessionMessageInputDto
    {
        public long OnlineSessionId { get; set; }
        public long? AfterMessageId { get; set; }
        public long? BeforeMessageId { get; set; }
        public int PageSize { get; set; } = 30;
    }
}
