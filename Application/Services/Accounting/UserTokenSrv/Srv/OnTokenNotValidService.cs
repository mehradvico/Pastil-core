using Application.Common.Dto.Result;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserTokenSrv.Srv
{
    public class OnTokenNotValidService : IOnTokenNotValidService
    {
        public async Task Execute(AuthenticationFailedContext context)
        {
            context.NoResult();
            // endpoint عمومی: توکن خراب نادیده گرفته می‌شود و درخواست مهمان ادامه دارد
            if (AnonymousEndpoint.Allows(context.HttpContext)) return;
            context.Response.StatusCode = 401;
            context.Response.ContentType = "text/plain";
            context.Response.WriteAsync(JsonSerializer.Serialize(new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken))).Wait();
            await Task.CompletedTask;
        }
    }
}
