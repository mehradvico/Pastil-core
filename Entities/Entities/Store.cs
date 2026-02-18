using Entities.Entities.CommonField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Store : Seo_Full_Field
    {
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public Point Location { get; set; }
        public long TypeId { get; set; }
        public long CityId { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public DateTime CreateDate { get; set; }
        public int MaxDiscountPercent { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public decimal CommissionPercent { get; set; } 
        public Picture Picture { get; set; }
        public Picture Icon { get; set; }
        public Code Type { get; set; }
        public City City { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<StoreComment> StoreComments { get; set; }
    }
}
