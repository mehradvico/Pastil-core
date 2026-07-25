using Application.Common.Dto.Result;
using Application.Services.Accounting.RolePermission.Dto;
using Application.Services.Accounting.RolePermission.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// دسترسی های نقش
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[Controller]")]
    [ApiController]
    [Authorize]
    public class RolePermission : ControllerBase
    {
        private IRolePermissionService rolePermissionService;
        public RolePermission(IRolePermissionService rolePermissionService)
        {
            this.rolePermissionService = rolePermissionService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>  
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Get(long roleId)
        {
            var dto = await rolePermissionService.FindAsyncDto(roleId);
            return Ok(dto);
        }
        /// <summary>
        /// اضافه و ویرایش دسترسی های نقش
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(RolePermissionDto dto)
        {
            var result = await rolePermissionService.InsertAndUpdateAsyncDto(dto);
            return Ok(result);
        }
    }
}
