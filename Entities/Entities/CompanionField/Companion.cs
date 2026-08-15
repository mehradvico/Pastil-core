using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using Entities.Entities.LocationField;
using Entities.Entities.PansionField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Companion : Seo_Full_Field
    {
        public bool IsPersonal { get; set; }
        public long OwnerId { get; set; }
        public string ReferralCode { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public DateTime? SilverAccountCreateDate { get; set; }
        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Deleted { get; set; }
        public string AddressValue { get; set; }
        public string Phone { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public string SearchKey { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
        public Point Location { get; set; }
        public Picture Picture { get; set; }
        public Picture BackgroundPicture { get; set; }
        public Picture Icon { get; set; }
        public User Owner { get; set; }
        public ICollection<CompanionType> CompanionTypes { get; set; }
        public ICollection<CompanionAssistance> CompanionAssistances { get; set; }
        public ICollection<CompanionUser> CompanionUsers { get; set; }
        public ICollection<CompanionReport> CompanionReports { get; set; }
        public ICollection<CompanionInsurancePackage> CompanionInsurancePackages { get; set; }
        public ICollection<CompanionPet> CompanionPets { get; set; }
        public ICollection<CompanionComment> CompanionComments { get; set; }
        public ICollection<Pansion> Pansions { get; set; }
        public ICollection<CompanionZone> CompanionZones { get; set; }


    }
}
