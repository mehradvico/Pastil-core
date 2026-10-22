using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت  پانسیون ها
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class PansionController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        public PansionController(IPansionService PansionService)
        {
            this._PansionService = PansionService;
        }


        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionInputDto dto)
        {
            dto.Approve = true;
            dto.Available = true;
            dto.ShowToSite = true; // فقط پانسیونی که ادمین برای نمایش در سایت علامت زده (برای داده‌ی قدیمی: BackfillPansionShowToSite.sql)
            var search = _PansionService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پانسیون</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Pansion = await _PansionService.FindAsyncVDto(id);
            // جزئیات عمومی فقط برای پانسیونِ تأییدشده و فعال (مثل فهرست عمومی)
            if (Pansion.IsSuccess && Pansion.Data != null && (!Pansion.Data.Active || !Pansion.Data.Approve || !Pansion.Data.ShowToSite))
                return Ok(new BaseResultDto<PansionVDto>(false, Resource.Notification.NothingFound, null));
            return Ok(Pansion);
        }
    }
}
