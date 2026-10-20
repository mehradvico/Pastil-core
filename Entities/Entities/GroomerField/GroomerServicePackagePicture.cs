using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    public class GroomerServicePackagePicture : Id_Field
    {
        public long GroomerServicePackageId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public GroomerServicePackage GroomerServicePackage { get; set; }
        public Picture Picture { get; set; }
    }
}
