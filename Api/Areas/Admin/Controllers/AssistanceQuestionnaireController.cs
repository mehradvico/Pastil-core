using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceQuestionnaireSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceQuestionnaireSrv.Iface;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;


namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  پرسشنامه های خدمات
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistanceQuestionnaireController : ControllerBase
    {
        private readonly IAssistanceQuestionnaireService _assistancequestionnaireService;
        public AssistanceQuestionnaireController(IAssistanceQuestionnaireService assistancequestionnaireService)
        {
            this._assistancequestionnaireService = assistancequestionnaireService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(AssistanceQuestionnaireSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceQuestionnaireInputDto dto)
        {
            var search = _assistancequestionnaireService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پرسشنامه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceQuestionnaireDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _assistancequestionnaireService.FindAsyncDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceQuestionnaireDto>), 200)]
        public async Task<IActionResult> Post(AssistanceQuestionnaireDto dto)
        {
            var result = await _assistancequestionnaireService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(AssistanceQuestionnaireDto dto)
        {
            var agency = _assistancequestionnaireService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _assistancequestionnaireService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
