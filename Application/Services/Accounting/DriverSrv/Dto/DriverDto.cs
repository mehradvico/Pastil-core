using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Application.Services.Accounting.DriverSrv.Dto
{
    public class DriverDto : Id_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Name), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Name { get; set; }
        public long OwnerId { get; set; }

        [Display(Name = nameof(Resource.Field.Phone1), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Phone { get; set; }

        [Display(Name = nameof(Resource.Field.Veichle), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Vehicle { get; set; }

        [Display(Name = nameof(Resource.Field.LicensePlateNumber), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
  ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
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
    }
}
