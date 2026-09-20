using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Accounting.UserTokenSrv.Dto;
using Application.Services.Accounting.UserTokenSrv.Iface;
using AutoMapper;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserTokenSrv.Srv
{
    public class UserTokenService : IUserTokenService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly Application.Common.Security.ISecurityAudit _audit;

        public UserTokenService(IDataBaseContext _context, IConfiguration configuration, IMapper mapper, Application.Common.Security.ISecurityAudit audit)
        {
            this._audit = audit;
            this._context = _context;
            this.configuration = configuration;
            this.mapper = mapper;
        }
        public UserTokenDto CreateToken(
            User user,
            bool isAdmin = false,
            bool rememberMe = false,
            DateTime? refreshTokenExpiresAt = null,
            long? rotatedFromTokenId = null,
            string deviceName = null)
        {
            var userToken = CreateUserTokenDto(user, isAdmin);
            var now = DateTime.UtcNow;
            var refreshExpiration = refreshTokenExpiresAt ??
                now.AddMinutes(GetRefreshExpirationMinutes(user, isAdmin, rememberMe));
            var tokenExpiration = now.AddMinutes(Convert.ToInt32(userToken.TokenExpires));

            if (tokenExpiration > refreshExpiration)
                tokenExpiration = refreshExpiration;

            var claims = new List<Claim>
                {
                    new Claim ("UserId", userToken.UserId.ToString()),
                    new Claim ("FirstName",  userToken.FirstName??" "),
                    new Claim ("LastName",  userToken.LastName??" "),
                    new Claim ("RoleId",  userToken.RoleId.ToString()),
                };
            string key = userToken.JwtKey;
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: userToken.Issuer,
                audience: userToken.Audience,
                expires: tokenExpiration,
                notBefore: now,
                claims: claims,
                signingCredentials: credentials
                );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            var item = mapper.Map<UserToken>(userToken);
            item.TokenExp = tokenExpiration;
            item.RefreshTokenExp = refreshExpiration;
            item.CreateDate = now;
            item.Deleted = false;
            item.RefreshTokenHash = refreshToken.Tosha256Hash();
            item.TokenHash = jwtToken.Tosha256Hash();
            item.RotatedFromTokenId = rotatedFromTokenId;
            if (deviceName != null)
                item.DeviceName = deviceName;
            _context.UserTokens.Add(item);
            _context.SaveChanges();
            var result = new UserTokenDto()
            {
                UserId = userToken.UserId,
                RefreshToken = refreshToken,
                Token = jwtToken,
                FirstName = userToken.FirstName,
                LastName = userToken.LastName,
                TokenExp = item.TokenExp,
                RefreshTokenExp = item.RefreshTokenExp
            };
            return result;

        }

        public async Task<BaseResultDto> RefreshTokenAsync(RefreshTokenDto refreshToken)
        {
            await using var transaction = await _context.BeginTransactionAsync(
                IsolationLevel.Serializable);

            var hashedToken = refreshToken.Token.Tosha256Hash();
            var hashedRefreshToken = refreshToken.RefreshToken.Tosha256Hash();
            var userToken = await _context.UserTokens.Include(s => s.User).ThenInclude(s => s.Role).FirstOrDefaultAsync(s => s.TokenHash.Equals(hashedToken) && s.RefreshTokenHash == hashedRefreshToken && s.RefreshTokenExp > DateTime.UtcNow && s.Deleted != true);
            if (userToken != null)
            {
                // نشست پنل به دستگاهی که ساخته‌اش گره خورده: refresh از دستگاه دیگر = توکن کپی شده → کل نشست‌های پنل
                // این کاربر باطل می‌شود (همان سیاست «سرقت توکن»).
                if (!PanelSessionDevice.Matches(userToken.DeviceName, refreshToken.DeviceId))
                {
                    await RevokePanelSessionsAsync(userToken.UserId);
                    await transaction.CommitAsync();
                    _audit.Failure("RefreshTokenTheft", userToken.UserId, detail: "panel_device_mismatch_sessions_revoked");
                    return new BaseResultDto(false, val: Resource.Notification.SessionRevokedTokenReuseDetected);
                }

                await _context.UserTokens
                    .Where(x => x.Id == userToken.Id && !x.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(token => token.Deleted, true));

                var isPanelSession = PanelSessionDevice.IsPanelSession(userToken.DeviceName) || refreshToken.IsAdmin;
                var createdDto = CreateToken(
                    userToken.User,
                    isPanelSession,
                    rememberMe: !isPanelSession || WasRemembered(userToken, userToken.User),
                    rotatedFromTokenId: userToken.Id,
                    deviceName: userToken.DeviceName);
                await transaction.CommitAsync();
                return new BaseResultDto<UserTokenDto>(true, createdDto);
            }

            // No live match — figure out whether this refresh token is simply
            // expired/unknown, or was already rotated away by an earlier,
            // legitimate refresh. The latter usually means this exact refresh
            // token got used twice, which only happens if it was copied/shared:
            // the real client already moved on to the token that replaced it.
            var rotatedAway = await _context.UserTokens
                .FirstOrDefaultAsync(s => s.RefreshTokenHash == hashedRefreshToken && s.Deleted == true);
            if (rotatedAway != null)
            {
                // اما همین رفتار می‌تواند کاملاً بی‌گناه هم باشد: چند تب باز، یا تلاش
                // هم‌زمان چند درخواست از همان کلاینت واقعی (مثلاً بازیابی خودکار سشن در
                // فرانت) می‌توانند هر دو دقیقاً همین یک رفرش‌توکن را با هم بفرستند. آن که
                // اول می‌رسد Rotate را انجام می‌دهد؛ دومی همان توکنِ لحظه‌ای‌پیش رفرش‌شده را
                // می‌فرستد و بدون این بررسی، به‌غلط «سرقت» تشخیص داده می‌شد و کل سشن کاربر
                // (شامل همان توکن تازه‌ی معتبر) نابود می‌شد. اگر جایگزینِ همین توکن ظرف چند
                // ثانیه‌ی اخیر ساخته شده، این را یک رقابت بی‌ضرر در نظر می‌گیریم، نه سرقت
                // واقعی — و به‌جای نابودی سشن، یک توکن تازه‌ی دیگر (هم‌زنجیره) صادر می‌کنیم.
                var wasRotatedWithinGracePeriod = await _context.UserTokens
                    .AnyAsync(s => s.RotatedFromTokenId == rotatedAway.Id &&
                                   s.CreateDate >= DateTime.UtcNow - TokenRotationPolicy.GracePeriod);
                if (wasRotatedWithinGracePeriod && PanelSessionDevice.Matches(rotatedAway.DeviceName, refreshToken.DeviceId))
                {
                    var owner = await _context.Users
                        .Include(s => s.Role)
                        .FirstOrDefaultAsync(s => s.Id == rotatedAway.UserId);
                    if (owner != null)
                    {
                        var isPanelSession = PanelSessionDevice.IsPanelSession(rotatedAway.DeviceName) || refreshToken.IsAdmin;
                        var reissuedDto = CreateToken(
                            owner,
                            isPanelSession,
                            rememberMe: !isPanelSession || WasRemembered(rotatedAway, owner),
                            rotatedFromTokenId: rotatedAway.Id,
                            deviceName: rotatedAway.DeviceName);
                        await transaction.CommitAsync();
                        return new BaseResultDto<UserTokenDto>(true, reissuedDto);
                    }
                }

                // بیرون از بازه‌ی Grace Period — همان رفتار قبلی: احتمال سرقت واقعی جدی
                // گرفته می‌شود، پس تمام سشن‌های فعال کاربر نابود می‌شوند.
                await _context.UserTokens
                    .Where(x => x.UserId == rotatedAway.UserId && !x.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(token => token.Deleted, true));
                await transaction.CommitAsync();
                _audit.Failure("RefreshTokenTheft", rotatedAway.UserId, detail: "reuse_outside_grace_all_sessions_revoked");
                return new BaseResultDto(false, val: Resource.Notification.SessionRevokedTokenReuseDetected);
            }

            await transaction.RollbackAsync();
            return new BaseResultDto(false, val: Resource.Notification.TokenExpired);
        }

        // بازه‌ای که در آن، استفاده‌ی مجدد از یک رفرش‌توکنِ همین‌الان Rotate‌شده، سرقت
        // واقعی در نظر گرفته نمی‌شود بلکه یک رقابت (race) بی‌ضرر بین درخواست‌های هم‌زمانِ
        // همان کلاینت واقعی فرض می‌شود — نگاه کنید به استفاده‌اش در RefreshTokenAsync بالا.
        private CreateUserTokenDto CreateUserTokenDto(User user, bool isAdmin = false)
        {
            var createToken = new CreateUserTokenDto()
            {
                UserId = user.Id,
                RoleId = user.RoleId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Issuer = configuration["JWtConfig:issuer"],
                Audience = configuration["JWtConfig:audience"],
                JwtKey = configuration["JWtConfig:key"],
                TokenExpires = user.Role.Label == RoleEnum.Admin.ToString() && isAdmin ? configuration["JWtConfig:expires_admin_minutes"] : configuration["JWtConfig:expires_user_minutes"],
                RefreshExpires = user.Role.Label == RoleEnum.Admin.ToString() && isAdmin ? configuration["JWtConfig:refresh_expires_admin_minutes"] : configuration["JWtConfig:refresh_expires_user_minutes"],
                Provider = "windows",
                DeviceName = "zand"
            };
            return createToken;
        }

        // پنجره‌ی refresh نشست «بدون مرا به خاطر بسپار» (idle timeout)؛ نشست ماندگار پنجره‌ای بلندتر دارد.
        private int GetNonPersistentRefreshMinutes(User user)
        {
            var configuredValue = user.Role.Label == RoleEnum.Admin.ToString()
                ? configuration["JWtConfig:refresh_expires_admin_minutes"]
                : configuration["JWtConfig:refresh_expires_user_minutes"];
            return Convert.ToInt32(configuredValue);
        }

        // نشستِ پنل ماندگار (remember-me) بوده اگر طول پنجره‌ی refreshش از پنجره‌ی غیرماندگار بیشتر باشد
        // (ستون جدیدی برای «به خاطر بسپار» نداریم؛ از همین اختلاف طول استنباط می‌شود).
        private bool WasRemembered(UserToken token, User user)
        {
            var nonPersistent = GetNonPersistentRefreshMinutes(user);
            return (token.RefreshTokenExp - token.CreateDate).TotalMinutes > nonPersistent + 1;
        }

        private async Task RevokePanelSessionsAsync(long userId)
        {
            await _context.UserTokens
                .Where(x => x.UserId == userId && !x.Deleted && x.DeviceName != null && x.DeviceName.StartsWith(PanelSessionDevice.Prefix))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(token => token.Deleted, true));
        }

        private int GetRefreshExpirationMinutes(
            User user,
            bool isAdmin,
            bool rememberMe)
        {
            // پنل + «مرا به خاطر بسپار»: پنجره‌ی لغزنده‌ی ماندگار ولی معقول (پیش‌فرض ۷ روز = همان وعده‌ی UI صفحه‌ی ورود پنل، قبلاً یک سال).
            if (isAdmin && rememberMe)
            {
                return int.TryParse(
                    configuration["JWtConfig:admin_persistent_session_minutes"],
                    out var adminPersistentMinutes) && adminPersistentMinutes > 0
                    ? adminPersistentMinutes
                    : 7 * 24 * 60;
            }

            if (!isAdmin)
            {
                return int.TryParse(
                    configuration["JWtConfig:persistent_session_minutes"],
                    out var persistentMinutes)
                    ? persistentMinutes
                    : 7 * 24 * 60;
            }

            var configuredValue = user.Role.Label == RoleEnum.Admin.ToString()
                ? configuration["JWtConfig:refresh_expires_admin_minutes"]
                : configuration["JWtConfig:refresh_expires_user_minutes"];

            return Convert.ToInt32(configuredValue);
        }
        public async Task<int> PurgeExpiredAsync()
        {
            // ۳۰ روز نگهداری بعد از انقضا: بسیار بیشتر از پنجره‌ی تشخیص «استفاده‌ی مجدد از refresh token» (چند ثانیه) است
            var cutoff = DateTime.UtcNow.AddDays(-30);
            return await _context.UserTokens
                .Where(token => token.RefreshTokenExp < cutoff)
                .ExecuteDeleteAsync();
        }

        public async Task<BaseResultDto> SignOut(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var hashed = token.Tosha256Hash();
                var userToken = await _context.UserTokens.FirstOrDefaultAsync(x => x.TokenHash == hashed);
                if (userToken != null)
                {
                    userToken.Deleted = true;
                    _context.UserTokens.Update(userToken);
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            return new BaseResultDto(isSuccess: false, Resource.Notification.InvalidToken);
        }


        public async Task<BaseResultDto> ResetTokenAsync(
            User user,
            bool isAdmin = false,
            bool rememberMe = false,
            string deviceId = null,
            bool revokeOnlySameClientKind = false)
        {
            await using var transaction = await _context.BeginTransactionAsync(
                IsolationLevel.Serializable);

            // ورود عادی (وب‌اپ/اپ) قبلاً «همه‌ی» نشست‌های کاربر را می‌کشت؛ یعنی ادمینی که با همان حساب در وب‌اپ یا
            // اپ موبایل وارد می‌شد، نشست پنلش می‌پرید. حالا ورود فقط نشست‌های هم‌نوع را می‌بندد (پنل ↔ پنل، غیرپنل ↔ غیرپنل).
            // مسیرهای حساس (ریست رمز، تغییر موبایل...) همچنان revokeOnlySameClientKind=false و همه را می‌بندند.
            var revocable = _context.UserTokens.Where(x => x.UserId == user.Id && !x.Deleted);
            if (revokeOnlySameClientKind)
            {
                revocable = isAdmin
                    ? revocable.Where(x => x.DeviceName != null && x.DeviceName.StartsWith(PanelSessionDevice.Prefix))
                    : revocable.Where(x => x.DeviceName == null || !x.DeviceName.StartsWith(PanelSessionDevice.Prefix));
            }
            await revocable.ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.Deleted, true));

            var newToken = CreateToken(
                user,
                isAdmin,
                rememberMe,
                deviceName: isAdmin ? PanelSessionDevice.BuildName(deviceId) : null);
            await transaction.CommitAsync();
            return new BaseResultDto<UserTokenDto>(isSuccess: true, newToken);
        }
    }
}
