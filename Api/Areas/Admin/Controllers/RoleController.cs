using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Accounting.RoleSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نقش ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private IRoleService roleService;
        public RoleController(IRoleService role)
        {
            roleService = role;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<RoleDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await roleService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>

        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(BaseSearchDto<RoleDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var roles = roleService.Search(dto);
            return Ok(roles);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<RoleDto>), 200)]
        public async Task<IActionResult> Post(RoleDto roleDto)
        {
            var dto = await roleService.InsertAsyncDto(roleDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(RoleDto roleDto)
        {
            var dto = roleService.UpdateDto(roleDto);
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
            var dto = roleService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
