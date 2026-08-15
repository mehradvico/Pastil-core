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

                return _userService
                    .GetByTokenDto(token)
                    .GetAwaiter()
                    .GetResult();

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
