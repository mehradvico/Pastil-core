using Application.Common.Dto.Result;
using Application.Services.Accounting.PermissionSrv.Dto;
using Api.Authorization;
using Application.Services.Accounting.PermissionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using Utility.Reflection.Iface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// دسترسی ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private IPermissionService permissionService;
        private readonly IControllerActionDiscoveryService controllerActionDiscoveryService;
        public PermissionController(
            IPermissionService permissionService,
            IControllerActionDiscoveryService controllerActionDiscoveryService)
        {
            this.permissionService = permissionService;
            this.controllerActionDiscoveryService = controllerActionDiscoveryService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PermissionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await permissionService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(PermissionSearchDto), 200)]
        public IActionResult Get([FromQuery] PermissionInputDto dto)
        {
            var searchDto = permissionService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PermissionDto>), 200)]
        public async Task<IActionResult> Post(PermissionDto permissionDto)
        {
            var dto = await permissionService.InsertAsyncDto(permissionDto);
            return Ok(dto);
        }

        /// <summary>
        /// همگام سازی دسترسی های پنل مدیریت با کنترلرها
        /// </summary>
        [HttpPost("sync")]
        [Authorize(Policy = PolicyNames.AdminOnly)]
        public async Task<IActionResult> Synchronize(CancellationToken cancellationToken)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "MehradVico.Api.xml");
            var xmlComments = XDocument.Load(xmlPath);
            var result = await controllerActionDiscoveryService.SynchronizePermissionsAsync(
                typeof(PermissionController).Assembly,
                xmlComments,
                cancellationToken);

            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PermissionDto permissionDto)
        {
            var dto = permissionService.UpdateDto(permissionDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = permissionService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
