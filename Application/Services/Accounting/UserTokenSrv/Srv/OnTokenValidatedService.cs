using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Security;
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
                // endpoint عمومی: نشست نامعتبر = مهمان؛ نوشتن ۴۰۱ و ادامه‌ی pipeline اتصال را قطع می‌کرد
                if (AnonymousEndpoint.Allows(context.HttpContext)) return;
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }
            var claimsidentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsidentity?.Claims == null || !claimsidentity.Claims.Any())
            {
                context.NoResult();
                // endpoint عمومی: نشست نامعتبر = مهمان؛ نوشتن ۴۰۱ و ادامه‌ی pipeline اتصال را قطع می‌کرد
                if (AnonymousEndpoint.Allows(context.HttpContext)) return;
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }
            var userId = claimsidentity.FindFirst("UserId")?.Value;
            if (!long.TryParse(userId, out long userLong))
            {
                context.NoResult();
                // endpoint عمومی: نشست نامعتبر = مهمان؛ نوشتن ۴۰۱ و ادامه‌ی pipeline اتصال را قطع می‌کرد
                if (AnonymousEndpoint.Allows(context.HttpContext)) return;
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
                return;
            }

            //long storeId = currentUserHelper.CurrentUser.StoreId;
            string area = (string)context.HttpContext.Request.RouteValues["area"] ?? null;
            string controller = (string)context.HttpContext.Request.RouteValues["controller"] ?? null;
            string action = (string)context.HttpContext.Request.RouteValues["action"] ?? null;
            long? tokenRoleId = long.TryParse(claimsidentity.FindFirst("RoleId")?.Value, out var parsedRoleId) ? parsedRoleId : null;
            var (userCheck, isSuperAdmin) = await userService.CheckUserWithIdentityAsync(Token.UnsafeToString(), userLong, area, controller, action, tokenRoleId);
            if (!userCheck.IsSuccess)
            {
                context.NoResult();
                // endpoint عمومی: نشست نامعتبر = مهمان؛ نوشتن ۴۰۱ و ادامه‌ی pipeline اتصال را قطع می‌کرد
                if (AnonymousEndpoint.Allows(context.HttpContext)) return;
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

            // ادمین اصلی فقط از همین‌جا (بعد از تطبیق با رکورد دیتابیس، نه فقط claim توکن) نشانه می‌گیرد؛ policyهای «AdminArea» و
            // «SuperAdminOnly» و فیلتر Hangfire به همین نشانه (fail-closed) تکیه می‌کنند و کلاینت نمی‌تواند آن را جعل کند.
            if (isSuperAdmin)
            {
                context.HttpContext.Items[SuperAdminIdentity.VerifiedItemKey] = true;
                return;
            }

            // هر نقش دیگری (از جمله «مدیر») روی ناحیه‌ی Admin فقط وقتی به اینجا می‌رسد که CheckUser مجوز RolePermission همین
            // controller/action را تأیید کرده باشد؛ policy «AdminArea» به همین نشانه (fail-closed) تکیه می‌کند.
            if (string.Equals(area, "admin", System.StringComparison.OrdinalIgnoreCase))
                context.HttpContext.Items["Pastil.AdminAreaPermissionVerified"] = true;
        }
    }
}
