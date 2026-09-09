using Entities.Entities.CommonField;

namespace Entities.Entities.SchoolField
{
    public class SchoolPicture : Id_Field
    {
        public long SchoolId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public School School { get; set; }
        public Picture Picture { get; set; }
    }
}
