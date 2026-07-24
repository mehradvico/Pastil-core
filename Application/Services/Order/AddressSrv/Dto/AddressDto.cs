using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Application.Services.Order.AddressSrv.Dto
{
    public class AddressDto : Name_FieldDto
    {
        [Display(Name = nameof(Resource.Field.City), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseSelectT1))]
        public long CityId { get; set; }
        [Display(Name = nameof(Resource.Field.Name), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string FirstName { get; set; }
        [Display(Name = nameof(Resource.Field.LastName), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string LastName { get; set; }
        public string Phone { get; set; }
        [Display(Name = nameof(Resource.Field.Mobile), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Mobile { get; set; }
        [Display(Name = nameof(Resource.Field.AddressValue), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string AddressValue { get; set; }
        public PointDto Location { get; set; }
        public string PostalCode { get; set; }
        public string NationalCode { get; set; }
        [IgnoreDataMember]
        public long UserId { get; set; }

    }
}
