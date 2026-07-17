using Entities.Entities.CommonField;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Entities
{
    public class NoticeType : Name_Field
    {
        public string Label { get; set; }

        public string Title { get; set; }

        public NoticeImportance Importance { get; set; }

        public string NavigationTemplate { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Notice> Notices { get; set; } = new List<Notice>();

        [NotMapped]
        public bool ShowToast =>
            Importance == NoticeImportance.Important ||
            Importance == NoticeImportance.Critical;

        [NotMapped]
        public bool SendPush => Importance == NoticeImportance.Critical;
    }
}
