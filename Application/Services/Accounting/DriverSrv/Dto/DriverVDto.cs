using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;

namespace Application.Services.Accounting.DriverSrv.Dto
{
    public class DriverVDto : Name_FieldDto
    {
        public long OwnerId { get; set; }
        public string Phone { get; set; }
        public string Vehicle { get; set; }
        public string LicensePlateNumber { get; set; }
        public string OwnerDetail { get; set; }
        public bool Active { get; set; }
        public int Rate { get; set; }
        public long? ProfilePictureId { get; set; }
        public long? CertificatePictureId { get; set; }
        public long? VehicleCardPictureId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public long StatusId { get; set; }
        public string AdminDetail { get; set; }
        public bool Approved { get; set; }
        public long? VehicleTypeId { get; set; }
        public CityVDto City { get; set; }
        public NeighborhoodVDto Neighborhood { get; set; }
        public PictureVDto ProfilePicture { get; set; }
        public PictureVDto CertificatePicture { get; set; }
        public PictureVDto VehicleCardPicture { get; set; }
        public UserMinVDto Owner { get; set; }
        public CodeVDto Status { get; set; }
        public CodeVDto VehicleType { get; set; }
    }
}
