using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Order.DeliveryDistanceSrv.Dto;
using Application.Services.Order.DeliveryDistanceSrv.iface;
using Application.Services.Order.DeliverySrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مسافت حمل نقل
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryDistanceController : ControllerBase
    {
        private IDeliveryDistanceService DeliveryDistanceService;
        public DeliveryDistanceController(IDeliveryDistanceService DeliveryDistanceService)
        {
            this.DeliveryDistanceService = DeliveryDistanceService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryDistanceDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await DeliveryDistanceService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] DeliveryDistanceInputDto dto)
        {
            var searchDto = DeliveryDistanceService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryDistanceDto>), 200)]
        public async Task<IActionResult> Post(DeliveryDistanceDto DeliveryDistanceDto)
        {

            var dto = await DeliveryDistanceService.InsertAsyncDto(DeliveryDistanceDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(DeliveryDistanceDto DeliveryDistanceDto)
        {
            var dto = DeliveryDistanceService.UpdateDto(DeliveryDistanceDto);
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
            var dto = DeliveryDistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
