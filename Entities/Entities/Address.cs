using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;

namespace Entities.Entities
{
    public class Address : Name_Field
    {
        public long UserId { get; set; }
        public long CityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string AddressValue { get; set; }
        public Point Location { get; set; }
        public string PostalCode { get; set; }
        public string NationalCode { get; set; }
        public bool Deleted { get; set; }
        public bool IsSelected { get; set; }
        public User User { get; set; }
        public City City { get; set; }
    }
}
