using Entities.Entities.CommonField;
using Entities.Entities.CompanionField;
using Entities.Entities.PastilClubField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.Security
{
    public class User : Id_Field
    {
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NaturalCode { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string Password { get; set; }
        public bool Locked { get; set; }
        public bool Deleted { get; set; }
        public long RoleId { get; set; }
        public string ReferralCode { get; set; }
        public string RequestCode { get; set; }
        public string Expertise { get; set; }
        public int RequestCodeTryCount { get; set; }
        public bool IsFemale { get; set; }
        public long? PictureId { get; set; }
        public double CurrentScore { get; set; }
        public DateTime CreateDate { get; set; }
        public Role Role { get; set; }
        public ICollection<UserToken> UserTokens { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<ProductOrder> ProductOrders { get; set; }
        public ICollection<Rebate> Rebates { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<UserProduct> UserProducts { get; set; }
        public ICollection<ProductLike> ProductLikes { get; set; }
        public ICollection<CompanionUser> CompanionUsers { get; set; }
        public ICollection<Store> Stores { get; set; }
        public ICollection<Companion> Companions { get; set; }
        public Driver Driver { get; set; }
        public Picture Picture { get; set; }
        public UserCurrentLocation UserCurrentLocation { get; set; }
        public ClubPointAccount ClubPointAccount { get; set; }
    }
}
