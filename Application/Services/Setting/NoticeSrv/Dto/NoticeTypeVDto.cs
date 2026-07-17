using Application.Common.Dto.Field;
using Entities.Entities;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public class NoticeTypeVDto : Id_FieldDto
    {
        public string Label { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public NoticeImportance Importance { get; set; }
        public string NavigationTemplate { get; set; }
        public bool IsActive { get; set; }
        public bool ShowToast { get; set; }
        public bool SendPush { get; set; }
    }
}
