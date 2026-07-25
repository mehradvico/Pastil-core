using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreCommentSrv.Dto;
using Application.Services.ProductSrvs.StoreCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرات فروشگاه
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreCommentController : ControllerBase
    {
        private IStoreCommentService _storeCommentService;
        public StoreCommentController(IStoreCommentService storeCommentService)
        {
            this._storeCommentService = storeCommentService;
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
            var dto = _storeCommentService.UpdateDto(storeCommentDto);
            return Ok(dto);
        }

    }
}
