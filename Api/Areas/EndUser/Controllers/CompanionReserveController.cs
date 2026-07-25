using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت رزرو نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveController : ControllerBase
    {
        private readonly ICompanionReserveService _companionReserveService;
        private readonly CurrentUserDto _currentUserHelper;
        public CompanionReserveController(ICompanionReserveService companionReserveService, ICurrentUserHelper currentUserHelper)
        {
            this._companionReserveService = companionReserveService;
            this._currentUserHelper = currentUserHelper.CurrentUser;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionReserveSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReserveInputDto dto)
        {
            dto.BookerId = _currentUserHelper.UserId;
            var search = _companionReserveService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه رزرو نماینده</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var companion = await _companionReserveService.FindAsyncVDto(id);
            return Ok(companion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveDto>), 200)]
        public async Task<IActionResult> Post(CompanionReserveDto dto)
        {
            dto.BookerId = _currentUserHelper.UserId;
            var result = await _companionReserveService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CompanionReserveDto dto)
        {
            dto.BookerId = _currentUserHelper.UserId;
            var companion = _companionReserveService.UpdateDto(dto);
            return Ok(companion);
        }
    }
}
