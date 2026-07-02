using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.DeliverySrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت حمل و نقل
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private IDeliveryService DeliveryService;
        /// <summary>
        /// مدیریت حمل و نقل
        /// </summary>
        ///
        public DeliveryController(IDeliveryService DeliveryService)
        {
            this.DeliveryService = DeliveryService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await DeliveryService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] DeliveryInputDto dto)
        {
            var searchDto = DeliveryService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryDto>), 200)]
        public async Task<IActionResult> Post(DeliveryDto DeliveryDto)
        {

            var dto = await DeliveryService.InsertAsyncDto(DeliveryDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(DeliveryDto DeliveryDto)
        {
            var dto = DeliveryService.UpdateDto(DeliveryDto);
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
            var dto = DeliveryService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
