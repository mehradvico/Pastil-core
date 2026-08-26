using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Services.ProductSrvs.StoreCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت نظرات فروشگاه ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreCommentController : ControllerBase
    {
        private IStoreCommentService _storeCommentService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت نظرات فروشگاه ها
        /// </summary>
        ///
        public StoreCommentController(IStoreCommentService storeCommentService, ICurrentUserHelper currentUser)
        {
            this._storeCommentService = storeCommentService;
            this._currentUser = currentUser;    
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoreCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _storeCommentService.FindAsyncDto(id);
            if (role.IsSuccess && role.Data?.StoreId != _currentUser.CurrentUser.StoreId)
                return Ok(new BaseResultDto<StoreCommentDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StoreCommentDto>), 200)]
        public IActionResult Get([FromQuery] StoreCommentInputDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            var searchDto = _storeCommentService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ویرایش نظرات
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(StoreCommentDto storeCommentDto)
        {
            storeCommentDto.StoreId = _currentUser.CurrentUser.StoreId;
            var dto = _storeCommentService.UpdateDto(storeCommentDto);
            return Ok(dto);
        }

    }
}
