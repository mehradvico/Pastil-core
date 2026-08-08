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
            DateTime? refreshTokenExpiresAt = null);
        Task<BaseResultDto> RefreshTokenAsync(RefreshTokenDto refreshToken);
        Task<BaseResultDto> SignOut(string token);
        Task<BaseResultDto> ResetTokenAsync(
            User user,
            bool isAdmin = false,
            bool rememberMe = false);
    }
}
