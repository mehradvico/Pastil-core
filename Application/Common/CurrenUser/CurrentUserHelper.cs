using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.Common.CurrentUser
{

    public class CurrentUserHelper : ICurrentUserHelper
    {
        private readonly IUserService _userService;
        private const string CacheKey = "Pastil.CurrentUser";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserHelper(IUserService userService, IHttpContextAccessor httpContext)
        {
            _httpContextAccessor = httpContext;
            _userService = userService;
        }
        public CurrentUserDto CurrentUser
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.User?.Identity?.IsAuthenticated != true)
                    return null;

                var token = GetAccessToken(context);
                if (string.IsNullOrWhiteSpace(token))
                    return null;

                // این Property در هر درخواست بارها خوانده می‌شود و هر بار یک کوئری سنگین با
                // چندین Include + انتظار هم‌زمان (sync-over-async) اجرا می‌کرد. نتیجه را
                // یک‌بار برای همان درخواست نگه می‌داریم (کلید = خود توکن، پس عوض‌شدن توکن
                // وسط یک درخواست کش قدیمی برنمی‌گرداند).
                if (context.Items.TryGetValue(CacheKey, out var cached)
                    && cached is (string cachedToken, CurrentUserDto cachedUser)
                    && cachedToken == token)
                {
                    return cachedUser;
                }

                var user = _userService
                    .GetByTokenDto(token)
                    .GetAwaiter()
                    .GetResult();
                if (user != null)
                    context.Items[CacheKey] = (token, user);
                return user;

            }
        }

        private static string GetAccessToken(HttpContext context)
        {
            try
            {
                var authenticationToken = context
                    .GetTokenAsync("access_token")
                    .GetAwaiter()
                    .GetResult();

                if (!string.IsNullOrWhiteSpace(authenticationToken))
                    return authenticationToken;
            }
            catch (InvalidOperationException)
            {
                // سرویس‌هایی مانند Payment درخواست Callback ناشناس دارند و
                // عمداً Authentication Scheme در آن‌ها ثبت نشده است.
            }

            var authorization = context.Request.Headers.Authorization.ToString();
            const string bearerPrefix = "Bearer ";

            return authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                ? authorization[bearerPrefix.Length..].Trim()
                : null;
        }
    }
}
