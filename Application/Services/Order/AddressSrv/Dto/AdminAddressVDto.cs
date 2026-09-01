using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Dto;
using Application.Services.LocationFields.CitySrv.Dto;

namespace Application.Services.Order.AddressSrv.Dto
{
    public class AdminAddressVDto : Name_FieldDto
    {
        public long UserId { get; set; }
        public long CityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string AddressValue { get; set; }
        public PointDto Location { get; set; }
        public string PostalCode { get; set; }
        public string NationalCode { get; set; }
        public bool IsSelected { get; set; }
        public UserVDto User { get; set; }
        public CityVDto City { get; set; }
    }
}
