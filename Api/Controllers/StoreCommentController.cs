using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Services.ProductSrvs.StoreCommentSrv.Iface;
using Application.Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با فروشگاه ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class StoreCommentController : ControllerBase
    {
        private IStoreCommentService _storeCommentService;
        /// <summary>
        /// مرتبط با پست ها
        /// </summary>
        private readonly ICurrentUserHelper _currentUser;
        public StoreCommentController(IStoreCommentService storeCommentService, ICurrentUserHelper currentUser)
        {
            this._storeCommentService = storeCommentService;
            _currentUser = currentUser;
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(StoreCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] StoreCommentInputDto dto)
        {
            dto.Available = true;
            var post = _storeCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        // قبلاً ناشناس بود و UserId را از بدنه می‌پذیرفت (جعل هویت/spam)؛ حالا فقط با لاگین و با UserId از توکن.
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseResultDto<StoreCommentDto>), 200)]
        public async Task<IActionResult> Post(StoreCommentDto storeComment)
        {
            storeComment.UserId = _currentUser.CurrentUser.UserId;
            var dto = await _storeCommentService.InsertAsyncDto(storeComment);
            return Ok(dto);
        }
    }
}
