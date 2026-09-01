using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.Security
{
    public class UserToken : Id_Field
    {
        public string Provider { get; set; }
        public string TokenHash { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime TokenExp { get; set; }
        public string RefreshTokenHash { get; set; }
        public DateTime RefreshTokenExp { get; set; }
        public string DeviceName { get; set; }
        public bool Deleted { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Id of the UserToken this one replaced via refresh-token rotation.
        /// Null for a token created at sign-in. Used to detect refresh-token
        /// reuse: if a request ever presents a refresh token whose row is
        /// already Deleted (i.e. it was already rotated away), that's strong
        /// evidence the token was copied/shared, and every active token for
        /// the user gets revoked.
        /// </summary>
        public long? RotatedFromTokenId { get; set; }
    }
}
