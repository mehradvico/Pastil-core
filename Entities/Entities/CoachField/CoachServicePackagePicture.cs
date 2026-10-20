using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    public class CoachServicePackagePicture : Id_Field
    {
        public long CoachServicePackageId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public CoachServicePackage CoachServicePackage { get; set; }
        public Picture Picture { get; set; }
    }
}
