using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کارت های بانکی کاربر ها
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserBankCardController : ControllerBase
    {
        private readonly IUserBankCardService UserBankCardService;
        private readonly ICurrentUserHelper _currentUser;

        public UserBankCardController(IUserBankCardService UserBankCardService, ICurrentUserHelper currentUser)
        {
            this.UserBankCardService = UserBankCardService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var UserBankCard = await UserBankCardService.FindAsyncVDto(id);
            if (UserBankCard.IsSuccess && UserBankCard.Data?.UserId != _currentUser.CurrentUser.UserId)
                return Ok(new BaseResultDto<UserBankCardVDto>(false, Resource.Notification.AccessDenied, default));

            return Ok(UserBankCard);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserBankCardSearchDto), 200)]
        public IActionResult Get([FromQuery] UserBankCardInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var UserBankCard = UserBankCardService.Search(dto);
            return Ok(UserBankCard);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public async Task<IActionResult> Post(UserBankCardDto userBankCardDto)
        {
            userBankCardDto.UserId = _currentUser.CurrentUser.UserId;
            var result = await UserBankCardService.InsertAsyncDto(userBankCardDto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public async Task<IActionResult> Put(UserBankCardDto userBankCardDto)
        {
            userBankCardDto.UserId = _currentUser.CurrentUser.UserId;
            var result = await UserBankCardService.UpdateAsyncDto(userBankCardDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await UserBankCardService.FindAsyncVDto(id);
            if (!existing.IsSuccess || existing.Data?.UserId != _currentUser.CurrentUser.UserId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var result = UserBankCardService.DeleteDto(id);
            return Ok(result);
        }
    }
}
