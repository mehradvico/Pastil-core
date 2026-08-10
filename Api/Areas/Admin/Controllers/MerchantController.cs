using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Order.MerchantSrv.Dto;
using Application.Services.Order.MerchantSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// درگاه بانکی
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class MerchantController : ControllerBase
    {
        private IMerchantService MerchantService;
        public MerchantController(IMerchantService MerchantService)
        {
            this.MerchantService = MerchantService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<MerchantDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await MerchantService.FindAsyncDto(id);
            if (role.Data != null)
            {
                role.Data.Username = null;
                role.Data.Password = null;
                role.Data.PrivateKey = null;
                role.Data.TerminalKey = null;
                role.Data.MerchantNo = null;
            }
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
            var searchDto = MerchantService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<MerchantDto>), 200)]
        public async Task<IActionResult> Post(MerchantDto MerchantDto)
        {

            var dto = await MerchantService.InsertAsyncDto(MerchantDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(MerchantDto MerchantDto)
        {
            var dto = await MerchantService.UpdateSecureAsyncDto(MerchantDto);
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
            var dto = MerchantService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
