using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.Accounting.RoleSrv.Iface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;


namespace Application.Services.Accounting.UserTokenSrv.Srv
{
    public class OnTokenValidatedService : IOnTokenValidatedService
    {
        private IUserService userService;
        private IRoleService roleService;
        private IUserTokenService userTokenSevice;
        private ICurrentUserHelper currentUserHelper;

        public OnTokenValidatedService(IUserService userService, IRoleService roleService, IUserTokenService userTokenSevice, ICurrentUserHelper currentUserHelper)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.userTokenSevice = userTokenSevice;
            this.currentUserHelper = currentUserHelper;
        }

        public async Task Execute(TokenValidatedContext context)
        {

            if (!(context.SecurityToken is JsonWebToken Token))
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }
            var claimsidentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsidentity?.Claims == null || !claimsidentity.Claims.Any())
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }
            var userId = claimsidentity.FindFirst("UserId")?.Value;
            if (!long.TryParse(userId, out long userLong))
            {
                context.NoResult();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }

            //long storeId = currentUserHelper.CurrentUser.StoreId;
            string area = (string)context.HttpContext.Request.RouteValues["area"] ?? null;
            string controller = (string)context.HttpContext.Request.RouteValues["controller"] ?? null;
            string action = (string)context.HttpContext.Request.RouteValues["action"] ?? null;
            var userCheck = await userService.CheckUser(Token.UnsafeToString(), userLong, area, controller, action /*storeId*/);
            if (!userCheck.IsSuccess)
            {
                context.NoResult();
                // «دسترسی ندارید» با «نشست/توکن نامعتبر» فرق دارد: کلاینت‌ها (پنل) روی ۴۰۱ تلاش refresh و در
                // نهایت خروج انجام می‌دهند؛ برای کاربری که فقط به یک endpoint مجوز ندارد باید ۴۰۳ برگردد
                // تا نشست سالمش با هر poll پریشان (rotate/پاک) نشود.
                var isPermissionDenied = userCheck.Messages?.Any(message =>
                    message?.Item1 == Resource.Notification.YouHaveNotPermission) == true;
                context.Response.StatusCode = isPermissionDenied ? 403 : 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(userCheck)).Wait();
                return;
            }
        }
    }
}
