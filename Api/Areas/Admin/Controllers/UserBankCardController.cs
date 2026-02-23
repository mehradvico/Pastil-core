using Application.Common.Dto.Result;
using Application.Services.Accounting.UserBankCardSrv.Dto;
using Application.Services.Accounting.UserBankCardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت کارت های بانکی کاربر ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserBankCardController : ControllerBase
    {
        private readonly IUserBankCardService UserBankCardService;

        public UserBankCardController(IUserBankCardService UserBankCardService)
        {
            this.UserBankCardService = UserBankCardService;
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
            var UserBankCard = UserBankCardService.Search(dto);
            return Ok(UserBankCard);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public async Task<IActionResult> Post(UserBankCardDto UserBankCardDto)
        {
            var result = await UserBankCardService.InsertAsyncDto(UserBankCardDto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public IActionResult Put(UserBankCardDto userBankCardDto)
        {
            var result = UserBankCardService.UpdateDto(userBankCardDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = UserBankCardService.DeleteDto(id);
            return Ok(result);
        }
    }
}
