using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.CoachField
{
    /// <summary>بسته‌ی قیمتی یک خدمت (بر اساس سایز حیوان). جایگزین CompanionAssistancePackage.</summary>
    public class CoachServicePackage : Name_Field
    {
        public long CoachServiceId { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public string Discription { get; set; }
        /// <summary>سایز حیوان (Small/Medium/Large، هم‌ارز UserPet.Size)؛ خالی = همه‌ی سایزها.</summary>
        public string PetSize { get; set; }
        public bool Deleted { get; set; }

        public CoachService CoachService { get; set; }
        public Picture Picture { get; set; }
        public ICollection<CoachServicePackagePicture> Pictures { get; set; }
        public ICollection<CoachServicePackageType> Types { get; set; }
    }
}
