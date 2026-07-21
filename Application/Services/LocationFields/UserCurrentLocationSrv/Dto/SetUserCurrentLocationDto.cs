using Application.Common.Dto.LocationPoint;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Dto
{
    public class SetUserCurrentLocationDto
    {
        [Required]
        public PointDto Location { get; set; }
    }
}
