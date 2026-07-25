using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreUserSrv.Dto;
using Application.Services.StoreSrvs.StoreUserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران فروشگاه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreUserController : ControllerBase
    {
        private readonly IStoreUserService _storeUserService;
        public StoreUserController(IStoreUserService storeUserService)
        {
            this._storeUserService = storeUserService;
        }
        /// <summary>
        /// جستجو 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Get([FromQuery] StoreUserDto storeUser)
        {
            var role = await _storeUserService.GetAllAsync(storeUser);
            return Ok(role);
        }
        /// <summary>
        ///  آیتم جدید
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(StoreUserDto storeUser)
        {
            var role = await _storeUserService.InsertAsync(storeUser);
            return Ok(role);
        }
        /// <summary>
        ///  حذف آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(StoreUserDto storeUser)
        {
            var role = await _storeUserService.RemoveAsync(storeUser);
            return Ok(role);
        }

    }
}
