using Application.Common.Dto.Result;
using Application.Services.Accounting.PermissionSrv.Dto;
using Application.Services.Accounting.PermissionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// منوی پنل مدیریت
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionMenuController : ControllerBase
    {
        private IPermissionService permissionService;
        /// <summary>
        /// منوی پنل مدیریت
        /// </summary>
        ///
        public PermissionMenuController(IPermissionService permissionService)
        {
            this.permissionService = permissionService;
        }
        /// <summary>
        ///  دریافت همه 
        /// </summary>

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PermissionMenuDto>), 200)]
        //[OutputCache(Duration = 180, VaryByHeaderNames =["Authorization"])]
        public IActionResult Get()
        {
            var menu = permissionService.Menu();
            return Ok(menu);
        }


        /// <summary>
        ///  دسترسی‌های action ناحیه Admin برای نقش کاربر جاری
        /// </summary>
        [HttpGet("Actions")]
        [ProducesResponseType(typeof(BaseResultDto<PermissionActionsDto>), 200)]
        public IActionResult Actions()
        {
            return Ok(permissionService.MyAdminActions());
        }
    }
}
