using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Order.BankSrv.Dto;
using Application.Services.Order.BankSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  بانک ها
    /// </summary>
    /// <parent>
    /// content
    /// </parent>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class BankController : ControllerBase
    {
        private IBankService BankService;
        /// <summary>
        ///  بانک ها
        /// </summary>
        ///
        public BankController(IBankService BankService)
        {
            this.BankService = BankService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<BankDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await BankService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = BankService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<BankDto>), 200)]
        public async Task<IActionResult> Post(BankDto BankDto)
        {

            var dto = await BankService.InsertAsyncDto(BankDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(BankDto BankDto)
        {
            var dto = BankService.UpdateDto(BankDto);
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
            var dto = BankService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
