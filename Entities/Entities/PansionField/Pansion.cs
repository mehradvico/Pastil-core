using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PansionField
{
    public class Pansion : Name_Field
    {
        public bool? IsSchool { get; set; }
        public long CompanionId { get; set; }
        public bool Active {  get; set; }
        public bool ShowToSite { get; set; }
        public bool Approve { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; } 
        public string Discription {  get; set; }
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
        public decimal DailyCommissionPercent { get; set; }
        public decimal HourlyCommissionPercent { get; set; }
        public Companion Companion { get; set; }
        public State State { get; set; }
        public City City { get; set; }  
        public Picture Picture { get; set; }
        public ICollection<PansionPet> PansionPets { get; set; }
        public ICollection<PansionComment> PansionComments { get; set; }
        public ICollection<PansionPicture> PansionPictures { get; set; }
    }
}
