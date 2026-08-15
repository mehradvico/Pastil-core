using System.Xml.Linq;
using Api.Authorization;
using Application.Common.Dto.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.Reflection.Dto;
using Utility.Reflection.Iface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// همگام سازی دسترسی ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class PermissionSyncController : ControllerBase
    {
        private readonly IControllerActionDiscoveryService controllerActionDiscoveryService;

        public PermissionSyncController(
            IControllerActionDiscoveryService controllerActionDiscoveryService)
        {
            this.controllerActionDiscoveryService = controllerActionDiscoveryService;
        }

        /// <summary>
        /// همگام سازی دسترسی های پنل مدیریت با کنترلرها
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PermissionSyncResultDto), 200)]
        public async Task<IActionResult> Post(CancellationToken cancellationToken)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "MehradVico.Api.xml");
            var xmlComments = XDocument.Load(xmlPath);
            var result = await controllerActionDiscoveryService.SynchronizePermissionsAsync(
                typeof(PermissionSyncController).Assembly,
                xmlComments,
                cancellationToken);

            if (result.UnmappedControllers.Count > 0)
            {
                var controllers = string.Join("، ", result.UnmappedControllers);
                return BadRequest(new BaseResultDto<PermissionSyncResultDto>(
                    false,
                    $"کنترلرهای بدون گروه دسترسی پیدا شدند: {controllers}",
                    result));
            }

            return Ok(new BaseResultDto<PermissionSyncResultDto>(true, result));
        }
    }
}
