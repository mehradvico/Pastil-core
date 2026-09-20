using Application.Common.Dto.Result;
using Application.Services.Accounting.UserTokenSrv.Dto;
using Entities.Entities.Security;
using System;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserTokenSrv.Iface
{
    public interface IUserTokenService
    {
        UserTokenDto CreateToken(
            User user,
            bool isAdmin = false,
            bool rememberMe = false,
            DateTime? refreshTokenExpiresAt = null,
            long? rotatedFromTokenId = null,
            string deviceName = null);
        Task<BaseResultDto> RefreshTokenAsync(RefreshTokenDto refreshToken);
        /// <summary>حذف ردیف‌های توکنی که بیش از ۳۰ روز از انقضای refresh‌شان گذشته؛ تعداد حذف‌شده را برمی‌گرداند.</summary>
        Task<int> PurgeExpiredAsync();
        Task<BaseResultDto> SignOut(string token);
        Task<BaseResultDto> ResetTokenAsync(
            User user,
            bool isAdmin = false,
            bool rememberMe = false,
            string deviceId = null,
            bool revokeOnlySameClientKind = false);
    }
}
