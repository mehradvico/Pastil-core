using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.Security;

namespace Entities.Entities
{
    public class Driver : Name_Field
    {
        public long OwnerId { get; set; }
        public string Phone { get; set; }

        public string Vehicle { get; set; }
        public string LicensePlateNumber { get; set; }
        public string OwnerDetail { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int Rate { get; set; }
        public long? ProfilePictureId { get; set; }
        public long? CertificatePictureId { get; set; }
        public long? VehicleCardPictureId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public long StatusId { get; set; }
        public string AdminDetail { get; set; }
        public bool Approved { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
        public Picture ProfilePicture { get; set; }
        public Picture CertificatePicture { get; set; }
        public Picture VehicleCardPicture { get; set; }
        public User Owner { get; set; }
        public Code Status { get; set; }
    }
}
