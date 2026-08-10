using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionSrv.Dto
{
    public class PansionDto : Name_FieldDto
    {
        public bool? IsSchool { get; set; }
        public long CompanionId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approve { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public string Discription { get; set; }
        public string AddressValue { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public long? PictureId { get; set; }
        public bool Suggested { get; set; }
        public double PansionPrice { get; set; }
        public double SchoolPrice { get; set; }
        public string Regulations { get; set; }
        public string OpenHour { get; set; }
        public string CloseHour { get; set; }
    }
}
