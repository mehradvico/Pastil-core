using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    public class ClinicServicePackagePicture : Id_Field
    {
        public long ClinicServicePackageId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public ClinicServicePackage ClinicServicePackage { get; set; }
        public Picture Picture { get; set; }
    }
}
