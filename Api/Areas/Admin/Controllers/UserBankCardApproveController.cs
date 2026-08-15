using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تایید کارت بانکی کاربر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserBankCardApproveController : ControllerBase
    {
        private readonly IUserBankCardService UserBankCardService;

        public UserBankCardApproveController(IUserBankCardService UserBankCardService)
        {
            this.UserBankCardService = UserBankCardService;
        }
        /// <summary>
        /// تایید آیتم
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<UserBankCardApproveDto>), 200)]
        public async Task<IActionResult> Put(UserBankCardApproveDto dto)
        {
            var result = await UserBankCardService.UpdateUserBankCardApproveAsyncDto(dto);
            return Ok(result);
        }
    }
}
